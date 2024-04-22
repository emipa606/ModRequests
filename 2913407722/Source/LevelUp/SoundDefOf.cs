using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace LevelUp
{
    [DefOf]
    public static class LevellingSoundDefOf
    {
        static LevellingSoundDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LevellingSoundDefOf));
        }
        public static SoundDef Level_Up;
    }
}
