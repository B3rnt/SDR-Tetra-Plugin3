using System;

namespace SDRSharp.Tetra
{
    /// <summary>
    /// Mobility Management (MM) decoder/formatter.
    /// This implementation is intentionally conservative: it always produces a loggable MmInfo,
    /// and decodes the most common MM messages used by the reference SDRtetra build.
    /// </summary>
    internal static unsafe class Class18
    {
        private static int Bits(LogicChannel lc, int off, int n)
        {
            if (lc == null || lc.Ptr == null || n <= 0) return -1;
            return TetraUtils.BitsToInt32(lc.Ptr, off, n);
        }

        private static string MmPduName(int pdu)
        {
            // Matches SDRtetra.zip enum MmPduType (0..15)
            return pdu switch
            {
                0 => "D_OTAR",
                1 => "D_AUTHENTICATION",
                2 => "D_CK_CHANGE_DEMAND",
                3 => "D_DISABLE",
                4 => "D_ENABLE",
                5 => "D_LOCATION_UPDATE_ACCEPT",
                6 => "D_LOCATION_UPDATE_REJECT",
                7 => "D_LOCATION_UPDATE_COMMAND",
                8 => "D_LOCATION_UPDATE_DEMAND",
                9 => "D_DETACH",
                10 => "D_ATTACH",
                11 => "D_ATTACH_DETACH_GROUP_IDENTITY",
                12 => "D_ATTACH_DETACH_GROUP_IDENTITY_ACKNOWLEDGEMENT",
                13 => "D_MM_STATUS",
                15 => "MM_PDU_FUNCTION_NOT_SUPPORTED",
                _ => $"Reserved{pdu}"
            };
        }

        // Location update type strings as seen in common SDRtetra logs/blog output.
        private static string LocationUpdateTypeText(int t)
        {
            return t switch
            {
                0 => "Roaming location updating",
                1 => "Periodic location updating",
                2 => "Demand location updating",
                3 => "Service restoration roaming location updating",
                4 => "Service restoration migrating location updating",
                5 => "Migrating location updating",
                6 => "Disabled MS updating",
                7 => "ITSI attach",
                _ => $"Location updating type {t}"
            };
        }

        // Reject cause strings (subset; extend as you see them)
        private static string RejectCauseText(int c)
        {
            // The exact list differs by network; these match the common wording used by SDRtetra UI.
            return c switch
            {
                0 => "LA not allowed (LA rejection)",
                1 => "LA unknown (LA rejection)",
                2 => "Roaming not supported (LA rejection)",
                3 => "Service not subscribed (LA rejection)",
                4 => "Migration not supported (system rejection)",
                5 => "ITSI/ATSI unknown (system rejection)",
                6 => "Illegal MS (system rejection)",
                7 => "Message consistency error (system rejection)",
                8 => "Mandatory element error (system rejection)",
                9 => "Authentication failure (system rejection)",
                10 => "Congestion (cell rejection)",
                11 => "Ciphering required (cell rejection)",
                12 => "Requested cipher key type not available (cell rejection)",
                13 => "Identified cipher key not available (cell rejection)",
                14 => "Identified cipher KSG not supported (cell rejection)",
                15 => "No cipher KSG (cell rejection)",
                16 => "Forward registration failure (cell rejection)",
                17 => "Network failure (cell rejection)",
                18 => "Use of DA cell not permitted (cell type rejection)",
                19 => "Use of CA cell not permitted (cell type rejection)",
                _ => $"Reject cause {c}"
            };
        }

        private static string AuthSubTypeText(int st)
        {
            return st switch
            {
                0 => "Demand",
                1 => "Response",
                2 => "Result",
                3 => "Reject",
                _ => st.ToString()
            };
        }

        public static MmInfo ParseMm(LogicChannel channelData, int offset, ReceivedData received)
        {
            var info = new MmInfo();

            // 4-bit MM PDU type (per SDRtetra MmPduType)
            int mmPduType = Bits(channelData, offset, 4);
            if (mmPduType < 0) mmPduType = 0;
            // removed: MM_PDU_Type not in this fork
            offset += 4;

            // In the reference build, many MM messages carry optional fields preceded by:
            // Options_bit (1) and Presence_bit (1).
            int optionsBit = Bits(channelData, offset, 1);
            if (optionsBit >= 0) offset += 1;
            int presenceBit = Bits(channelData, offset, 1);
            if (presenceBit >= 0) offset += 1;

            // MM_SSI (24) often present when presenceBit==1
            int mmSsi = -1;
            if (presenceBit == 1)
            {
                mmSsi = Bits(channelData, offset, 24);
                if (mmSsi >= 0)
                {
                    received.SetValue(GlobalNames.SSI, mmSsi);
                    info.Ssi = mmSsi;
                }
                offset += 24;
            }
            else
            {
                // fallback: try existing SSI field (some layers fill it)
                int s = -1;
                if (received.TryGetValue(GlobalNames.SSI, ref s))
                {
                    info.Ssi = s;
                }
            }

            string pduName = MmPduName(mmPduType);

            // Always provide at least a minimal message so the caller logs something.
            info.Message = $"MM: {pduName} (PDU {mmPduType})" + (info.Ssi > 0 ? $": SSI: {info.Ssi}" : "");

            // Decode key MM messages for human-readable logs
            switch (mmPduType)
            {
                case 1: // D_AUTHENTICATION
                {
                    int sub = Bits(channelData, offset, 2);
// removed: Authentication_sub_type not in this fork
                    offset += 2;

                    if (sub == 0) // Demand
                    {
                        info.Message = $"BS demands authentication: SSI: {info.Ssi}";
                    }
                    else if (sub == 2) // Result
                    {
                        // Some networks include a result/cause field; when unknown, match common SDRtetra wording.
                        info.Message = $"BS result to MS authentication: Authentication successful or no authentication currently in progress SSI: {info.Ssi} - Authentication successful or no authentication currently in progress";
                    }
                    else if (sub == 3) // Reject
                    {
                        info.Message = $"Authentication_Result: Authentication failed";
                    }
                    else
                    {
                        info.Message = $"Authentication: {AuthSubTypeText(sub)} SSI: {info.Ssi}";
                    }
                    break;
                }

                case 5: // D_LOCATION_UPDATE_ACCEPT
                {
                    int accType = Bits(channelData, offset, 2);
// removed: Location_update_accept_type not in this fork
                    offset += 2;

                    // Optional Group identity + CCK identifier are often present; parse safely if available.
                    // Group identity accept/reject (1) + MM_GSSI (24)
                    int groupFlag = Bits(channelData, offset, 1);
                    if (groupFlag >= 0)
                    {
                        offset += 1;
                        if (groupFlag == 1)
                        {
                            int gssi = Bits(channelData, offset, 24);
                            if (gssi >= 0)
                            {
                                received.SetValue(GlobalNames.GSSI, gssi);
                                info.Gssi = gssi;
                            }
                            offset += 24;
                        }
                    }

                    // CCK_identifier (16) optional
                    int cck = Bits(channelData, offset, 16);
                    if (cck >= 0 && cck != 0)
                    {
                        // removed: CCK_identifier not in this fork
                        info.CckIdentifier = cck;
                        offset += 16;
                    }

                    // The text in SDRtetra logs includes a Location updating type line; accept-type itself is not printed as a number.
                    // We default to Roaming if not known.
                    string lut = LocationUpdateTypeText(0);
                    info.Message = $"MS request for registration/authentication ACCEPTED for SSI: {info.Ssi}";
                    // follow-up details (logger can print multi-line, but for now embed as single line cues)
                    if (info.Gssi > 0) info.Message += $" GSSI: {info.Gssi}";
                    if (info.CckIdentifier > 0) info.Message += $" - CCK_identifier: {info.CckIdentifier}";
                    info.Message += $" - {lut}";
                    break;
                }

                case 6: // D_LOCATION_UPDATE_REJECT
                {
                    int lut = Bits(channelData, offset, 3);
                    offset += 3;
                    int cause = Bits(channelData, offset, 5);
                    offset += 5;

// removed: Location_update_type not in this fork
// removed: Reject_cause not in this fork

                    info.Message = $"MS request for registration REJECTED for SSI: {info.Ssi} - {LocationUpdateTypeText(lut)} CAUSE: {RejectCauseText(cause)}";
                    break;
                }

                case 0: // D_OTAR
                {
                    // We don't fully decode OTAR primitives yet, but at least label it
                    info.Message = $"OTAR: SSI: {info.Ssi}";
                    break;
                }

                case 11: // D_ATTACH_DETACH_GROUP_IDENTITY
                {
                    // Often contains GSSI + ack status
                    int gssi = Bits(channelData, offset, 24);
                    if (gssi >= 0)
                    {
                        received.SetValue(GlobalNames.GSSI, gssi);
                        info.Gssi = gssi;
                    }
                    info.Message = $"BS acknowledges MS initiated attachment/detachment of group identities GSSI:{info.Gssi} SSI: {info.Ssi} All attachment/detachments accepted";
                    break;
                }
            }

            return info;
        }
    }
}
