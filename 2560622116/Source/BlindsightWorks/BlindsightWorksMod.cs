using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace BSMod
{
    [DefOf]
    public static class BS_DefOf
    {
        public static HediffDef BS_BlindSightWorks;        
    }

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            new Harmony("TrueSight.Mod").PatchAll();
        }

        public static bool ShouldHaveBlindSightWorksHediff(this Pawn pawn)
        {
            return !pawn.Dead && pawn.Ideo != null && pawn.Ideo.IdeoApprovesOfBlindness()
                && pawn.health?.hediffSet != null && pawn.health.hediffSet.GetFirstHediffOfDef(BS_DefOf.BS_BlindSightWorks) is null
                && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight);
        }
    }

    //checks once they have sight reduced to 0%
    [HarmonyPatch(typeof(JobDriver_Blind), "Blind")]
    public static class JobDriver_Blind_Patch
    {
        public static void Postfix(Pawn pawn, Pawn doer)
        {
            if (pawn.ShouldHaveBlindSightWorksHediff())
            {
                var hediff = HediffMaker.MakeHediff(BS_DefOf.BS_BlindSightWorks, pawn);
                pawn.health.AddHediff(hediff);
            }
        }
    }

    //checks when the pawn is created
    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public static class Pawn_SpawnSetup_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance.ShouldHaveBlindSightWorksHediff())
            {
                var hediff = HediffMaker.MakeHediff(BS_DefOf.BS_BlindSightWorks, __instance);
                __instance.health.AddHediff(hediff);
            }
        }
    }

    //checks when health of pawn changes
    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
    public static class Pawn_CheckForStateChange_Patch
    {
        public static void Postfix(Pawn ___pawn)
        {
            if (___pawn.ShouldHaveBlindSightWorksHediff())
            {
                var hediff = HediffMaker.MakeHediff(BS_DefOf.BS_BlindSightWorks, ___pawn);
                ___pawn.health.AddHediff(hediff);                
            }            
        }
    }


    

    public class Hediff_BlindSightWorks : HediffWithComps
    {        
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
        }


        public override bool ShouldRemove => pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight) || !pawn.Ideo.IdeoApprovesOfBlindness();

        //public override void Tick()
        //{
        //    base.Tick();
        //    if (Find.TickManager.TicksGame % 60 != 0)
        //        return;
        //    this.TryChangeSeverity();
        //}

        //public void TryChangeSeverity()
        //{
        //    float newSeverity;
        //    if (!this.ShouldChangeSeverity(out newSeverity))
        //        return;
        //    this.severityInt = newSeverity;
        //}

        //private bool ShouldChangeSeverity(out float newSeverity)
        //{
        //    if (!this.pawn.HasPsylink && (double)this.severityInt > 0.0)
        //    {
        //        newSeverity = 0.0f;
        //        return true;
        //    }
        //    float num = (float)this.pawn.GetPsylinkLevel() / 10f;
        //    if ((double)this.severityInt != (double)num)
        //    {
        //        newSeverity = num;
        //        return true;
        //    }
        //    newSeverity = -1f;
        //    return false;
        //}
    }
}
