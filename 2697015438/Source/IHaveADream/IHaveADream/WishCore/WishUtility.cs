using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using RimWorld.Planet;

namespace HDream
{
    public static class WishUtility
    {
        private static Dictionary<Pawn, Pawn_WishTracker> pawnWishTracker = new Dictionary<Pawn, Pawn_WishTracker>();

        private static WishUtilityDef def;

        public static WishUtilityDef Def 
        {
            get 
            {
                if (def == null)
                {
                    if (DefDatabase<WishUtilityDef>.DefCount == 1) def = DefDatabase<WishUtilityDef>.AllDefsListForReading[0];
                    else if (DefDatabase<WishUtilityDef>.DefCount > 1) Log.Error("You have more then 1 WishUtilityDef you should only have 1");
                    else Log.Error("WishUtilityDef not found in DefDatabase");
                }
                return def;
            }
        }

        public static Pawn_WishTracker wishes(this Pawn pawn)
        {
            if (PawnTrackerSetted(pawn)) return pawnWishTracker[pawn];
            return null;
        }
        public static void CreatePawn_WishTracker(this Pawn pawn)
        {
            if (PawnTrackerSetted(pawn))
            {
                Log.Warning(pawn.Name.ToStringFull + "already have a Pawn_WishTracker");
                return;
            }
            pawnWishTracker.Add(pawn, new Pawn_WishTracker(pawn));
        }
        public static void CreatePawn_WishTracker(this Pawn pawn, Pawn_WishTracker tracker)
        {
            if (PawnTrackerSetted(pawn))
            {
                Log.Message(pawn.Name.ToStringFull + "already have a Pawn_WishTracker, are you sure you should replace it?");
                RemovePawn_WishTracker(pawn);
            }
            pawnWishTracker.Add(pawn, tracker);
        }
        public static void RemovePawn_WishTracker(this Pawn pawn)
        {
            if (PawnTrackerSetted(pawn))
            {
                pawnWishTracker.Remove(pawn);
            }
        }

        private static bool PawnTrackerSetted(Pawn pawn)
        {
            return (pawnWishTracker.ContainsKey(pawn));
        }
        public static float ChancePerHourToGetNewWish(Pawn pawn)
        {
            if (!PawnTrackerSetted(pawn)) return -1;
            int expectationIndex = ExpectationsUtility.CurrentExpectationFor(pawn).order;
            if (expectationIndex >= Def.ChancePerHourExpectationToGetTimeWish.Count) expectationIndex = Def.ChancePerHourExpectationToGetTimeWish.Count - 1;
            float chance = Def.ChancePerHourExpectationToGetTimeWish[expectationIndex] * SettingMenu.settings.wishFrequencyFactor;
            int count = pawnWishTracker[pawn].wishes.Where(w => w.def.category == WishCategory.Time).Count();
            for (int i = 0; i < count; i++) chance *= Def.factorChancePerOtherTimeWish;
            List<Map> tmpMaps = Find.Maps.FindAll(map => map.IsPlayerHome);
            List<Caravan> tmpCaravans = Find.WorldObjects.Caravans.FindAll(car => car.IsPlayerControlled);
            count = 0;
            for (int i = 0; i < tmpMaps.Count; i++)
            {
                count += tmpMaps[i].mapPawns.FreeColonists.Count;
            }
            for (int m = 0; m < tmpCaravans.Count; m++)
            {
                count += tmpCaravans[m].PawnsListForReading.Where(p => p.IsColonist).Count();
            }
            chance *= Def.factorPerPawn.Evaluate(count);
            return chance;
        }
        public static string FormateTierKeyword(string text, int index)
        {
            return text.Replace(Def.tierKeySingular, Def.tierSingular[index])
                        .Replace(Def.tierKeyPlural, Def.tierPlural[index]);
        }

        public static bool CanHaveWish(Pawn pawn)
        {
            return pawn.IsColonistPlayerControlled && pawn.needs?.mood != null && Def.noWishTrait.Find(trait => pawn.story.traits.HasTrait(trait)) == null;
        }

    }
}
