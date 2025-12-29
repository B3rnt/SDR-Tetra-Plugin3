
using SDRSharp.Radio;
using System;

namespace SDRSharp.Tetra
{
= "Mast";
        public long FrequencyHz { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public sealed class MastReceiver
    {
        public MastConfig Config { get; }
        public bool ListenAudio { get; set; }

        public ComplexFifoStream Fifo { get; } = new ComplexFifoStream(BlockMode.None);
        public DdcDownconverter Downconverter { get; private set; }
        public SimpleAgc Agc { get; } = new SimpleAgc();
        public Demodulator Demod { get; } = new Demodulator();
        public TetraDecoder Decoder { get; }

        // Status
        public int LostBuffers { get; set; }
        public string StatusText { get; set; } = "Idle";

        public MastReceiver(MastConfig cfg, TetraPanel host)
        {
            Config = cfg;
            Decoder = new TetraDecoder(host);
        }

        public void EnsureDownconverter(double fs)
        {
            // Target output around 96 kHz (lets demod run without interpolation in most cases)
            int decim = (int)Math.Round(fs / 96000.0);
            if (decim < 1) decim = 1;

            if (Downconverter == null || Downconverter.Decimation != decim)
                Downconverter = new DdcDownconverter(fs, decim, cutoffHz: 15000.0, taps: 129);
        }
        public void SetFrequency(long frequencyHz, int sampleRate, long centerFrequencyHz)
        {
            if (Config != null) Config.FrequencyHz = frequencyHz;
            _offsetHz = frequencyHz - centerFrequencyHz;
            Downconverter?.SetOffset(_offsetHz, sampleRate);
        }

        public string GetStatusString()
        {
            // lightweight status string; decoder already produces sync events for active mast.
            var off = _offsetHz;
            string offStr = off == 0 ? "0" : (off > 0 ? "+" + off.ToString() : off.ToString());
            return Enabled ? $"off {offStr} Hz" : "disabled";
        }

    }
}
