﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Dormitories
{
    [DefOf]
    public static class InternalDefOf
    {
        [MayRequire("Hospitality")]
        public static RoomRoleDef GuestRoom;
    }
}
