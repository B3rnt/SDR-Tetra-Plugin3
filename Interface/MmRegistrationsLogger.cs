using System;
using System.Collections.Generic;
using System.IO;

namespace SDRSharp.Tetra
{
    internal static class MmRegistrationsLogger
    {
        private static readonly object _lock = new object();
        private static DateTime _sessionStart = DateTime.UtcNow;

        public static void Log(Dictionary<GlobalNames, int> data, string message)
        {
            if (!Global.LogMmRegistrations) return;

            // Determine LA (location area) from current parsed data if available.
            int la = 0;
            if (data != null && data.TryGetValue(GlobalNames.Location_Area, out var laVal))
                la = laVal;

            // Time since session start (matches example '00:00:xx')
            var ts = DateTime.UtcNow - _sessionStart;
            if (ts.TotalHours >= 99) ts = TimeSpan.FromHours(ts.TotalHours % 100);
            var timeStr = ts.ToString(@"hh\:mm\:ss");

            // Spacing rule: 3-digit LA => two spaces; 4+ => one space
            string spaceBeforeLa = (la >= 1000) ? " " : "  ";

            var line = $" {timeStr}{spaceBeforeLa}[LA: {la}]   {message}";

            var folder = Global.LogWriteFolder;
            if (string.IsNullOrWhiteSpace(folder))
                folder = AppDomain.CurrentDomain.BaseDirectory;

            var filename = $"TETRA_MM-Registrations_{DateTime.Now:yyyy-MM-dd}.txt";
            var path = Path.Combine(folder, filename);

            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.AppendAllText(path, line + Environment.NewLine);
                }
            }
            catch
            {
                // Intentionally swallow to not break decoding.
            }
        }
    }
}
