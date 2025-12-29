using System;
using System.Text;

namespace SDRSharp.Tetra
{
    unsafe class MmLevel
    {
        // Ported (cleaned) from the extended SDRtetra source: Class18 rules_0..rules_5
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

            if (offset + 4 > channelData.Length)
            {
                result.SetValue(GlobalNames.OutOfBuffer, 1);
                return;
            }

            var mmType = (MmPduType)TetraUtils.BitsToInt32(channelData.Ptr, offset, 4);
            result.SetValue(GlobalNames.MM_PDU_Type, (int)mmType);
            offset += 4;

            // Decode what we can. For the more complex messages (OTAR / AUTH), we at least log the subtype and raw payload.
            switch (mmType)
            {
                case MmPduType.D_LOCATION_UPDATE_ACCEPT:
                    offset = Global.ParseParams(channelData, offset, _locationUpdateAcceptRules, result);
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
                    else
                    {
                        result.SetValue(GlobalNames.OutOfBuffer, 1);
                    }
                    break;

                case MmPduType.MM_PDU_FUNCTION_NOT_SUPPORTED:
                    if (offset + 4 <= channelData.Length)
                    {
                        result.SetValue(GlobalNames.Not_supported_sub_PDU_type, TetraUtils.BitsToInt32(channelData.Ptr, offset, 4));
                        offset += 4;
                    }
                    else
                    {
                        result.SetValue(GlobalNames.OutOfBuffer, 1);
                    }
                    break;

                case MmPduType.D_OTAR:
                    if (offset + 4 <= channelData.Length)
                    {
                        result.SetValue(GlobalNames.Otar_sub_type, TetraUtils.BitsToInt32(channelData.Ptr, offset, 4));
                        offset += 4;
                    }
                    else
                    {
                        result.SetValue(GlobalNames.OutOfBuffer, 1);
                    }
                    break;

                case MmPduType.D_AUTHENTICATION:
                    if (offset + 2 <= channelData.Length)
                    {
                        result.SetValue(GlobalNames.Authentication_sub_type, TetraUtils.BitsToInt32(channelData.Ptr, offset, 2));
                        offset += 2;
                    }
                    else
                    {
                        result.SetValue(GlobalNames.OutOfBuffer, 1);
                    }
                    break;

                case MmPduType.D_CK_CHANGE_DEMAND:
                    if (offset + 1 <= channelData.Length)
                    {
                        result.SetValue(GlobalNames.CK_provision_flag, TetraUtils.BitsToInt32(channelData.Ptr, offset, 1));
                        offset += 1;
                    }
                    else
                    {
                        result.SetValue(GlobalNames.OutOfBuffer, 1);
                    }
                    break;

                // D_DISABLE / D_ENABLE and reserved types have no additional mandatory fields we can safely decode here.
            }

            // Always log the MM PDU (type + decoded fields + raw payload)
            MmLogger.LogMmPdu(channelData, mmStart, channelData.Length - mmStart, result);
        }
    }

    internal static unsafe class MmLogger
    {
        private const string DefaultPath = "mm_messages.log";

        public static void LogMmPdu(LogicChannel channelData, int bitOffset, int bitLength, ReceivedData parsed)
        {
            try
            {
                var sb = new StringBuilder(256);
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.Append(" MM ");

                int pduType = parsed.Value(GlobalNames.MM_PDU_Type);
                sb.Append(((MmPduType)pduType).ToString());

                // Add a few useful decoded fields when present
                AppendIfPresent(sb, parsed, GlobalNames.Otar_sub_type, "otar_sub");
                AppendIfPresent(sb, parsed, GlobalNames.Authentication_sub_type, "auth_sub");
                AppendIfPresent(sb, parsed, GlobalNames.Status_downlink, "status");
                AppendIfPresent(sb, parsed, GlobalNames.MM_SSI, "ssi");
                AppendIfPresent(sb, parsed, GlobalNames.MM_vGSSI, "vgssi");
                AppendIfPresent(sb, parsed, GlobalNames.Location_update_type, "lu_type");
                AppendIfPresent(sb, parsed, GlobalNames.Location_update_accept_type, "lu_acc");
                AppendIfPresent(sb, parsed, GlobalNames.Reject_cause, "rej");
                AppendIfPresent(sb, parsed, GlobalNames.Not_supported_sub_PDU_type, "notsup");
                AppendIfPresent(sb, parsed, GlobalNames.Cipher_control, "cc");
                AppendIfPresent(sb, parsed, GlobalNames.Group_identity_attach_detach_mode, "gi_mode");
                AppendIfPresent(sb, parsed, GlobalNames.Group_identity_accept_reject, "gi_acc");

                // Raw payload in hex (bit-packed, MSB first)
                sb.Append(" raw=");
                sb.Append(BitsToHex(channelData.Ptr, bitOffset, bitLength));

                new TextFile().Write(sb.ToString(), DefaultPath);
            }
            catch
            {
                // Intentionally ignore logging failures so decoding continues.
            }
        }

        private static void AppendIfPresent(StringBuilder sb, ReceivedData data, GlobalNames name, string label)
        {
            int v = data.Value(name);
            if (v >= 0)
            {
                sb.Append(' ');
                sb.Append(label);
                sb.Append('=');
                sb.Append(v);
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
                int bitInByte = 7 - (i % 8); // MSB-first
                bytes[byteIndex] |= (byte)(bit << bitInByte);
            }

            var sb = new StringBuilder(byteLen * 2);
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
