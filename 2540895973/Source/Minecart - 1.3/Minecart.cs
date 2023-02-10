using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Minecart
{
    // Minecart ThingDef class
    public class ThingDef_Minecart : ThingDef
    {
        public float frictionCoef; // Speed is multiplied by this value every tick, simulating friction on the ground
        public float launchSpeed; // Speed is set to this value when the minecart is launched by hand

        public ThingDef railDef = ThingDefOf.ThingRail;

        public override void ResolveReferences()
        {
            if (railDef == null)
                railDef = ThingDefOf.ThingRail;
        }
    }

    // Minecart Building class
    public class Building_Minecart : Building
    {
        // Public values
        private float subtile = 0f;
        public float Subtile
        {
            get { return headMinecart.subtile; }
            set { headMinecart.subtile = value; }
        }
        public float recursiveMass = 1;
        private float speed = 0f;
        public float Speed
        {
            get { return headMinecart.speed; }
            set { headMinecart.speed = value; }
        }
        public Building_Minecart leadingMinecart;
        public Building_Minecart trailingMinecart;

        // Queriable
        public Building_Minecart headMinecart
        {
            get { return leadingMinecart == null ? this : leadingMinecart.headMinecart; }
        }

        public List<Building_Minecart> train
        {
            get
            {
                List<Building_Minecart> minecarts = new List<Building_Minecart>();
                minecarts.Add(headMinecart);
                while (true)
                {
                    if (minecarts.Last().trailingMinecart == null)
                        break;
                    minecarts.Add(minecarts.Last().trailingMinecart);
                }
                return minecarts;
            }
        }

        // Miscellaneous overrides
        public ThingDef_Minecart Def { get { return def as BuildableDef as ThingDef_Minecart; } }

        public override Vector3 DrawPos { get { return (Rotation.FacingCell.ToVector3() * subtile + this.TrueCenter()); } }

        public override void Draw() { DrawAt(DrawPos); base.Draw(); }

        public override void Print(SectionLayer layer) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref speed, "speed", defaultValue: 0f);
            Scribe_Values.Look(ref subtile, "subtile", defaultValue: 0f);
            Scribe_References.Look(ref trailingMinecart, "trailingMinecart");
            Scribe_References.Look(ref leadingMinecart, "leadingMinecart");
        }

        // Update trailing minecart method
        private float UpdateTrailingMinecart()
        {
            if (Spawned)
            {
                if (trailingMinecart != null && trailingMinecart.Spawned)
                {
                    trailingMinecart.speed = speed;
                    trailingMinecart.subtile = subtile;
                    recursiveMass = trailingMinecart.UpdateTrailingMinecart() + this.GetStatValue(StatDefOf.Mass);
                }
                else
                    recursiveMass = this.GetStatValue(StatDefOf.Mass);

                if (leadingMinecart != null && leadingMinecart.Spawned)
                {
                    if (Forward.GetFirstThing<Building_Minecart>(Map) == null || !leadingMinecart.Spawned)
                    {
                        leadingMinecart.trailingMinecart = null;
                        leadingMinecart = null;
                    }
                    if (subtile > 1)
                    {
                        Position = leadingMinecart.Position;
                        Rotation = leadingMinecart.Rotation;
                        subtile--;
                    }
                }
                else
                    leadingMinecart = null;
            }
            else
            {
                if (trailingMinecart != null && !trailingMinecart.Spawned)
                {
                    trailingMinecart.speed = speed;
                    trailingMinecart.subtile = subtile;
                    recursiveMass = trailingMinecart.UpdateTrailingMinecart() + this.GetStatValue(StatDefOf.Mass);
                }
                else
                    recursiveMass = this.GetStatValue(StatDefOf.Mass);
            }
            return (recursiveMass);
        }

        #region Utilities
        // Clearance
        private bool isClear(IntVec3 cell, bool ignoreMinecarts = false, bool ignoreRails = false)
        {
            return isClear(cell, Map, Def, ignoreMinecarts, ignoreRails);
        }

        public static bool isClear(IntVec3 cell, Map map, ThingDef_Minecart def, bool ignoreMinecarts = false, bool ignoreRails = false)
        {
            Building_Door door = cell.GetDoor(map);
            return (cell.Standable(map) || (ignoreMinecarts && cell.GetFirstThing<Building_Minecart>(map) != null))
                && (door != null ? door.Open : true)
                && (cell.GetFirstThing(map, def.railDef) != null || ignoreRails)
                ;
        }

        // Summary:
        //  Applies a force of the given paramaters into the minecart's physics model
        public void Push(float pushSpeed, 
            float pushAccel, float pushPower)
        {
            Speed = pushSpeed + ((Speed - pushSpeed) * (1 - (pushAccel / (1 + recursiveMass / pushPower))));
        }
        
        //Summary:
        //  Instantly sets the minecarts speed to the default launch speed
        public void Launch()
        {
            if (trailingMinecart == null && leadingMinecart == null && !isClear(Position + Rotation.FacingCell))
                Rotation = Rotation.Opposite;
            Speed = Def.launchSpeed;
        }

        public void Brake(float pushAccel, float pushPower)
        {
            Speed = Speed * (1 - (pushAccel / (1 + recursiveMass / pushPower)));
        }

        // Summary:
        //  Instantly sets the minecarts speed to the parameters launch speed
        public void Launch(float launchSpeed)
        {
            if (trailingMinecart == null && leadingMinecart == null && !isClear(Position + Rotation.FacingCell))
                Rotation = Rotation.Opposite;
            speed = launchSpeed;
        }

        private IntVec3 Forward { get { return Position + Rotation.FacingCell; } }
        private IntVec3 Right { get { return Position + Rotation.Rotated(RotationDirection.Clockwise).FacingCell; } }
        private IntVec3 Left { get { return Position + Rotation.Rotated(RotationDirection.Counterclockwise).FacingCell; } }

        // Rail step
        private void doRailStep()
        {
            foreach (Thing thing in Forward.GetThingList(Map).ToList())
            {
                if (thing.Faction.HostileTo(Faction))
                    thing.TakeDamage(new DamageInfo(DamageDefOf.Blunt,
                        Speed * recursiveMass, instigator: this));
            }

            if (isClear(Forward, true)) Position = Forward;

            Building_RailSwitch railSwitch = Position.GetFirstThing<Building_RailSwitch>(Map);

            if (railSwitch != null)
            {
                if (railSwitch.GetComp<CompFlickable>().SwitchIsOn)
                {
                    if (isClear(Right, true)) Rotation = Rotation.Rotated(RotationDirection.Clockwise);
                    else if (isClear(Forward, true)) { }
                    else if (isClear(Left, true)) Rotation = Rotation.Rotated(RotationDirection.Counterclockwise);
                }
                else
                {
                    if (isClear(Left, true)) Rotation = Rotation.Rotated(RotationDirection.Counterclockwise);
                    else if (isClear(Forward, true)) { }
                    else if (isClear(Right, true)) Rotation = Rotation.Rotated(RotationDirection.Clockwise);
                }
            }
            else
            {
                if (isClear(Forward, true)) { }
                else if (isClear(Right, true) && !isClear(Left, true)) Rotation = Rotation.Rotated(RotationDirection.Clockwise);
                else if (isClear(Left, true) && !isClear(Right, true)) Rotation = Rotation.Rotated(RotationDirection.Counterclockwise); 
            }
        }
        #endregion Utilities

        // Gizmos
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            if (Prefs.DevMode)
            {
                Command_Action launch = new Command_Action();

                launch.action = () => headMinecart.Launch();
                launch.defaultLabel = "Launch";
                launch.defaultDesc = "Launch minecart";
                yield return launch;
            }
        }

        // Float menu
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn))
                yield return option;
            yield return new FloatMenuOption("Launch Minecart", () => selPawn.jobs.StartJob(
                new Job(JobDefOf.LaunchCart,
                new LocalTargetInfo(this))));
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(base.GetInspectString());

            sb.AppendFormat("Speed: {0}\n",
                Speed.ToStringWithSign("0.00'c'")
            );

            sb.AppendFormat("Mass: {0} ({1})",
                this.GetStatValue(StatDefOf.Mass).ToString("0.00'kg'"),
                recursiveMass.ToString("0.00'kg'")
            );

            return sb.ToString();
        }


        // Spawn setup
        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                Building_Minecart minecart = Forward.GetFirstThing<Building_Minecart>(map);
                if (minecart != null)
                {
                    leadingMinecart = minecart;
                    minecart.trailingMinecart = this;
                }
                minecart = (Position + Rotation.Opposite.FacingCell).GetFirstThing<Building_Minecart>(map);
                if (minecart != null)
                {
                    trailingMinecart = minecart;
                    minecart.leadingMinecart = this;
                }
            }
        }

        // Tick
        public override void Tick()
        {
            base.Tick();

            if (Spawned)
            {
                if (leadingMinecart == null)
                {
                    if (isClear(Forward))
                    {
                        subtile += speed / GenTicks.TicksPerRealSecond;
                        speed *= Def.frictionCoef;
                    }
                    else if (isClear(Right, true) && !isClear(Left, true)) Rotation = Rotation.Rotated(RotationDirection.Clockwise);
                    else if (isClear(Left, true) && !isClear(Right, true)) Rotation = Rotation.Rotated(RotationDirection.Counterclockwise);
                    else
                    {
                        speed = 0f;
                        subtile = 0f;
                    }

                    UpdateTrailingMinecart();

                    if (subtile > 1)
                    {
                        doRailStep();

                        /* if (Forward.InNoBuildEdgeArea(Map))
                        {
                            WorldObject_Minecart worldObject = new WorldObject_Minecart();
                            worldObject.def = WorldObjectDefOf.MinecartCaravan;
                            worldObject.Tile = Map.Tile;
                            worldObject.minecarts = train;
                            worldObject.direction = Rotation.AsAngle;
                            Find.WorldObjects.Add(worldObject);
                            foreach(Thing thing in train) 
                                thing.DeSpawn();
                        }
                        else */
                            subtile--;
                    }
                }
                else
                {
                    if (!leadingMinecart.Spawned)
                    {
                        leadingMinecart = null;
                        leadingMinecart.trailingMinecart = null;
                    }
                }
            }
            else
            {
                if (leadingMinecart == null)
                {
                    speed *= Def.frictionCoef;
                    UpdateTrailingMinecart();
                }
            }
        }
    }
    
    // Launch job
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef LaunchCart;
    }

    // Rail terrain
    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef ThingRail;
    }

    public class JobDriver_LaunchCart : JobDriver_Flick
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, 20, true, true);
            yield return Toils_General.Do(() =>
            {
                (TargetA.Thing as Building_Minecart).Launch();
            });
        }
    }

    public class PlaceWorker_Minecart : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef bcheckingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            ThingDef_Minecart def = bcheckingDef as ThingDef_Minecart;
            if (!Building_Minecart.isClear(loc, map, def, ignoreRails: true)) return new AcceptanceReport("Blocked");
            else if (!Building_Minecart.isClear(loc, map, def)) return new AcceptanceReport("No rail");
            else if (!Building_Minecart.isClear(loc + rot.FacingCell, map, def, true)) return new AcceptanceReport("Path not clear");
            else return AcceptanceReport.WasAccepted;
        }
    }

    // Small patch that converts all the old terrain rails to the fancy new thing rails
    // Apparently doesn't work tho ;-;
    /*public class GameComponent_RailFixer : GameComponent
    {
        private Game game;

        public GameComponent_RailFixer(Game game)
        {
            this.game = game;
            {
                Log.Message(ThingDefOf.ThingRail.ToString());
            }
        }

        public override void GameComponentTick()
        {
            if (GenTicks.TicksAbs == 2)
            {
                foreach (Map map in game.Maps)
                {
                    foreach (IntVec3 vec3 in map.AllCells)
                    {
                        if (map.terrainGrid.TerrainAt(vec3).defName == "Rail")
                        {
                            map.terrainGrid.RemoveTopLayer(vec3, false);
                            GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("ThingRail"), vec3, map);
                        }
                    }
                }
            }
        }
    }*/
}
