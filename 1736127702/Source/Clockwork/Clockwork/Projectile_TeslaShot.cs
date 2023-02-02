using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Clockwork
{
    //#region Properties
    //public class Projectile_TeslaShot : Projectile
    //{
    //    public ThingDef_TeslaShot Def
    //    {
    //        get
    //        {
    //            return this.def as ThingDef_TeslaShot;
    //        }
    //    }

    //    #endregion

    //    #region Overrides
    //    protected override void Impact(Thing hitThing)
    //    {
    //        base.Impact(hitThing);
    //        if (Def != null && hitThing != null && hitThing is Pawn hitPawn)
    //        {
    //            var rand = Rand.Value;
    //            if (rand <= Def.AddHediffChance)
    //            {
    //                MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "TeslaShot_SuccessMote".Translate(), 12f);
    //                var shockedOnPawn = hitPawn.health?.hediffSet?.GetFirstHediffOfDef(Def.HediffToAdd);
    //                var randomSeverity = Rand.Range(0.15f, 0.30f);
    //                if (shockedOnPawn != null)
    //                {
    //                    shockedOnPawn.Severity += randomSeverity;
    //                }
    //                else
    //                {
    //                    Hediff hediff = HediffMaker.MakeHediff(Def.HediffToAdd, hitPawn);
    //                    hediff.Severity = randomSeverity;
    //                    hitPawn.health.AddHediff(hediff);
    //                }
    //            }
    //            else
    //            {
    //                MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "TeslaShot_FailureMote".Translate(Def.AddHediffChance), 12f);
    //            }
    //        }
    //    }
    //    #endregion Overrides
    //}
}
