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
    public static class LevellingHediffDefOf
    {
        static LevellingHediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LevellingHediffDefOf));
        }
        public static HediffDef HealthLevel;
    }
}
