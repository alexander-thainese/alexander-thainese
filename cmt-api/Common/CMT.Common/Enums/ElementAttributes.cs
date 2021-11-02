using System;

namespace CMT.Common
{
    [Flags]
    public enum ElementAttributes : int
    {
        Brand = 1,
        HasGlobalValues = 2,
        Derived = 4
    }
}
