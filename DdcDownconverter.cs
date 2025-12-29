
using SDRSharp.Radio;
using System;

namespace SDRSharp.Tetra
{
    /// <summary>
    /// Simple wideband -> channel DDC: complex mixer + FIR lowpass + integer decimator.
    /// Designed for TETRA carriers inside the current SDR bandwidth.
    /// </summary>
    public unsafe sealed class DdcDownconverter
    {
        private readonly int _decim;
        private readonly float[] _taps;
        private readonly Complex[] _delay;
        private int _delayPos;
        private int _decimCount;

        private double _phase;
        private readonly double _twoPi = 2.0 * Math.PI;

        public int Decimation => _decim;
        public int TapCount => _taps.Length;

        public DdcDownconverter(double inputSampleRate, int decimation, double cutoffHz = 15000.0, int taps = 129)
        {
            if (decimation < 1) decimation = 1;
            _decim = decimation;

            if (taps < 33) taps = 33;
            if (taps % 2 == 0) taps += 1; // odd for symmetry

            _taps = DesignLowpass(inputSampleRate, cutoffHz, taps);
            _delay = new Complex[_taps.Length];
            _delayPos = 0;
            _decimCount = 0;
            _phase = 0.0;
        }

        public void ResetNco() => _phase = 0.0;

        /// <summary>
        /// Process wideband IQ, extracting a narrowband channel at offsetHz from center.
        /// Writes decimated samples into output buffer (must have at least length/decim + 4).
        /// Returns number of output samples written.
        /// </summary>
        public int Process(Complex* input, int length, double inputSampleRate, double offsetHz, Complex* output, int outputCapacity)
        {
            if (length <= 0) return 0;
            if (outputCapacity <= 0) return 0;

            // NCO increment
            double phaseInc = _twoPi * offsetHz / inputSampleRate;
            int outLen = 0;

            for (int i = 0; i < length; i++)
            {
                // Mix to baseband: x * e^{-j*phase}
                double c = Math.Cos(_phase);
                double s = Math.Sin(_phase);
                _phase += phaseInc;
                if (_phase > Math.PI) _phase -= _twoPi;
                else if (_phase < -Math.PI) _phase += _twoPi;

                Complex mixed;
                mixed.Real = (float)(input[i].Real * c + input[i].Imag * s);
                mixed.Imag = (float)(-input[i].Real * s + input[i].Imag * c);

                // push into delay line (circular)
                _delay[_delayPos] = mixed;
                _delayPos++;
                if (_delayPos == _delay.Length) _delayPos = 0;

                // decimate
                if (_decimCount == 0)
                {
                    // FIR: dot(taps, delayline) with newest at delayPos-1
                    double accR = 0.0;
                    double accI = 0.0;

                    int di = _delayPos;
                    for (int t = 0; t < _taps.Length; t++)
                    {
                        // walk backwards in delay line
                        di--;
                        if (di < 0) di = _delay.Length - 1;

                        float h = _taps[t];
                        accR += _delay[di].Real * h;
                        accI += _delay[di].Imag * h;
                    }

                    if (outLen < outputCapacity)
                    {
                        output[outLen].Real = (float)accR;
                        output[outLen].Imag = (float)accI;
                        outLen++;
                    }
                    else
                    {
                        // output buffer full, stop
                        _decimCount = 0;
                        return outLen;
                    }

                    _decimCount = _decim - 1;
                }
                else
                {
                    _decimCount--;
                }
            }

            return outLen;
        }

        private static float[] DesignLowpass(double fs, double fc, int taps)
        {
            // Windowed-sinc lowpass (Hamming)
            // fc: cutoff in Hz (one-sided), fs: input sample rate
            double norm = fc / fs; // 0..0.5
            if (norm > 0.49) norm = 0.49;
            if (norm < 1e-6) norm = 1e-6;

            int m = taps - 1;
            double mid = m / 2.0;

            var h = new float[taps];
            double sum = 0.0;

            for (int n = 0; n < taps; n++)
            {
                double x = n - mid;
                double sinc;
                if (Math.Abs(x) < 1e-9)
                    sinc = 2.0 * norm;
                else
                    sinc = Math.Sin(2.0 * Math.PI * norm * x) / (Math.PI * x);

                // Hamming window
                double w = 0.54 - 0.46 * Math.Cos(2.0 * Math.PI * n / m);

                double v = sinc * w;
                h[n] = (float)v;
                sum += v;
            }

            // normalize DC gain
            if (Math.Abs(sum) > 1e-12)
            {
                for (int n = 0; n < taps; n++)
                    h[n] = (float)(h[n] / sum);
            }

            return h;
        }
    }
}
