using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Diagnostics;
using UnityEngine;

namespace WallStuff
{
    public class WallMountedCompSpawner : ThingComp
    {
        private int ticksUntilSpawn;
        private int thingToSpawnIndex = 0;
        private int ticksToSpawn = 60000;
        private Boolean spawned = false;
        private ThingCountExposable thingToSpawn;
        private const int ONE_HOUR_TICKS = 2500;
        private const int ONE_DAY_TICKS = 60000;
        private const int MAX_SPAWN_TIME = ONE_DAY_TICKS * 120;
        private const int MIN_SPAWN_TIME = ONE_HOUR_TICKS;

        WallMountedCompProperties_Spawner WallMountedPropsSpawner
        {
            get
            {                
                return (WallMountedCompProperties_Spawner)this.props;
            }
        }

        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                this.ResetCountdown();
                this.thingToSpawn = WallStuffSettings.listOfSpawnableThings[thingToSpawnIndex];
            }
            spawned = true;
        }

        public override void CompTick()
        {
            this.TickInterval(1);
        }

        public override void CompTickRare()
        {
            this.TickInterval(250);
        }

        private void TickInterval(int interval)
        {
            if (!this.parent.Spawned)
            {
                return;
            }
            else if (this.parent.Position.Fogged(this.parent.Map))
            {
                return;
            }
            if (this.WallMountedPropsSpawner.requiresPower && !this.PowerOn)
            {
                return;
            }
            this.ticksUntilSpawn -= interval;
            this.CheckShouldSpawn();
        }

        private void CheckShouldSpawn()
        {
            if (this.ticksUntilSpawn <= 0)
            {
                this.TryDoSpawn();
                this.ResetCountdown();
            }
        }

        public bool TryDoSpawn()
        {
            if (!this.parent.Spawned)
            {
                return false;
            }
            if (this.WallMountedPropsSpawner.spawnMaxAdjacent >= 0)
            {
                int num = 0;
                for (int i = 0; i < 9; i++)
                {
                    IntVec3 c = this.parent.Position + GenAdj.AdjacentCellsAndInside[i];
                    if (c.InBounds(this.parent.Map))
                    {
                        List<Thing> thingList = c.GetThingList(this.parent.Map);
                        for (int j = 0; j < thingList.Count; j++)
                        {
                            if (thingList[j].def == this.thingToSpawn.thingDef)
                            {
                                num += thingList[j].stackCount;
                                if (num >= this.WallMountedPropsSpawner.spawnMaxAdjacent)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            IntVec3 center;
            if (this.TryFindSpawnCell(out center))
            {
                Thing thing = ThingMaker.MakeThing(this.thingToSpawn.thingDef, null);
                thing.stackCount = this.thingToSpawn.count;
                if (this.WallMountedPropsSpawner.inheritFaction && thing.Faction != this.parent.Faction)
                {
                    thing.SetFaction(this.parent.Faction, null);
                }
                Thing t;
                GenPlace.TryPlaceThing(thing, center, this.parent.Map, ThingPlaceMode.Direct, out t, null, null);
                if (this.WallMountedPropsSpawner.spawnForbidden)
                {
                    t.SetForbidden(true, true);
                }
                if (this.WallMountedPropsSpawner.showMessageIfOwned && this.parent.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(this.thingToSpawn.thingDef.LabelCap).CapitalizeFirst(), thing, MessageTypeDefOf.PositiveEvent, true);
                }
                return true;
            }
            return false;
        }

        private bool TryFindSpawnCell(out IntVec3 result)
        {
            Rot4 thingRot = this.parent.Rotation;
            ///IntVec3 thingCent = this.parent.Position + IntVec3.North.RotatedBy(thingRot);
            IntVec3 thingCent = this.parent.Position;
            IntVec2 thingsSize = thingRot.FacingCell.ToIntVec2;
            IEnumerable<IntVec3> adjCells = GenAdj.CellsAdjacentAlongEdge(thingCent, thingRot, thingsSize, LinkDirections.Down);

            result = thingCent + IntVec3.North.RotatedBy(thingRot);
            return true;
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (spawned)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.ChangeSpawnThing();
                act.defaultLabel = "Change Spawn Item";
                act.defaultDesc = "On";
                act.activateSound = SoundDef.Named("Click");
                act.icon = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight", true);
                yield return act;
            }

            if (spawned)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.IncrementSpawnTimerByAnHour(act);
                act.icon = ContentFinder<Texture2D>.Get("UI/IncField", true);
                act.defaultLabel = "Plus 1 Hour - Currently " + getTimeToSpawn();
                act.defaultDesc = "" + getTimeToSpawn();
                act.activateSound = SoundDef.Named("Click");
                yield return act;
            }

            if (spawned)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.DecrementSpawnTimerByAnHour(act);
                act.icon = ContentFinder<Texture2D>.Get("UI/DecField", true);
                act.defaultLabel = "Minus 1 Hour - Currently " + getTimeToSpawn();
                act.defaultDesc = "" + getTimeToSpawn();
                act.activateSound = SoundDef.Named("Click");
                yield return act;
            }

            if (spawned)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.IncrementSpawnTimerByADay(act);
                act.icon = ContentFinder<Texture2D>.Get("UI/IncFieldDay", true);
                act.defaultLabel = "Plus 1 Day - Currently " + getTimeToSpawn();
                act.defaultDesc = "" + getTimeToSpawn();
                act.activateSound = SoundDef.Named("Click");
                yield return act;
            }

            if (spawned)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.DecrementSpawnTimerByADay(act);
                act.icon = ContentFinder<Texture2D>.Get("UI/DecFieldDay", true);
                act.defaultLabel = "Minus 1 Day - Currently " + getTimeToSpawn();
                act.defaultDesc = "" + getTimeToSpawn();
                act.activateSound = SoundDef.Named("Click");
                yield return act;
            }

            if (Prefs.DevMode && this.thingToSpawn != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn " + this.thingToSpawn.thingDef.label,
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        this.TryDoSpawn();
                        this.ResetCountdown();
                    }
                };
            }
        }

        private void ChangeSpawnThing()
        {
            thingToSpawnIndex = (thingToSpawnIndex + 1) % WallStuffSettings.listOfSpawnableThings.Count;
            this.thingToSpawn = WallStuffSettings.listOfSpawnableThings[thingToSpawnIndex];
            this.ResetCountdown();
        }

        private void ResetCountdown()
        {
            IntRange spawnIntervalRange = new IntRange(ticksToSpawn, ticksToSpawn);

            this.ticksUntilSpawn = spawnIntervalRange.RandomInRange;
        }

        public override void PostExposeData()
        {
            string str = (!this.WallMountedPropsSpawner.saveKeysPrefix.NullOrEmpty()) ? (this.WallMountedPropsSpawner.saveKeysPrefix + "_") : null;
            Scribe_Values.Look<int>(ref this.ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
            Scribe_Values.Look<int>(ref this.thingToSpawnIndex, str + "thingToSpawnIndex", 0, false);
            this.thingToSpawn = WallStuffSettings.listOfSpawnableThings[thingToSpawnIndex];
        }

        public override string CompInspectStringExtra()
        {
            if (this.WallMountedPropsSpawner.writeTimeLeftToSpawn && (!this.WallMountedPropsSpawner.requiresPower || this.PowerOn))
            {
                return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(this.thingToSpawn.thingDef, null, this.thingToSpawn.count)) + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod();
            }
            return null;
        }

        private void IncrementSpawnTimerByAnHour(Command_Action act)
        {
            IncrementSpawnTimer(act, ONE_HOUR_TICKS);
        }

        private void DecrementSpawnTimerByAnHour(Command_Action act)
        {
            DecrementSpawnTimer(act, ONE_HOUR_TICKS);
        }
        private void IncrementSpawnTimerByADay(Command_Action act)
        {
            IncrementSpawnTimer(act, ONE_DAY_TICKS);
        }

        private void DecrementSpawnTimerByADay(Command_Action act)
        {
            DecrementSpawnTimer(act, ONE_DAY_TICKS);
        }

        private void IncrementSpawnTimer(Command_Action act, int ticks)
        {
            ticksToSpawn += ticks;
            if (ticksToSpawn > MAX_SPAWN_TIME)
            {
                ticksToSpawn = MAX_SPAWN_TIME;
            }
            ResetCountdown();
        }

        private void DecrementSpawnTimer(Command_Action act, int ticks)
        {
            ticksToSpawn -= ticks;
            if (ticksToSpawn < MIN_SPAWN_TIME)
            {
                ticksToSpawn = MIN_SPAWN_TIME;
            }
            ResetCountdown();
        }

        private String getTimeToSpawn()
        {
            int daysTillSpawn = ticksToSpawn / ONE_DAY_TICKS;

            if(daysTillSpawn < 3)
            {
                return (ticksToSpawn / ONE_HOUR_TICKS) + " Hours";
            }

            return daysTillSpawn + " Days";
        }
    }
}