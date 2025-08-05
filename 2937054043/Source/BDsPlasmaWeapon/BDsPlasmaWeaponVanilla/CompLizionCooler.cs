using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using Verse.Sound;
using Verse.AI;

namespace BDsPlasmaWeaponVanilla
{
    public class CompLizionCooler : CompResource
    {
        public CompProperties_LizionCooler Props
        {
            get
            {
                return (CompProperties_LizionCooler)props;
            }
        }

        protected CompPowerTrader powerComp;

        protected CompBreakdownable breakdownableComp;

        protected CompFlickable flickableComp;

        private int currentMode = 0;

        private int targetMode = 0;

        private PipeNet pipeNet;

        public override void PostPostMake()
        {
            base.PostPostMake();
            currentMode = 0;
        }

        public bool WantsFlick()
        {
            return targetMode != currentMode;
        }

        public void DoFlick()
        {
            currentMode = targetMode;
            SoundDefOf.FlickSwitch.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                currentMode = 0;
            }
            powerComp = parent.GetComp<CompPowerTrader>();
            flickableComp = parent.GetComp<CompFlickable>();
            breakdownableComp = parent.GetComp<CompBreakdownable>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentMode, "currentMode", 0);
            Scribe_Values.Look(ref targetMode, "targetMode", 0);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (parent != null && !parent.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }

            Command_Action turnUp = new Command_Action
            {
                action = new Action(this.turnUp),
                defaultLabel = Props.turnUpLabel.Translate(),
                icon = ContentFinder<Texture2D>.Get(Props.turnUpIcon, false),
            };
            yield return turnUp;

            Command_Action turnDown = new Command_Action
            {
                action = new Action(this.turnDown),
                defaultLabel = Props.turnDownLabel.Translate(),
                icon = ContentFinder<Texture2D>.Get(Props.turnDownIcon, false),
            };
            yield return turnDown;

            yield break;
        }

        public override string CompInspectStringExtra()
        {
            string inspectStringExtra = "CurrentMode".Translate() + currentMode + "\n" + "TargetMode".Translate() + targetMode;
            return inspectStringExtra;
        }

        public void turnUp()
        {
            if (targetMode < Props.maxModes)
            {
                targetMode++;
            }
        }

        public void turnDown()
        {
            if (targetMode > 0)
            {
                targetMode--;
            }
        }

        private bool ShouldPushHeatNow
        {
            get
            {
                PipeNet pipeNet = PipeNet;
                if ((powerComp != null && !powerComp.PowerOn) || (breakdownableComp != null && breakdownableComp.BrokenDown) || currentMode <= 0 || (pipeNet.Stored < Props.consumptionPerMode * currentMode) || (flickableComp != null && !flickableComp.SwitchIsOn))
                {
                    return false;
                }
                return true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            float currentConsumption = Props.consumptionPerMode * currentMode;
            if (parent.IsHashIntervalTick(60) && ShouldPushHeatNow)
            {
                PipeNet pipeNet = PipeNet;
                GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerMode * currentMode);
                pipeNet.DrawAmongStorage(currentConsumption, pipeNet.storages);
            }
            if (parent.IsHashIntervalTick((int)(30 / currentConsumption)) && ShouldPushHeatNow)
            {
                TargetInfo a = parent;

                if (currentConsumption > 2)
                {
                    Effecter effecter = BDStatDefOf.LizionCoolerHigh.Spawn(parent.Position, parent.Map);
                    effecter.Trigger(a, TargetInfo.Invalid);
                }
                else
                {
                    Effecter effecter = BDStatDefOf.LizionCoolerLow.Spawn(parent.Position, parent.Map);
                    effecter.Trigger(a, TargetInfo.Invalid);
                }
            }
        }
    }

    public class CompProperties_LizionCooler : CompProperties_Resource
    {
        public int maxModes = 10;
        public float heatPerMode = -10;
        public float consumptionPerMode = 1;
        public string turnUpIcon = "UI/Commands/DesirePower";
        public string turnUpLabel = "BDP_LizionCoolerTurnUp";
        public string turnDownIcon = "UI/Commands/DesirePower";
        public string turnDownLabel = "BDP_LizionCoolerTurnDown";


        public CompProperties_LizionCooler()
        {
            compClass = typeof(CompLizionCooler);
        }
    }



    public class JobDriver_FlickLizionCooler : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(15).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            Toil finalize = new Toil();
            finalize.initAction = delegate
            {
                Pawn actor = finalize.actor;
                ThingWithComps thingWithComps = (ThingWithComps)actor.CurJob.targetA.Thing;
                for (int i = 0; i < thingWithComps.AllComps.Count; i++)
                {
                    if (thingWithComps.AllComps[i] is CompLizionCooler compLizionCooler && compLizionCooler.WantsFlick())
                    {
                        compLizionCooler.DoFlick();
                    }
                }
                actor.records.Increment(RecordDefOf.SwitchesFlicked);
            };
            finalize.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finalize;
        }
    }

    public class WorkGiver_FlickLizionCooler : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.BDP_LizionCooler);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompLizionCooler comp = t.TryGetComp<CompLizionCooler>();
            if (comp == null || !comp.WantsFlick())
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(JobDefOf.BDP_FlickLizionCooler, t);
        }
    }
}
