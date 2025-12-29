using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace SDRSharp.Tetra
{
    /// <summary>
    /// Separate MM registrations logfile writer.
    /// Named differently to avoid conflicts with existing forks.
    /// </summary>
    internal static class MmRegLogWriter
    {
        private static readonly Stopwatch _sw = Stopwatch.StartNew();
        private static DateTime _currentDay = DateTime.MinValue.Date;
        private static string _currentPath;

        public static void TryLog(MmInfo info, TetraSettings settings)
        {
            if (!Global.LogMmRegistrations) return;
            if (info == null || string.IsNullOrEmpty(info.Message)) return;

            int la = info.LocationArea ?? -1;
            int ssi = info.Ssi ?? -1;

            string time = TimeSpan.FromMilliseconds(_sw.ElapsedMilliseconds).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

            // spacing rule: 3-digit LA => two spaces, 4+ => one space
            string laStr = la >= 0 ? la.ToString(CultureInfo.InvariantCulture) : "0";
            string padBetween = (laStr.Length <= 3) ? "  " : " ";

            string prefix = " " + time + padBetween + "[LA: " + laStr + "]   ";

            string line = prefix + info.Message;
            if (ssi >= 0 && !line.Contains("SSI:"))
            {
                // Put SSI at the end if message didn't already include it
                line += ": SSI: " + ssi.ToString(CultureInfo.InvariantCulture);
            }

            WriteLine(line, settings);
        }

        private static void WriteLine(string line, TetraSettings settings)
        {
            var day = DateTime.Now.Date;
            if (day != _currentDay || string.IsNullOrEmpty(_currentPath))
            {
                _currentDay = day;
                string folder = settings != null ? settings.LogWriteFolder : string.Empty;
                if (string.IsNullOrWhiteSpace(folder))
                {
                    folder = AppDomain.CurrentDomain.BaseDirectory;
                }
                Directory.CreateDirectory(folder);
                _currentPath = Path.Combine(folder, "TETRA_MM-Registrations_" + day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".txt");
            }

            try
            {
                File.AppendAllText(_currentPath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MM log write failed: " + ex.Message);
            }
        }
    }
}