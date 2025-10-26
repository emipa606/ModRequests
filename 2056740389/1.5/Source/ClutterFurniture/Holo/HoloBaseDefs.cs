using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Clutter_Furniture
{
    public class HoloBaseDefs : ThingDef
    {
        public List<ThingDef> HologramList = new List<ThingDef>();
        public string ActiveTexturePath = "";
        public string HoloButtonPath = "";

    }
}
