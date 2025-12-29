
using SDRSharp.Radio;
using System;

namespace SDRSharp.Tetra
{
    /// <summary>
    /// Lightweight per-channel AGC to keep decoder stable when running multiple carriers.
    /// </summary>
    public unsafe sealed class SimpleAgc
    {
        public float TargetRms { get; set; } = 0.20f;
        public float Gain { get; private set; } = 1.0f;

        // Larger Attack reacts faster to overload; Decay reacts slower when signal is weak
        public float Attack { get; set; } = 0.02f;
        public float Decay  { get; set; } = 0.002f;

        public void Reset(float gain = 1.0f) => Gain = gain;

        public void Process(Complex* buffer, int length)
        {
            if (length <= 0) return;

            double p = 0.0;
            for (int i = 0; i < length; i++)
            {
                double r = buffer[i].Real;
                double im = buffer[i].Imag;
                p += r * r + im * im;
            }

            double rms = Math.Sqrt(p / Math.Max(1, length));
            if (rms < 1e-12) return;

            float desired = (float)(TargetRms / rms);
            float k = desired < Gain ? Attack : Decay;
            Gain = Gain + k * (desired - Gain);

            float g = Gain;
            for (int i = 0; i < length; i++)
            {
                buffer[i].Real *= g;
                buffer[i].Imag *= g;
            }
        }
    }
}
