using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace SDRSharp.Tetra
{
    internal static class MmRegistrationsLogger
    {
        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        // We keep last known LA per channel in case message doesn't carry it
        public static void TryLog(LogicChannel ch, Dictionary<GlobalNames,int> d)
        {
            if (!Global.LogMmRegistrations) return;
            try
            {
                if (d == null || d.Count == 0) return;
                if (!d.TryGetValue(GlobalNames.MM_PDU_Type, out var mmPdu)) return;

                // LA: prefer decoded field, else current cell LA if present in dict
                int la = -1;
                if (d.TryGetValue(GlobalNames.Location_Area, out var laVal)) la = laVal;

                // SSI: prefer MM_SSI
                int ssi = -1;
                if (d.TryGetValue(GlobalNames.MM_SSI, out var mmSsi)) ssi = mmSsi;
                else if (d.TryGetValue(GlobalNames.SSI, out var ssiVal)) ssi = ssiVal;

                // Only log when we have at least LA and SSI
                if (la < 0 || ssi < 0) return;

                string msg = BuildMessage((MmPduType)mmPdu, d, ssi, la);
                if (string.IsNullOrWhiteSpace(msg)) return;

                var t = _sw.Elapsed;
                string time = $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";

                // spacing rule: 2 spaces if 3-digit LA, else 1 space
                string between = (la >= 100 && la <= 999) ? "  " : " ";
                string line = $" {time}{between}[LA: {la}]   {msg}";

                var folder = Global.CurrentLogWriteFolder;
                if (string.IsNullOrWhiteSpace(folder))
                    folder = AppDomain.CurrentDomain.BaseDirectory;

                Directory.CreateDirectory(folder);
                string file = Path.Combine(folder, $"TETRA_MM-Registrations_{DateTime.Now:yyyy-MM-dd}.txt");
                File.AppendAllText(file, line + Environment.NewLine);
            }
            catch
            {
                // never break decoder due to logging
            }
        }

        private static string BuildMessage(MmPduType type, Dictionary<GlobalNames,int> d, int ssi, int la)
        {
            // These strings are matched to your sample logfile.
            switch (type)
            {
                case MmPduType.D_AUTHENTICATION:
                    // sub-type: 2 bits
                    if (d.TryGetValue(GlobalNames.Authentication_sub_type, out var sub))
                    {
                        // 0/1: BS demands authentication; 2/3: result to MS authentication (DL)
                        // This mirrors typical traffic in sample.
                        if (sub == 0 || sub == 1)
                            return $"BS demands authentication: SSI: {ssi}";
                        // For result we try map status text
                        string status = "Authentication successful or no authentication currently in progress";
                        return $"BS result to MS authentication: {status} SSI: {ssi} - {status}";
                    }
                    return $"BS demands authentication: SSI: {ssi}";

                case MmPduType.D_LOCATION_UPDATE_ACCEPT:
                    {
                        string authStatus = "Authentication successful or no authentication currently in progress";
                        int gssi = d.TryGetValue(GlobalNames.MM_GSSI, out var g) ? g : -1;
                        int cck = d.TryGetValue(GlobalNames.CCK_identifier, out var c) ? c : -1;
                        string acceptType = "Roaming location updating";
                        if (d.TryGetValue(GlobalNames.Location_update_accept_type, out var at))
                        {
                            // 0 Roaming location updating, 1 Service restoration roaming location updating, 2 ITSI attach, 3 ??? keep generic
                            acceptType = at switch
                            {
                                0 => "Roaming location updating",
                                1 => "Service restoration roaming location updating",
                                2 => "ITSI attach",
                                _ => acceptType
                            };
                        }

                        // sometimes "registration/authentication ACCEPTED" and sometimes "registration ACCEPTED"
                        bool regAuth = d.TryGetValue(GlobalNames.Registration_required, out var rr) && rr == 1;
                        string prefix = regAuth ? "MS request for registration/authentication ACCEPTED" : "MS request for registration ACCEPTED";
                        string s = $"{prefix} for SSI: {ssi}";
                        if (gssi > -1) s += $" GSSI: {gssi}";
                        s += $" - {authStatus}";
                        if (cck > -1) s += $" - CCK_identifier: {cck}";
                        s += $" - {acceptType}";
                        return s;
                    }

                case MmPduType.D_LOCATION_UPDATE_REJECT:
                    {
                        string acceptType = "Roaming location updating";
                        if (d.TryGetValue(GlobalNames.Location_update_accept_type, out var at))
                        {
                            acceptType = at switch
                            {
                                0 => "Roaming location updating",
                                1 => "Service restoration roaming location updating",
                                2 => "ITSI attach",
                                _ => acceptType
                            };
                        }
                        string cause = "Unknown";
                        if (d.TryGetValue(GlobalNames.Reject_cause, out var rc))
                        {
                            // minimal mapping per sample
                            cause = rc switch
                            {
                                3 => "LA not allowed (LA rejection)",
                                9 => "ITSI/ATSI unknown (system rejection)",
                                _ => $"Cause {rc}"
                            };
                        }
                        return $"MS updating in the network is NOT ACCEPTED for SSI: {ssi} - {acceptType} CAUSE: {cause}";
                    }

                case MmPduType.D_MM_STATUS:
                    {
                        // sample doesn't show, but keep informative
                        if (d.TryGetValue(GlobalNames.Status_downlink, out var st))
                            return $"MM status: SSI: {ssi} - Status: {st}";
                        return $"MM status: SSI: {ssi}";
                    }

                default:
                    return null;
            }
        }
    }
}
