using System;
using System.Collections.Generic;
using System.Text;

namespace phirSOFT.PropertyObservation
{
    [Flags]
    public enum SyncDirection
    {
        LeftToRight = 1,
        RightToLeft = 2,
        Both = LeftToRight | RightToLeft;
    }
}
