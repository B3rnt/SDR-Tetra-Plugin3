
using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SDRSharp.Tetra
{
    /// <summary>
    /// Registers SDR# stream hooks exactly once and broadcasts IQ/Audio buffers
    /// to any number of subscribers. This avoids SDR# freezing when hooks are
    /// added/removed while the radio is running.
    /// </summary>
    public sealed unsafe class SharedStreamHub : IDisposable
    {
        public delegate void IQReadyDelegate(Complex* buffer, double samplerate, int length);
        public delegate void AudioReadyDelegate(float* buffer, double samplerate, int length);

        public event IQReadyDelegate IQReady;
        public event AudioReadyDelegate AudioReady;

        private static readonly object _sync = new object();
        private static readonly Dictionary<ISharpControl, SharedStreamHub> _instances = new Dictionary<ISharpControl, SharedStreamHub>();

        private readonly ISharpControl _control;
        private readonly IFProcessor _ifProcessor;
        private readonly AudioProcessor _audioProcessor;

        private bool _isDisposed;

        private SharedStreamHub(ISharpControl control)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));

            _ifProcessor = new IFProcessor();
            _ifProcessor.IQReady += OnIQ;

            _audioProcessor = new AudioProcessor();
            _audioProcessor.AudioReady += OnAudio;

            // Full-band IQ. (ProcessorType)0 is used by several plugins (e.g. AuxVFO) to hook raw IQ.
            _control.RegisterStreamHook(_ifProcessor, (ProcessorType)0);
            _control.RegisterStreamHook(_audioProcessor, ProcessorType.DemodulatorOutput);

            _ifProcessor.Enabled = true;
            _audioProcessor.Enabled = true;
        }

        private double TryGetSampleRate()
        {
            try
            {
                // Use reflection so we don't depend on a specific SDR# API version.
                var t = _control.GetType();
                var p = t.GetProperty("SampleRate", BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(double))
                {
                    return (double)p.GetValue(_control);
                }
                // Some builds expose it as int
                if (p != null && p.PropertyType == typeof(int))
                {
                    return Convert.ToDouble(p.GetValue(_control));
                }
            }
            catch { }
            return 0.0;
        }

        private void OnIQ(Complex* buffer, double ignoredSamplerate, int length)
        {
            var sr = TryGetSampleRate();
            if (sr <= 0) sr = ignoredSamplerate;
            IQReady?.Invoke(buffer, sr, length);
        }

        private void OnAudio(float* buffer, double samplerate, int length)
        {
            AudioReady?.Invoke(buffer, samplerate, length);
        }

        public static SharedStreamHub GetOrCreate(ISharpControl control)
        {
            lock (_sync)
            {
                if (!_instances.TryGetValue(control, out var hub))
                {
                    hub = new SharedStreamHub(control);
                    _instances[control] = hub;
                }
                return hub;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                _ifProcessor.IQReady -= OnIQ;
                _audioProcessor.AudioReady -= OnAudio;
            }
            catch { }

            lock (_sync)
            {
                _instances.Remove(_control);
            }
        }
    }
}
