using System;

namespace CMT.BO
{
    [Flags]
    public enum ApplicationAttributes : byte
    {
        IsSourceSystem = 1,
        IsThirdPartyApp = 2
    }
}
