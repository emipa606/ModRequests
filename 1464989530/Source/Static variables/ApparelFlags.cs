using System;

namespace NightVision
{
    [Flags]
    public enum ApparelFlags : byte
    {
        None = 0,
        NullifiesPS = 1,
        GrantsNV = 1<<1,
    }
}
