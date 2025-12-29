namespace SDRSharp.Tetra
{
    // OTAR PDU subtype mapping (4 bits) – matches the reference SDRtetra project.
    internal enum D_OtarPduSubType
    {
        CCK_Provide = 0,
        CCK_Reject = 1,
        SCK_Provide = 2,
        SCK_Reject = 3,
        GCK_Provide = 4,
        GCK_Reject = 5,
        Key_associate_Demand = 6,
        OTAR_NEWCELL = 7,
        GSKO_Provide = 8,
        GSKO_Reject = 9,
        Key_delete_demand = 10,
        Key_status_demand = 11,
        CMG_GTSI_provide = 12,
        DM_SCK_Activate = 13,
        Reserved_14 = 14,
        Reserved_15 = 15
    }
}
