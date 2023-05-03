using System;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mashed_BlackPlague
{
    public static class Utility
    {
        public static int TuurngaitInFaction(Faction faction)
        {
            return faction == null ? 0 : TuurngaitInFactionList(faction).Count();
        }

        public static List<Pawn> TuurngaitInFactionList(Faction faction)
        {
            if (faction == null)
            {
                return null;
            }
            List<Pawn> list = new List<Pawn>() { };
            using (List<Pawn>.Enumerator enumerator = PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != null && enumerator.Current.RaceProps.Humanlike && PawnIsTuurngait(enumerator.Current))
                    {
                        list.Add(enumerator.Current);
                    }
                }
            }
            return list;
        }

        public static bool ThingIsTuurngait(Thing t)
        {
            return t is Pawn p && PawnIsTuurngait(p);
        }

        public static bool PawnIsTuurngait(Pawn p)
        {
            return !p.Dead && p.def == ThingDefOf.BlackPlague_TuurngaitRace;
        }

        public static bool PawnHasVesselThought(Pawn p)
        {
            return p.needs.mood != null && p.needs.mood.thoughts.memories.Memories.Any(x => x.def == ThoughtDefOf.BlackPlague_TouchedVessel || x.def == ThoughtDefOf.BlackPlague_TouchedVessel_Infected);
        }

        public static bool PawnIsNotValid(Pawn p)
        {
            if (DefDatabase<HediffDef>.GetNamedSilentFail("ESCP_SloadThrallPassive") != null)
            {
                if (p.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail("ESCP_SloadThrallPassive")) != null)
                {
                    return true;
                }
            }
            return p.needs.mood == null || !p.RaceProps.IsFlesh || !p.RaceProps.Humanlike 
                || p.def.defName == "ESCP_SloadRace";
        }
    }
}
