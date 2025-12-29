
using SDRSharp.Radio;
using System;

namespace SDRSharp.Tetra
{
    internal unsafe sealed class MultiStageDecimator
    {
        private readonly int[] _stages;
        private readonly ComplexFirFilter[] _filters;
        private readonly Complex[][] _stageBuf;
        private readonly int _maxIn;

        // Simple complex FIR filter wrapper
        private unsafe sealed class ComplexFirFilter
        {
            private readonly float[] _h;
            private readonly Complex[] _delay;
            private int _idx;

            public ComplexFirFilter(float[] h)
            {
                _h = h;
                _delay = new Complex[h.Length];
                _idx = 0;
            }

            public Complex Process(Complex x)
            {
                _delay[_idx] = x;
                float accRe = 0, accIm = 0;
                int di = _idx;
                for (int i = 0; i < _h.Length; i++)
                {
                    var d = _delay[di];
                    var c = _h[i];
                    accRe += d.Real * c;
                    accIm += d.Imag * c;
                    if (--di < 0) di = _delay.Length - 1;
                }
                if (++_idx >= _delay.Length) _idx = 0;
                return new Complex(accRe, accIm);
            }
        }

        public MultiStageDecimator(double inputRate, double outputRate, int maxInput)
        {
            _maxIn = Math.Max(1024, maxInput);
            if (outputRate <= 0 || inputRate <= 0) throw new ArgumentException("Invalid sample rate.");
            var ratio = inputRate / outputRate;
            var iratio = (int)Math.Round(ratio);

            if (Math.Abs(ratio - iratio) > 1e-6)
            {
                // Not an integer ratio. Caller should avoid this by choosing a sample rate that is a multiple of 25 kHz.
                // We still allow it but will behave like nearest integer.
                iratio = Math.Max(1, iratio);
            }

            // Factor ratio into small stages (<=8) to keep FIR sizes reasonable.
            _stages = Factor(iratio);
            _filters = new ComplexFirFilter[_stages.Length];
            _stageBuf = new Complex[_stages.Length][];

            double stageIn = inputRate;
            for (int s = 0; s < _stages.Length; s++)
            {
                int d = _stages[s];
                double stageOut = stageIn / d;

                // Lowpass cutoff ~ 0.45 * stageOut (keeps aliasing down)
                double cutoff = 0.45 * stageOut;
                _filters[s] = new ComplexFirFilter(MakeLowpass(stageIn, cutoff, 63));
                _stageBuf[s] = new Complex[_maxIn]; // reused
                stageIn = stageOut;
            }
        }

        private static int[] Factor(int n)
        {
            // Prefer stages 8, 6, 5, 4, 3, 2
            int[] prefs = new[] { 8, 6, 5, 4, 3, 2 };
            var tmp = new System.Collections.Generic.List<int>();
            foreach (var p in prefs)
            {
                while (n % p == 0)
                {
                    tmp.Add(p);
                    n /= p;
                }
            }
            while (n > 1)
            {
                // whatever remains
                tmp.Add(n);
                break;
            }
            return tmp.ToArray();
        }

        private static float[] MakeLowpass(double fs, double fc, int taps)
        {
            if (taps % 2 == 0) taps++;
            var h = new float[taps];
            int m = taps / 2;
            double norm = 2.0 * fc / fs;
            double sum = 0;
            for (int i = 0; i < taps; i++)
            {
                int n = i - m;
                double sinc = (n == 0) ? 1.0 : Math.Sin(Math.PI * norm * n) / (Math.PI * norm * n);
                // Hann window
                double w = 0.5 - 0.5 * Math.Cos(2.0 * Math.PI * i / (taps - 1));
                double v = norm * sinc * w;
                h[i] = (float)v;
                sum += v;
            }
            // normalize DC gain
            for (int i = 0; i < taps; i++) h[i] = (float)(h[i] / sum);
            return h;
        }

        public int Process(Complex* input, int inputLen, Complex* output, int maxOut)
        {
            // copy input into stage0 buffer by filtering+decim
            int len = inputLen;
            Complex* inPtr = input;
            Complex[] tmpBuf = null;

            for (int s = 0; s < _stages.Length; s++)
            {
                int d = _stages[s];
                var filter = _filters[s];
                int outLen = 0;

                // Use a managed intermediate buffer per stage to keep code simpler.
                tmpBuf = _stageBuf[s];
                for (int i = 0; i < len; i++)
                {
                    var y = filter.Process(inPtr[i]);
                    if ((i % d) == 0)
                    {
                        if (outLen < tmpBuf.Length) tmpBuf[outLen++] = y;
                    }
                }

                // For next stage, set inPtr to a temporary unmanaged view
                // We'll pin by copying into output at final stage only.
                // Instead, we reuse output pointer only at the end.
                // Here we create a fixed pointer via stackalloc for small sizes is unsafe; avoid.
                // We'll just write back into a scratch unmanaged buffer by copying into output when at last stage.
                // For simplicity: at each stage after first, we treat tmpBuf as input array and copy into output buffer for pointer access.
                len = outLen;
                // copy tmpBuf into output temporarily (as scratch) if not last stage
                if (s < _stages.Length - 1)
                {
                    int copyLen = Math.Min(len, maxOut);
                    for (int j = 0; j < copyLen; j++) output[j] = tmpBuf[j];
                    inPtr = output;
                }
            }

            // final tmpBuf contains decimated signal
            int finalLen = Math.Min(len, maxOut);
            for (int i = 0; i < finalLen; i++) output[i] = tmpBuf[i];
            return finalLen;
        }
    }
}
