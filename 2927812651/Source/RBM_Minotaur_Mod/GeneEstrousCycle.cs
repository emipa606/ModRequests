using HarmonyLib;
using RimWorld;
using Verse;




namespace RBM_Minotaur
{
    [HarmonyPatch]
    public static class Lovin_MTB_Patch
    {
        [HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetLovinMtbHours))]
        [HarmonyPostfix]
        // Adjusts Mean Time Between Lovin' for pawns in heat
        public static void GetLovinMtbHours_Postfix(ref float __result, Pawn pawn, Pawn partner)
        {
            //Log message if dewbug is enabled
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (Lovin Patch) public static void GetLovinMtbHours_Postfix(ref float __result, Pawn pawn, Pawn partner)"); }

            if (pawn.health.hediffSet.HasHediff(RBM_DefOf.EstrousHeat) || partner.health.hediffSet.HasHediff(RBM_DefOf.EstrousHeat))
            {
                __result /= 4;
            }
        }
    }

    public class Gene_RBM_EstrousCycle : Gene
    {
        // Adds the 'In Heat' hediff to pawns with the Estrous Cycle gene in ApriMay
        public override void Tick()
        {
            base.Tick();

            //Return if the hash interval is incorrect (not enough time has passed)
            if (!this.pawn.IsHashIntervalTick(1440))
            {
                return;
            }

            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (Lovin Patch) public override void Tick() (Hash inteval passed) "); }

            //Return if the pawn is not spawned to stop 
            if (!this.pawn.Spawned)
            {
                return;
            }

            float latitude = Find.WorldGrid.LongLatOf(this.pawn.Map.Tile).y;
            int absTick = GenTicks.TicksAbs;
            bool isAprimay = GenDate.Quadrum(absTick, latitude) == Quadrum.Aprimay;
            bool pawnHasHeatHediff = this.pawn.health.hediffSet.HasHediff(RBM_DefOf.EstrousHeat);

            //Add Hediff if it is may
            if (isAprimay && !pawnHasHeatHediff)
            {
                this.pawn.health.AddHediff(RBM_DefOf.EstrousHeat);
            }

            //Remove Hediff if it is not aprimay
            else if (!isAprimay && pawnHasHeatHediff)
            {
                Hediff HeatHediff = this.pawn.health.hediffSet.GetFirstHediffOfDef(RBM_DefOf.EstrousHeat);
                this.pawn.health.RemoveHediff(HeatHediff);
            }

        }
    }
}





