using System;
using SDRSharp.Radio;

namespace SDRSharp.Tetra
{
    /// <summary>
    /// Holds *per mast* DSP + decoder state so multiple carriers can be monitored at once.
    /// </summary>
    public sealed class MastReceiver
    {
        public MastConfig Config { get; }
        public bool ListenAudio { get; set; }

        public ComplexFifoStream Fifo { get; } = new ComplexFifoStream(BlockMode.None);
        public DdcDownconverter Downconverter { get; private set; }
        public SimpleAgc Agc { get; } = new SimpleAgc();
        public Demodulator Demod { get; } = new Demodulator();
        public TetraDecoder Decoder { get; }

        // Runtime status
        public int LostBuffers { get; set; }
        public string StatusText { get; set; } = "Idle";

        private long _offsetHz;

        public MastReceiver(MastConfig cfg, TetraPanel host)
        {
            Config = cfg ?? throw new ArgumentNullException(nameof(cfg));
            Decoder = new TetraDecoder(host);

            // Per-mast AGC tuning defaults from config
            Agc.TargetRms = cfg.AgcTargetRms;
            Agc.Attack = cfg.AgcAttack;
            Agc.Decay = cfg.AgcDecay;
        }

        public bool Enabled => Config.Enabled;

        public void EnsureDownconverter(double fs)
        {
            // Target output around 96 kHz (keeps CPU low, and is fine for symbol timing recovery)
            int decim = (int)Math.Round(fs / 96000.0);
            if (decim < 1) decim = 1;

            if (Downconverter == null || Downconverter.Decimation != decim)
            {
                Downconverter = new DdcDownconverter(fs, decim, cutoffHz: 15000.0, taps: 129);
                // Re-apply last known offset if we have one
                Downconverter.SetOffset(_offsetHz, (int)Math.Round(fs));
            }
        }

        public void SetFrequency(long frequencyHz, int sampleRate, long centerFrequencyHz)
        {
            Config.FrequencyHz = frequencyHz;
            _offsetHz = frequencyHz - centerFrequencyHz;
            Downconverter?.SetOffset(_offsetHz, sampleRate);
        }

        public string GetStatusString()
        {
            var off = _offsetHz;
            string offStr = off == 0 ? "0" : (off > 0 ? "+" + off.ToString() : off.ToString());
            return Enabled ? $"off {offStr} Hz" : "disabled";
        }
    }
}
