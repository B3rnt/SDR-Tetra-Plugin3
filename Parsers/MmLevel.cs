using System;
using System.Text;

namespace SDRSharp.Tetra
{
    unsafe class MmLevel
    {
        private readonly Rules[] _locationUpdateAcceptRules = new Rules[]
        {
            new Rules(GlobalNames.Location_update_accept_type, 3, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Options_bit, 1, RulesType.Options_bit, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.MM_SSI, 24, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 24, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 16, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 14, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 6, RulesType.Reserved, 0, 0, 0)
        };

        private readonly Rules[] _locationUpdateCommandRules = new Rules[]
        {
            new Rules(GlobalNames.Group_identity_report, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Cipher_control, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Ciphering_parameters, 32, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Group_identity_acknowledgement_request, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Location_update_type, 2, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.MM_SSI, 24, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.MM_Address_extension, 24, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Reserved, 2, RulesType.Reserved, 0, 0, 0)
        };

        private readonly Rules[] _locationUpdateRejectRules = new Rules[]
        {
            new Rules(GlobalNames.Reject_cause, 4, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Options_bit, 1, RulesType.Options_bit, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.MM_SSI, 24, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 24, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 16, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 14, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 6, RulesType.Reserved, 0, 0, 0)
        };

        private readonly Rules[] _locationUpdateProceedingRules = new Rules[]
        {
            new Rules(GlobalNames.Location_update_type, 2, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Options_bit, 1, RulesType.Options_bit, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.MM_SSI, 24, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 24, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 16, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 14, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.Presence_bit, 1, RulesType.Presence_bit, 1, 0, 0),
            new Rules(GlobalNames.Reserved, 6, RulesType.Reserved, 0, 0, 0)
        };

        private readonly Rules[] _attachDetachGroupIdentityRules = new Rules[]
        {
            new Rules(GlobalNames.Group_identity_accept_reject, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Group_identity_attach_detach_mode, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Group_identity_report, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.MM_vGSSI, 24, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Cipher_control, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Ciphering_parameters, 32, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Group_identity_acknowledgement_request, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Reserved, 3, RulesType.Reserved, 0, 0, 0)
        };

        private readonly Rules[] _attachDetachGroupIdentityAckRules = new Rules[]
        {
            new Rules(GlobalNames.Group_identity_attach_detach_mode, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Group_identity_accept_reject, 1, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Reserved, 2, RulesType.Reserved, 0, 0, 0),
            new Rules(GlobalNames.MM_SSI, 24, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.MM_Address_extension, 24, RulesType.Direct, 0, 0, 0),
            new Rules(GlobalNames.Reserved, 4, RulesType.Reserved, 0, 0, 0)
        };

        public void Parse(LogicChannel channelData, int offset, ReceivedData result)
        {
            int mmStart = offset;

            // Defensive: ReceivedData is reused; clear per-PDU fields so values don't leak.
            result.SetValue(GlobalNames.GSSI, -1);
            result.SetValue(GlobalNames.MM_vGSSI, -1);
            result.SetValue(GlobalNames.CCK_id, -1);
            result.SetValue(GlobalNames.GSSI_verified, 0);
            result.SetValue(GlobalNames.ITSI_attach, 0);

            if (offset + 4 > channelData.Length)
            {
                result.SetValue(GlobalNames.OutOfBuffer, 1);
                return;
            }

            MmPduType mmType = (MmPduType)TetraUtils.BitsToInt32(channelData.Ptr, offset, 4);
            result.SetValue(GlobalNames.MM_PDU_Type, (int)mmType);
            offset += 4;

            switch (mmType)
            {
                case MmPduType.D_LOCATION_UPDATE_ACCEPT:
                    offset = Global.ParseParams(channelData, offset, _locationUpdateAcceptRules, result);
                    offset = ParseLocationUpdateAcceptExtensions(channelData, offset, result);

                    // Last resort: if we still didn't find CCK_id, scan a bit wider over this MM payload.
                    if (result.Value(GlobalNames.CCK_id) <= 0)
                        ScanForCck64(channelData, mmStart, result);

                    if (result.Value(GlobalNames.CCK_id) == 64 && result.Value(GlobalNames.GSSI_verified) == 0)
                        result.SetValue(GlobalNames.ITSI_attach, 1);
                    break;

                case MmPduType.D_LOCATION_UPDATE_COMMAND:
                    offset = Global.ParseParams(channelData, offset, _locationUpdateCommandRules, result);
                    break;

                case MmPduType.D_LOCATION_UPDATE_REJECT:
                    offset = Global.ParseParams(channelData, offset, _locationUpdateRejectRules, result);
                    break;

                case MmPduType.D_LOCATION_UPDATE_PROCEEDING:
                    offset = Global.ParseParams(channelData, offset, _locationUpdateProceedingRules, result);
                    break;

                case MmPduType.D_ATTACH_DETACH_GROUP_IDENTITY:
                    offset = Global.ParseParams(channelData, offset, _attachDetachGroupIdentityRules, result);
                    break;

                case MmPduType.D_ATTACH_DETACH_GROUP_IDENTITY_ACKNOWLEDGEMENT:
                    offset = Global.ParseParams(channelData, offset, _attachDetachGroupIdentityAckRules, result);
                    break;

                case MmPduType.D_MM_STATUS:
                    if (offset + 6 <= channelData.Length)
                    {
                        result.SetValue(GlobalNames.Status_downlink, TetraUtils.BitsToInt32(channelData.Ptr, offset, 6));
                        offset += 6;
                    }
                    else result.SetValue(GlobalNames.OutOfBuffer, 1);
                    break;

                case MmPduType.MM_PDU_FUNCTION_NOT_SUPPORTED:
                    if (offset + 4 <= channelData.Length)
                    {
                        result.SetValue(GlobalNames.Not_supported_sub_PDU_type, TetraUtils.BitsToInt32(channelData.Ptr, offset, 4));
                        offset += 4;
                    }
                    else result.SetValue(GlobalNames.OutOfBuffer, 1);
                    break;

                case MmPduType.D_OTAR:
                    if (offset + 4 <= channelData.Length)
                    {
                        int sub = TetraUtils.BitsToInt32(channelData.Ptr, offset, 4);
                        result.SetValue(GlobalNames.Otar_sub_type, sub);
                        offset += 4;

                        if (offset + 8 <= channelData.Length)
                        {
                            result.SetValue(GlobalNames.CCK_id, TetraUtils.BitsToInt32(channelData.Ptr, offset, 8));
                            offset += 8;
                        }
                    }
                    else result.SetValue(GlobalNames.OutOfBuffer, 1);
                    break;

                case MmPduType.D_AUTHENTICATION:
                    if (offset + 2 <= channelData.Length)
                    {
                        int sub = TetraUtils.BitsToInt32(channelData.Ptr, offset, 2);
                        result.SetValue(GlobalNames.Authentication_sub_type, sub);
                        offset += 2;

                        if ((sub == (int)D_AuthenticationPduSubType.Result || sub == (int)D_AuthenticationPduSubType.Reject) &&
                            offset + 6 <= channelData.Length)
                        {
                            result.SetValue(GlobalNames.Authentication_status, TetraUtils.BitsToInt32(channelData.Ptr, offset, 6));
                            offset += 6;
                        }
                    }
                    else result.SetValue(GlobalNames.OutOfBuffer, 1);
                    break;

                case MmPduType.D_CK_CHANGE_DEMAND:
                    if (offset + 1 <= channelData.Length)
                    {
                        result.SetValue(GlobalNames.CK_provision_flag, TetraUtils.BitsToInt32(channelData.Ptr, offset, 1));
                        offset += 1;
                    }
                    else result.SetValue(GlobalNames.OutOfBuffer, 1);
                    break;

                default:
                    break;
            }

            MmLogger.LogMmPdu(channelData, mmStart, channelData.Length - mmStart, result);
        }

        private static int ParseLocationUpdateAcceptExtensions(LogicChannel channelData, int offset, ReceivedData result)
        {
            try
            {
                if (offset + 10 > channelData.Length)
                    return offset;

                int groupIdentityLocAccept = TetraUtils.BitsToInt32(channelData.Ptr, offset, 4);
                offset += 4;

                int defaultLifetime = TetraUtils.BitsToInt32(channelData.Ptr, offset, 6);
                offset += 6;

                if (groupIdentityLocAccept == 0)
                {
                    bool cckFound = ScanForCck64(channelData, offset, result);
                    if (cckFound && result.Value(GlobalNames.CCK_id) == 64)
                        result.SetValue(GlobalNames.ITSI_attach, 1);
                    return offset;
                }

                // fallback candidate from GI list if marker recovery fails
                int giListCandidate = -1;

                while (offset + 2 <= channelData.Length)
                {
                    int t = TetraUtils.BitsToInt32(channelData.Ptr, offset, 2);
                    offset += 2;

                    if (t == 3)
                        break;

                    if (t == 0)
                    {
                        if (offset + 24 > channelData.Length) break;
                        int gssi = TetraUtils.BitsToInt32(channelData.Ptr, offset, 24);
                        offset += 24;

                        if (giListCandidate < 0) giListCandidate = gssi;
                        if (result.Value(GlobalNames.MM_vGSSI) <= 0) result.SetValue(GlobalNames.MM_vGSSI, gssi);
                    }
                    else if (t == 1)
                    {
                        if (offset + 48 > channelData.Length) break;
                        int gssi = TetraUtils.BitsToInt32(channelData.Ptr, offset, 24);
                        offset += 24;

                        if (giListCandidate < 0) giListCandidate = gssi;
                        if (result.Value(GlobalNames.MM_vGSSI) <= 0) result.SetValue(GlobalNames.MM_vGSSI, gssi);

                        offset += 24;
                    }
                    else if (t == 2)
                    {
                        if (offset + 24 > channelData.Length) break;
                        int vgssi = TetraUtils.BitsToInt32(channelData.Ptr, offset, 24);
                        offset += 24;

                        result.SetValue(GlobalNames.MM_vGSSI, vgssi);
                        if (giListCandidate < 0) giListCandidate = vgssi;
                    }
                    else
                    {
                        break;
                    }
                }

                ScanForCck64(channelData, offset, result);

                // AUTHORITATIVE: recover nibble-shifted GSSI before (?? 84 8D 40) on ANY bit alignment
                if (TryRecoverNibbleShiftedGssiBefore848D40_AnyAlignment(channelData, out int recoveredGssi))
                {
                    result.SetValue(GlobalNames.GSSI, recoveredGssi);
                    result.SetValue(GlobalNames.GSSI_verified, 1);
                }
                else if (giListCandidate > 0)
                {
                    result.SetValue(GlobalNames.GSSI, giListCandidate);
                    result.SetValue(GlobalNames.GSSI_verified, 1);
                }

                if (result.Value(GlobalNames.CCK_id) == 64 && result.Value(GlobalNames.GSSI_verified) == 0)
                    result.SetValue(GlobalNames.ITSI_attach, 1);

                return offset;
            }
            catch
            {
                return offset;
            }
        }

        private static bool ScanForCck64(LogicChannel channelData, int offset, ReceivedData result)
        {
            try
            {
                int scanEnd = Math.Min(channelData.Length - 8, offset + 192);
                for (int i = offset; i <= scanEnd; i++)
                {
                    if ((i % 8) != 0) continue;
                    int b = TetraUtils.BitsToInt32(channelData.Ptr, i, 8);
                    if (b == 64)
                    {
                        result.SetValue(GlobalNames.CCK_id, b);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Recovers GSSI when it is nibble-shifted (4-bit) and located right before the marker ?? 84 8D 40,
        /// but the marker itself may be NOT byte-aligned in the bitstream. So we scan all 8 possible alignments.
        ///
        /// Reconstruction:
        ///   bytes: p3 p2 p1 p0 84 8D 40  (p0 high nibble is last nibble of GSSI; p3 low nibble is first nibble)
        ///   GSSI = [lowNibble(p3)] [p2] [p1] [highNibble(p0)]
        /// </summary>
        private static bool TryRecoverNibbleShiftedGssiBefore848D40_AnyAlignment(LogicChannel channelData, out int gssi)
        {
            gssi = -1;
            try
            {
                int maxBit = channelData.Length - 8;

                for (int start = 0; start < 8; start++)
                {
                    // we need: p3 p2 p1 p0 84 8D 40  => total 7 bytes => 56 bits
                    for (int bit = start; bit + (8 * 7) <= channelData.Length; bit++)
                    {
                        // interpret "bytes" at this alignment
                        byte p0 = ReadByteAtBit(channelData, bit + (8 * 3)); // variable
                        byte b1 = ReadByteAtBit(channelData, bit + (8 * 4)); // 0x84
                        byte b2 = ReadByteAtBit(channelData, bit + (8 * 5)); // 0x8D
                        byte b3 = ReadByteAtBit(channelData, bit + (8 * 6)); // 0x40

                        if (b1 == 0x84 && b2 == 0x8D && b3 == 0x40)
                        {
                            byte p3 = ReadByteAtBit(channelData, bit + (8 * 0));
                            byte p2 = ReadByteAtBit(channelData, bit + (8 * 1));
                            byte p1 = ReadByteAtBit(channelData, bit + (8 * 2));

                            int value =
                                ((p3 & 0x0F) << 20) |
                                (p2 << 12) |
                                (p1 << 4) |
                                ((p0 >> 4) & 0x0F);

                            gssi = value;
                            return true;
                        }

                        // advance 1 bit at a time within this alignment-search window
                        // (we keep it as bit++ so we can catch markers that start at this alignment but not at byte boundaries of the PDU)
                    }
                }
            }
            catch { }

            return false;
        }

        private static byte ReadByteAtBit(LogicChannel channelData, int bitOffset)
        {
            byte v = 0;
            for (int i = 0; i < 8; i++)
            {
                int bit = channelData.Ptr[bitOffset + i] & 0x1;
                v |= (byte)(bit << (7 - i));
            }
            return v;
        }
    }

    internal static unsafe class MmLogger
    {
        private const string DefaultPath = "mm_messages.log";

        private static int _lastAuthStatus = -1;
        private static int _lastAuthSsi = -1;
        private static DateTime _lastAuthTime = DateTime.MinValue;

        public static void LogMmPdu(LogicChannel channelData, int bitOffset, int bitLength, ReceivedData parsed)
        {
            try
            {
                var sb = new StringBuilder(512);

                sb.Append(DateTime.Now.ToString("HH:mm:ss"));
                sb.Append("  ");

                int la = parsed.Value(GlobalNames.Location_Area);
                if (la <= 0)
                    la = TetraRuntime.CurrentLocationArea;
                if (la > 0)
                {
                    sb.Append("[LA: ");
                    sb.Append(la.ToString().PadLeft(4));
                    sb.Append("]   ");
                }
                else
                {
                    sb.Append("[LA:    ]   ");
                }

                MmPduType mmType = (MmPduType)parsed.Value(GlobalNames.MM_PDU_Type);

                int ssi = parsed.Value(GlobalNames.SSI);
                if (ssi <= 0) ssi = parsed.Value(GlobalNames.MM_SSI);

                int gssi = parsed.Value(GlobalNames.GSSI);
                int gssiVerified = parsed.Value(GlobalNames.GSSI_verified);
                if (gssiVerified != 1) gssi = -1;

                int cckId = parsed.Value(GlobalNames.CCK_id);

                bool isItsiAttach = (mmType == MmPduType.D_LOCATION_UPDATE_ACCEPT && parsed.Value(GlobalNames.ITSI_attach) == 1);
                int lut = parsed.Value(GlobalNames.Location_update_type);

                switch (mmType)
                {
                    case MmPduType.D_AUTHENTICATION:
                    {
                        int sub = parsed.Value(GlobalNames.Authentication_sub_type);
                        int status = parsed.Value(GlobalNames.Authentication_status);

                        if (sub == (int)D_AuthenticationPduSubType.Result || sub == (int)D_AuthenticationPduSubType.Reject)
                        {
                            _lastAuthStatus = status;
                            _lastAuthSsi = ssi;
                            _lastAuthTime = DateTime.Now;
                        }

                        if (sub == (int)D_AuthenticationPduSubType.Demand)
                        {
                            sb.Append("BS demands authentication");
                            if (ssi > 0) { sb.Append(": SSI: "); sb.Append(ssi); }
                        }
                        else if (sub == (int)D_AuthenticationPduSubType.Result)
                        {
                            sb.Append("BS result to MS authentication: ");
                            sb.Append(AuthenticationStatusToString(status));
                            if (ssi > 0) { sb.Append(" SSI: "); sb.Append(ssi); }
                            sb.Append(" - ");
                            sb.Append(AuthenticationStatusToString(status));
                        }
                        else
                        {
                            sb.Append("MM D_AUTHENTICATION auth_sub=");
                            sb.Append(sub);
                            if (ssi > 0) { sb.Append(" SSI: "); sb.Append(ssi); }
                        }
                        break;
                    }

                    case MmPduType.D_LOCATION_UPDATE_ACCEPT:
                    {
                        int acc = parsed.Value(GlobalNames.Location_update_accept_type);

                        sb.Append("MS request for registration");
                        bool recentAuth = (_lastAuthSsi > 0 && _lastAuthSsi == ssi && (DateTime.Now - _lastAuthTime).TotalSeconds <= 3.0);
                        if (acc == 0 || recentAuth) sb.Append("/authentication ACCEPTED");
                        else sb.Append(" ACCEPTED");

                        if (ssi > 0) { sb.Append(" for SSI: "); sb.Append(ssi); }

                        if (gssi > 0)
                        {
                            sb.Append(" GSSI: ");
                            sb.Append(gssi);
                        }

                        if (_lastAuthStatus >= 0 && (_lastAuthSsi <= 0 || _lastAuthSsi == ssi))
                        {
                            sb.Append(" - ");
                            sb.Append(AuthenticationStatusToString(_lastAuthStatus));
                        }

                        if (cckId > 0)
                        {
                            sb.Append(" - CCK_identifier: ");
                            sb.Append(cckId);
                        }

                        if (isItsiAttach)
                        {
                            sb.Append(" - ITSI attach");
                        }
                        else if (cckId == 64)
                        {
                            sb.Append(" - Roaming location updating");
                        }
                        else
                        {
                            if (lut >= 0)
                            {
                                string lutText = LocationUpdateTypeToString(lut);
                                if (!string.IsNullOrEmpty(lutText))
                                {
                                    sb.Append(" - ");
                                    sb.Append(lutText);
                                }
                            }
                        }

                        break;
                    }

                    default:
                    {
                        sb.Append("MM ");
                        sb.Append(mmType.ToString());
                        break;
                    }
                }

                sb.Append("  raw=");
                sb.Append(BitsToHex(channelData.Ptr, bitOffset, bitLength));

                new TextFile().Write(sb.ToString(), DefaultPath);
            }
            catch
            {
            }
        }

        private static string AuthenticationStatusToString(int status)
        {
            if (status >= 0)
                return "Authentication successful or no authentication currently in progress";
            return "Authentication status unknown";
        }

        private static string LocationUpdateTypeToString(int t)
        {
            switch (t)
            {
                case 0: return "Normal location updating";
                case 1: return "Roaming location updating";
                case 2: return "Periodic location updating";
                default: return "Location update type " + t.ToString();
            }
        }

        private static string BitsToHex(byte* ptr, int bitOffset, int bitLength)
        {
            if (bitLength <= 0) return string.Empty;
            int byteLen = (bitLength + 7) / 8;
            byte[] bytes = new byte[byteLen];

            for (int i = 0; i < bitLength; i++)
            {
                int bit = ptr[bitOffset + i] & 0x1;
                int byteIndex = i / 8;
                int bitInByte = 7 - (i % 8);
                bytes[byteIndex] |= (byte)(bit << bitInByte);
            }

            var sb = new StringBuilder(byteLen * 2);
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
