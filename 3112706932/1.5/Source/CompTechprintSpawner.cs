using RimWorld;
using System.Linq;
using Verse;

namespace ImperialFunctionality
{
    public class CompTechprintSpawner : CompImperialSpawner
    {
        public ThingDef selectedThingDef;

        public bool isWorking;

        public override int TicksUntilSpawn => GenDate.TicksPerDay * 5;

        protected override bool CanOperate => parent.Spawned && parent.GetComp<CompPowerTrader>().PowerOn;

        protected override void TickInterval(int interval)
        {
            if (CanOperate)
            {
                var comp = parent.GetComp<CompRefuelable>();
                if (isWorking is false && comp.HasFuel)
                {
                    selectedThingDef = IF_DefOf.IF_ImperialInfo;
                    comp.ConsumeFuel(1f);
                    isWorking = true;
                    ResetCountdown();
                }

                if (isWorking)
                {
                    ticksUntilSpawn -= interval;
                    CheckShouldSpawn();
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            if (CanOperate && isWorking && selectedThingDef != null)
            {
                return "NextSpawnedItemIn".Translate(selectedThingDef.label).Resolve() + ": " + ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
            }
            return base.CompInspectStringExtra();
        }

        protected override void TryDoSpawn()
        {
            if (selectedThingDef != null)
            {
                Thing thing = ThingMaker.MakeThing(selectedThingDef);
                if (TryFindRandomCellNear(parent.Position, parent.Map, 9, out var result))
                {
                    if (result.IsValid)
                    {
                        GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Near, out var lastResultingThing);
                        isWorking = false;
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isWorking, "CompTechprintSpawner_isWorking");
            Scribe_Defs.Look(ref selectedThingDef, "ImperialFunctionality_selectedThingDef");
        }
    }
}
