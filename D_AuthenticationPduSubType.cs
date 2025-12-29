namespace SDRSharp.Tetra
{
    // Authentication PDU subtypes (2 bits) – matches the reference SDRtetra project.
    internal enum D_AuthenticationPduSubType
    {
        Demand = 0,
        Response = 1,
        Result = 2,
        Reject = 3
    }
}
