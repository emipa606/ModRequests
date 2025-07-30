using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ImperialFunctionality
{
    public class CompUnderfarmSpawner : CompImperialSpawner
    {
        public CompRefuelable compRefuelable;
        protected override bool CanOperate
        {
            get
            {
                var canOperate = parent.Spawned && WaterMainNearby() && parent.Position.Roofed(parent.Map) is false
            && parent.GetComp<CompPowerTrader>().PowerOn;
                if (canOperate)
                {
                    if (compRefuelable != null && compRefuelable.HasFuel is false) 
                    {
                        return false;
                    }
                    if (ModCompatibility.DubsBadHygieneActive && ModCompatibility.HasPipeCompAndIsEmpty(parent))
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        private bool WaterMainNearby()
        {
            var watermains = parent.Map.listerThings.ThingsOfDef(IF_DefOf.VFED_WaterMain);
            foreach (var waterMain in watermains)
            {
                if (waterMain.TryGetComp<CompPowerTrader>().PowerOn)
                {
                    var compRefuelable = waterMain.TryGetComp<CompRefuelable>();
                    if (compRefuelable != null && !compRefuelable.HasFuel)
                    {
                        continue;
                    }
                    if (ModCompatibility.DubsBadHygieneActive && ModCompatibility.HasPipeCompAndIsEmpty(waterMain))
                    {
                        continue;
                    }
                    foreach (var cell in GenRadial.RadialCellsAround(waterMain.Position, waterMain.def.specialDisplayRadius,
                        useCenter: true))
                    {
                        if (cell.InBounds(parent.Map))
                        {
                            var thing = cell.GetFirstThing(parent.Map, parent.def);
                            if (thing == this.parent)
                            {

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public ThingDef selectedThingDef;

        public IEnumerable<ThingDef> Candidates
        {
            get
            {
                yield return ThingDefOf.Plant_Potato;
                yield return IF_DefOf.Plant_Cotton;
                yield return IF_DefOf.Plant_Tinctoria;
                yield return IF_DefOf.Plant_Psychoid;
                yield return IF_DefOf.Plant_Healroot;
            }
        }

        public override int TicksUntilSpawn => GenDate.TicksPerDay * 7;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compRefuelable = parent.GetComp<CompRefuelable>();
            if (!respawningAfterLoad)
            {
                selectedThingDef = Candidates.First();
            }
        }

        protected override void TryDoSpawn()
        {
            var spawnInfo = GetSpawnInfo();
            var spawnCount = spawnInfo.spawnCount;
            while (spawnCount > 0)
            {
                Thing thing = ThingMaker.MakeThing(spawnInfo.defToSpawn);
                thing.stackCount = Mathf.Min(thing.def.stackLimit, spawnCount);
                spawnCount -= thing.stackCount;
                if (TryFindRandomCellNear(parent.Position, parent.Map, 9, out var result))
                {
                    if (result.IsValid)
                    {
                        GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Near, out var lastResultingThing);
                    }
                }
            }
        }

        public (ThingDef defToSpawn, int spawnCount) GetSpawnInfo()
        {
            return (selectedThingDef.plant.harvestedThingDef, GetCountToSpawn());
        }

        public int GetCountToSpawn()
        {
            if (selectedThingDef == ThingDefOf.Plant_Potato)
            {
                return 300;
            }
            if (selectedThingDef == IF_DefOf.Plant_Cotton)
            {
                return 250;
            }
            if (selectedThingDef == IF_DefOf.Plant_Psychoid)
            {
                return 150;
            }
            if (selectedThingDef == IF_DefOf.Plant_Tinctoria)
            {
                return 100;
            }
            if (selectedThingDef == IF_DefOf.Plant_Healroot)
            {
                return 75;
            }
            return 0;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref selectedThingDef, "ImperialFunctionality_selectedThingDef");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "IF.SelectSpawnThing".Translate(selectedThingDef.label),
                action = delegate
                {
                    var floatList = new List<FloatMenuOption>();
                    foreach (var thingDef in Candidates)
                    {
                        floatList.Add(new FloatMenuOption(thingDef.LabelCap, delegate
                        {
                            selectedThingDef = thingDef;
                            ResetCountdown();
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(floatList));
                },
                icon = selectedThingDef.uiIcon
            };
        }

        public override string CompInspectStringExtra()
        {
            if (CanOperate)
            {
                var spawnInfo = GetSpawnInfo();
                return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(spawnInfo.defToSpawn, null, spawnInfo.spawnCount)).Resolve() + ": " + ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
            }
            else
            {
                return "IF.UnderfarmInoperable".Translate();
            }
        }
    }
}
