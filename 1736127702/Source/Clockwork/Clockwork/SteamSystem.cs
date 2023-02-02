using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Clockwork
{
    public class SteamSystemMapComp : MapComponent
    {
        public static SteamSystemMapComp loccachecomp = null;
        public static Dictionary<int, SteamSystemMapComp> SteamComps = new Dictionary<int, SteamSystemMapComp>();

        public List<SteamSystem> steamSystems = new List<SteamSystem>();

        public SteamSystemMapComp(Map map) : base(map)
        {
            if (SteamComps.ContainsKey(base.map.uniqueID))
            {
                SteamComps[base.map.uniqueID] = this;
            }
            else
            {
                SteamComps.Add(base.map.uniqueID, this);
            }
            loccachecomp = null;
        }

        public override void MapComponentTick()
        {
            foreach (SteamSystem steamSystem in steamSystems)
            {
                try { steamSystem.Tick(); }
                catch { steamSystems.Remove(steamSystem); }
            }
        }

        public void RegisterSteamSystem(SteamSystem steamSystem)
        {
            if (!steamSystems.Contains(steamSystem))
            {
                steamSystem.steamSystemMapComp = this;
                steamSystems.Add(steamSystem);
            }
        }

        public void UnregisterSteamSystem(SteamSystem steamSystem)
        {
            if (steamSystems.Contains(steamSystem))
            {
                steamSystems.Remove(steamSystem);
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref steamSystems, "steamSystems", LookMode.Deep);
            base.ExposeData();
        }

        //public void DrawLayer()
        //{
        //    if (DebugViewSettings.drawThingsPrinted)
        //    {
        //        Designator_Build val = Find.DesignatorManager.SelectedDesignator as Designator_Build;
        //        if (val != null)
        //        {
        //            ThingDef val2 = val.PlacingDef as ThingDef;
        //            if (val2 != null && val2.comps.OfType<CompProperties_CompSteam>().Any())
        //            {
        //                DrawSteamNets();
        //            }
        //        }
        //    }
        //}

        //private void DrawSteamNets()
        //{
        //    if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap != map)
        //    {
        //        return;
        //    }
        //    foreach (SteamSystem steamSystem in steamSystems)
        //    {
        //        foreach (CompSteam item in steamSystem.steamThingList)
        //        {
        //            foreach (IntVec3 item2 in GenAdj.CellsOccupiedBy(item.parent))
        //            {
        //                CellRenderer.RenderCell(item2, 0.25f);
        //            }
        //        }
        //    }
        //}
    }

    public class SteamSystem : IExposable, ILoadReferenceable
    {

        protected int SystemID = -1;

        public SteamSystem()
        {
            if (SystemID == -1)
            {
                SystemID = Find.UniqueIDsManager.GetNextThingID();
            }
        }

        public SteamSystemMapComp steamSystemMapComp = null;

        //P = ((M/18) * 1000 * 8.20574587 × 10-5 * 373.15K ) / V In atm. Multiply by 101.325 for kPA
        public static float DENSITY_OF_STEAM = 0.6f; //In kg/m3
        public static float STEAM_CONSTANT = 172.363623769f; //Calculated from molucular mass of steam (18 kg/kmol) and gas constant (8.314) assuming 100C (373.15K).

        public float systemVolume = 0; //In m3
        public float systemPressure = 0; //In kPa
        public float systemMaxPressure = 0; //In kPa
        public float systemMaxMass = 0; //In kg
        //public float systemMinPressure = 0; //In kPa
        //public float systemMinConsumption = 0; //In kg/h

        public float steamVolume = 0; //In m3
        public float steamMass = 0; //In kg

        public float steamGenerationSum = 0; //In kg/h
        public float steamConsumptionSum = 0; //In kg/h

        public bool insufficentSteam = false;

        public List<CompSteam> steamThingList = new List<CompSteam>();

        private List<ThingComp_SteamGenerator> steamGeneratorList = new List<ThingComp_SteamGenerator>();
        private List<ThingComp_SteamConsumer> steamConsumerList = new List<ThingComp_SteamConsumer>();

        public void RegisterSteamThing(CompSteam steamThing)
        {
            if (!steamThingList.Contains(steamThing))
            {
                steamThingList.Add(steamThing);
            }

            if (steamThing is ThingComp_SteamGenerator && !steamGeneratorList.Contains(steamThing))
            {
                steamGeneratorList.Add(steamThing as ThingComp_SteamGenerator);
            }
            if (steamThing is ThingComp_SteamConsumer && !steamConsumerList.Contains(steamThing))
            {
                steamConsumerList.Add(steamThing as ThingComp_SteamConsumer);
            }

            UpdateSystem();
        }

        public void UnregisterSteamThing(CompSteam steamThing)
        {
            steamThingList.Remove(steamThing);
            UpdateSystemConnections();
        }

        public void UpdateSystem()
        {
            try
            {
                systemVolume = steamThingList.Sum((CompSteam x) => x.Props.volume);
                systemMaxPressure = steamThingList.Max((CompSteam x) => x.Props.maxPressure);
                systemMaxMass = systemMaxPressure * systemVolume / STEAM_CONSTANT;
                // var minConsumption = steamConsumerList.Min((ThingComp_SteamConsumer x) => x.Props.steamConsumption);
                //systemMinPressure = steamConsumerList.Min((ThingComp_SteamConsumer x) => x.Props.minPressure);
                //systemMinConsumption = 0;
                //foreach (ThingComp_SteamConsumer x in steamConsumerList)
                //{
                //    if (x.Props.minPressure == systemMinPressure)
                //    {
                //        systemMinConsumption += x.Props.steamConsumption;
                //    }
                //}
                // systemMinConsumption = minConsumption * steamConsumerList.Count(c => c.Props.minPressure == minPressure);
            }
            catch { }
        }

        public void UpdateSystemConnections()
        {
            steamGeneratorList.Clear();
            steamConsumerList.Clear();

            foreach (CompSteam steamThing in steamThingList)
            {
                steamThing.steamSystem = null;
            }

            steamThingList[0].steamSystem = this;

            List<CompSteam> oldSteamThingList = new List<CompSteam>(steamThingList);
            steamThingList.Clear();

            foreach (CompSteam steamThing in oldSteamThingList)
            {
                steamThing.RegisterToSteamSystem();
            }

            UpdateSystem();
        }

        public void MergeSystem(SteamSystem system)
        {
            steamMass += system.steamMass;
            system.steamMass = 0;


            foreach (CompSteam steamThing in system.steamThingList)
            {
                if (!steamThingList.Contains(steamThing))
                {
                    RegisterSteamThing(steamThing);
                }
                steamThing.steamSystem = this;
            }

            if (system.steamSystemMapComp != null)
            {
                system.steamSystemMapComp.UnregisterSteamSystem(system);
            }

            UpdateSystem();
        }

        public void Tick()
        {
            steamGenerationSum = steamGeneratorList.Sum((ThingComp_SteamGenerator x) => x.steamOutput);
            steamConsumptionSum = steamConsumerList.Sum((ThingComp_SteamConsumer x) => x.steamNeeded);

            if (steamGenerationSum <= 0)
            {
                steamMass -= 0.01f; // Steam condenses in system
            }
            //else if ((steamGenerationSum < systemMinConsumption) && (systemPressure >= systemMinPressure)) 
            //{
            //    insufficentSteam = true;
            //    return;
            //}
            //else
            //{
            //    insufficentSteam = false;
            //}

            steamMass += (steamGenerationSum - steamConsumptionSum) / 2500f; //Divide by ticks per hour
            steamMass = Mathf.Clamp(steamMass, 0f, systemMaxMass);

            steamVolume = steamMass / DENSITY_OF_STEAM;
            steamVolume = Mathf.Clamp(steamVolume, 0f, systemVolume);

            if (steamVolume == systemVolume)
            {
                systemPressure = steamMass * STEAM_CONSTANT / systemVolume;
                systemPressure = Mathf.Clamp(systemPressure, 0f, systemMaxPressure);
            }
            else
            {
                systemPressure = 0;
            }


        }

        private bool loaded = false;

        public void ExposeData()
        {
            if (SystemID == -1)
            {
                SystemID = Find.UniqueIDsManager.GetNextThingID();
            }

            Scribe_Values.Look(ref steamMass, "steamMass", 0f);

            Scribe_Values.Look(ref SystemID, "SystemID", -1);

            if (!loaded)
            {
                UpdateSystem();
                loaded = true;
            }
        }

        public string GetUniqueLoadID()
        {
            return "SteamSystem_" + SystemID;
        }

        public int ShowSystemID()
        {
            return SystemID;
        }
    }

    public class CompSteam : ThingComp
    {
        public bool registered = false;
        public SteamSystem steamSystem = null;
        public SteamSystemMapComp mapComp;
        public List<IntVec3> cachedAdjCellsCardinal;

        public CompProperties_CompSteam Props => (CompProperties_CompSteam)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            mapComp = base.parent.Map.GetComponent<SteamSystemMapComp>();
            if (!parent.Spawned)
            {
                return;
            }
            CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
            if (comp != null)
            {
                if (!comp.Awake)
                {
                    return;
                }
            }
            else if (parent.Position.Fogged(parent.Map))
            {
                return;
            }
            else
            {
                if (!registered)
                {
                    RegisterToSteamSystem();
                }

            }

            //CreatePipes();

        }

        public override void PostDeSpawn(Map map)
        {
            steamSystem.UnregisterSteamThing(this);
            base.PostDeSpawn(map);
            //RemoveDecorativePipes();
        }

        public void RegisterToSteamSystem()
        {
            if (steamSystem == null)
            {
                TryConnectToSteamSystem();
            }
            steamSystem.RegisterSteamThing(this);
            registered = true;
        }

        public void TryConnectToSteamSystem()
        {
            for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
            {
                List<Thing> thingList = AdjCellsCardinalInBounds[i].GetThingList(parent.Map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    Thing thing = thingList[j];
                    var steamThing = thing.TryGetComp<CompSteam>();
                    if (steamThing != null)
                    {
                        var newSystem = steamThing.SteamSystem();
                        if (newSystem != null)
                        {
                            if (this.steamSystem == null)
                            {
                                this.steamSystem = newSystem;
                            }
                            else if (this.steamSystem != newSystem)
                            {
                                steamSystem.MergeSystem(newSystem);
                            }
                        }
                    }
                }
            }
            if (this.steamSystem == null)
            {
                this.steamSystem = new SteamSystem();
                mapComp.RegisterSteamSystem(steamSystem);
            }
        }

        public List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                if (cachedAdjCellsCardinal == null)
                {
                    cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(parent)
                                              where c.InBounds(parent.Map)
                                              select c).ToList();
                }
                return cachedAdjCellsCardinal;
            }
        }

        public override string CompInspectStringExtra()
        {
            string str = "Unregistered";

            if (registered)
            {
                str = "Steam/system volume: " + Math.Round(steamSystem.steamVolume, 1) + " m³ / " + Math.Round(steamSystem.systemVolume, 1) + " m³" + "\n"
                    + "System pressure: " + Math.Round(steamSystem.systemPressure) + " kPa";

                if (ClockworkAndSteamSettings.showSteamSystemID)
                {
                    str += "\n" + "System ID: " + steamSystem.ShowSystemID();
                }

                return str;
            }

            return null;
        }

        public void PrintForSteamGrid(SectionLayer layer)
        {
            if (Props.pipe)
            {
                SteamOverlayMats.LinkedOverlayGraphic.Print(layer, parent, 0f);
            }
        }

        //public void CreatePipes()
        //{
        //    if (!Props.pipe)
        //    {
        //        foreach (IntVec3 cell in GenAdj.CellsOccupiedBy(parent))
        //        {
        //            List<Thing> list = parent.Map.thingGrid.ThingsListAt(cell);
        //            for (int i = 0; i < list.Count; i++)
        //            {
        //                if (list[i].def.GetCompProperties<CompProperties_CompSteam>().pipe)
        //                {
        //                    return;
        //                }
        //            }

        //            GenSpawn.Spawn(ThingDefOf.SteamPipe, cell, parent.Map).SetFaction(parent.Faction);
        //        }
        //    }
        //}

        //public void RemoveDecorativePipes()
        //{
        //    if (!Props.pipe)
        //    {
        //        foreach (IntVec3 cell in GenAdj.CellsOccupiedBy(parent))
        //        {
        //            List<Thing> list = parent.Map.thingGrid.ThingsListAt(cell);
        //            for (int i = 0; i < list.Count; i++)
        //            {
        //                if (list[i].def.GetCompProperties<CompProperties_CompSteamPipeDecorative>().pipe)
        //                {
        //                    GenSpawn.WipeExistingThings(cell, list[i].Rotation, ThingDefOf.SteamPipeDecorative, parent.Map, DestroyMode.Vanish);
        //                }
        //            }
        //        }
        //    }
        //}

        public SteamSystem SteamSystem()
        {
            return steamSystem;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref steamSystem, "steamSystem");
        }
    }

    public class CompProperties_CompSteam : CompProperties
    {
        public float volume = 1;
        public float maxPressure = 100;
        public bool pipe = false;

        public CompProperties_CompSteam()
        {
            compClass = typeof(CompSteam);
        }
    }

    //public class SteamPipeDecorative : ThingComp
    //{
    //    public void PrintForSteamGrid(SectionLayer layer)
    //    {
    //        SteamOverlayMats.LinkedOverlayGraphic.Print(layer, parent, 0f);
    //    }
    //}

    //public class CompProperties_CompSteamPipeDecorative : CompProperties
    //{
    //    public bool pipe = true;
    //    public CompProperties_CompSteamPipeDecorative()
    //    {
    //        compClass = typeof(SteamPipeDecorative);
    //    }
    //}

    public class ThingComp_SteamGenerator : CompSteam
    {
        public CompProperties_SteamGenerator Props => (CompProperties_SteamGenerator)props;

        protected CompRefuelable refuelableComp;
        protected CompBreakdownable breakdownableComp;
        protected CompFlickable flickableComp;
        protected CompAutoPowered autoPoweredComp;

        private Sustainer sustainerProducingPower;
        private OverlayHandle? overlayPowerOff;
        private OverlayHandle? overlayNeedsPower;

        public float steamOutput;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            refuelableComp = parent.GetComp<CompRefuelable>();
            breakdownableComp = parent.GetComp<CompBreakdownable>();
            flickableComp = parent.GetComp<CompFlickable>();
            autoPoweredComp = parent.GetComp<CompAutoPowered>();
        }

        public override void CompTick()
        {
            base.CompTick();
            UpdateDesiredSteamOutput();
            if (Props.soundAmbientProducingSteam == null)
            {
                return;
            }
            if (steamOutput > 0.01f)
            {
                if (sustainerProducingPower == null || sustainerProducingPower.Ended)
                {
                    sustainerProducingPower = Props.soundAmbientProducingSteam.TrySpawnSustainer(SoundInfo.InMap(parent));
                }
                sustainerProducingPower.Maintain();
            }
            else if (sustainerProducingPower != null)
            {
                sustainerProducingPower.End();
                sustainerProducingPower = null;
            }
        }

        public void UpdateDesiredSteamOutput()
        {
            if ((breakdownableComp != null && breakdownableComp.BrokenDown) || (refuelableComp != null && !refuelableComp.HasFuel) || (flickableComp != null && !flickableComp.SwitchIsOn) || (autoPoweredComp != null && !autoPoweredComp.WantsToBeOn))
            {
                steamOutput = 0f;
            }
            else
            {
                steamOutput = Props.steamGeneration;
            }
        }

        public override string CompInspectStringExtra()
        {
            string str = "Unregistered";

            if (registered)
            {
                str = "Current output: " + steamOutput + " kg/h";
            }

            return str + "\n" + base.CompInspectStringExtra(); ;
        }

    }

    public class CompProperties_SteamGenerator : CompProperties_CompSteam
    {
        public float steamGeneration = 1;
        public SoundDef soundAmbientProducingSteam;

        public CompProperties_SteamGenerator()
        {
            compClass = typeof(ThingComp_SteamGenerator);
        }
    }

    public class ThingComp_SteamConsumer : CompSteam
    {
        public CompProperties_SteamConsumer Props => (CompProperties_SteamConsumer)props;

        protected CompRefuelable refuelableComp;
        protected CompBreakdownable breakdownableComp;
        protected CompFlickable flickableComp;
        protected CompAutoPowered autoPoweredComp;

        public float steamNeeded;
        public bool running = false;

        private OverlayHandle? overlayPowerOff;
        private OverlayHandle? overlayNeedsPower;

        private bool pressureReached => steamSystem.systemPressure > Props.minPressure;
        private bool insufficentSteam => steamSystem.insufficentSteam;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            refuelableComp = parent.GetComp<CompRefuelable>();
            breakdownableComp = parent.GetComp<CompBreakdownable>();
            flickableComp = parent.GetComp<CompFlickable>();
            autoPoweredComp = parent.GetComp<CompAutoPowered>();
        }
        public override void CompTick()
        {
            base.CompTick();
            UpdateDesiredSteamOutput();
        }

        public void UpdateDesiredSteamOutput()
        {
            if ((breakdownableComp != null && breakdownableComp.BrokenDown) || (refuelableComp != null && !refuelableComp.HasFuel) || (flickableComp != null && !flickableComp.SwitchIsOn) || (autoPoweredComp != null && !autoPoweredComp.WantsToBeOn) /*|| !pressureReached || insufficentSteam*/)
            {
                steamNeeded = 0f;
                running = false;
                UpdateOverlays();
            }
            else
            {
                steamNeeded = Props.steamConsumption;
                if (pressureReached)
                {
                    running = true;
                }
                else
                {
                    running = false;
                }
                UpdateOverlays();
            }
        }

        public override string CompInspectStringExtra()
        {
            string str = "Unregistered";
            if (registered)
            {
                if (!running)
                {
                    if (flickableComp != null && !flickableComp.SwitchIsOn)
                    {
                        str = "Switched off";
                    }
                    //else if(insufficentSteam)
                    //{
                    //    str = "Not enough steam production!";
                    //}
                    else
                    {
                        str = "Pressure needed: " + Props.minPressure + " kPa"
                        + "\n" + "Current consumption: " + steamNeeded + " kg/h";
                    }

                }
                else
                {
                    str = "Current consumption: " + steamNeeded + " kg/h";
                }

            }

            return str + "\n" + base.CompInspectStringExtra(); ;
        }

        private void UpdateOverlays()
        {
            if (!parent.Spawned)
            {
                return;
            }
            parent.Map.overlayDrawer.Disable(parent, ref overlayPowerOff);
            parent.Map.overlayDrawer.Disable(parent, ref overlayNeedsPower);
            if (!parent.IsBrokenDown())
            {
                if (flickableComp != null && !flickableComp.SwitchIsOn && !overlayPowerOff.HasValue)
                {
                    overlayPowerOff = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.PowerOff);
                }
                if (FlickUtility.WantsToBeOn(parent) && !running && !overlayNeedsPower.HasValue)
                {
                    overlayNeedsPower = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.NeedsPower);
                }
            }
        }
    }

    public class CompProperties_SteamConsumer : CompProperties_CompSteam
    {
        public float steamConsumption = 1;
        public float minPressure = 100;

        public CompProperties_SteamConsumer()
        {
            compClass = typeof(ThingComp_SteamConsumer);
        }
    }

    public class ThingComp_SteamGauge : CompSteam
    {
        public override string CompInspectStringExtra()
        {
            string str = "Unregistered";
            if (registered)
            {
                str = "System steam mass: " + Math.Round(steamSystem.steamMass) + " kg / " + Math.Round(steamSystem.systemMaxMass) + " kg"
                    + "\n" + "System net mass: " + (steamSystem.steamGenerationSum - steamSystem.steamConsumptionSum) + " kg/h"
                    + "\n" + "System max pressure: " + steamSystem.systemMaxPressure + " kPa";
            }

            return str + "\n" + base.CompInspectStringExtra(); ;
        }
    }

    public class CompProperties_SteamGauge : CompProperties_CompSteam
    {
        public CompProperties_SteamGauge()
        {
            compClass = typeof(ThingComp_SteamGauge);
        }
    }

    public class Building_SteamWorkTable : Building_WorkTable, IBillGiver
    {
        private ThingComp_SteamConsumer steamConsumerComp;
        private CompBreakdownable breakdownableComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            steamConsumerComp = GetComp<ThingComp_SteamConsumer>();
            breakdownableComp = GetComp<CompBreakdownable>();
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public bool CurrentlyUsableForBills()
        {
            if (!UsableForBillsAfterFueling())
            {
                return false;
            }
            if (steamConsumerComp != null && !steamConsumerComp.running)
            {
                return false;
            }
            return true;
        }

        public bool UsableForBillsAfterFueling()
        {
            if (steamConsumerComp != null && !steamConsumerComp.running)
            {
                return false;
            }
            if (breakdownableComp != null && breakdownableComp.BrokenDown)
            {
                return false;
            }
            return true;
        }
    }

    public class CompPowerTraderSteam : CompPowerTrader
    {
        protected ThingComp_SteamConsumer steamConsumerComp;

        private Sustainer sustainerPowered;

        private OverlayHandle? overlayNeedsPower;

        private bool wasPowered = false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            steamConsumerComp = parent.GetComp<ThingComp_SteamConsumer>();
        }

        public override void CompTick()
        {
            if (steamConsumerComp.running)
            {
                if (wasPowered)
                {
                    return;
                }
                else
                {
                    base.PowerOn = true;
                    parent.Map.overlayDrawer.Disable(parent, ref overlayNeedsPower);

                    StartSustainerPoweredIfInactive();

                    wasPowered = true;
                }
            }
            else
            {
                if (wasPowered)
                {
                    base.PowerOn = false;

                    EndSustainerPoweredIfActive();

                    wasPowered = false;
                }

                parent.Map.overlayDrawer.Disable(parent, ref overlayNeedsPower);
            }
        }

        public override string CompInspectStringExtra()
        {
            if (steamConsumerComp == null)
            {
                return "ERROR: MISSING STEAMCOMP";
            }

            if (base.PowerOn)
            {
                return "Operational";
            }
            else
            {
                return "Awaiting pressure...";
            }

        }

        public override void ReceiveCompSignal(string signal) { }

        private void StartSustainerPoweredIfInactive()
        {
            CompProperties_Power props = base.Props;
            if (!props.soundAmbientPowered.NullOrUndefined() && sustainerPowered == null)
            {
                SoundInfo info = SoundInfo.InMap(parent);
                sustainerPowered = props.soundAmbientPowered.TrySpawnSustainer(info);
            }
        }

        private void EndSustainerPoweredIfActive()
        {
            if (sustainerPowered != null)
            {
                sustainerPowered.End();
                sustainerPowered = null;
            }
        }

    }

    public class CompPowerPlantSteam : CompPowerTraderSteam
    {

        //private Sustainer sustainerProducingPower;
        private bool outputtingPower = false;
        protected float desiredPowerOutput;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            steamConsumerComp = parent.GetComp<ThingComp_SteamConsumer>();
            desiredPowerOutput = 0f - Props.basePowerConsumption;
            if (Props.transmitsPower || parent.def.ConnectToPower)
            {
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: false);
                if (Props.transmitsPower)
                {
                    parent.Map.powerNetManager.Notify_TransmitterSpawned(this);
                }
                if (parent.def.ConnectToPower)
                {
                    parent.Map.powerNetManager.Notify_ConnectorWantsConnect(this);
                }
            }
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            //if (sustainerProducingPower != null && !sustainerProducingPower.Ended)
            //{
            //    sustainerProducingPower.End();
            //}
        }

        public override void CompTick()
        {
            if (desiredPowerOutput > 0f && steamConsumerComp.running)
            {
                base.PowerOutput = desiredPowerOutput;
                outputtingPower = true;
            }
            else if (desiredPowerOutput > 0f && !steamConsumerComp.running)
            {
                base.PowerOutput = 0f;
                outputtingPower = false;
            }

            //if (base.Props.soundAmbientProducingPower == null)
            //{
            //    return;
            //}
            //if (outputtingPower)
            //{
            //    if (sustainerProducingPower == null || sustainerProducingPower.Ended)
            //    {
            //        sustainerProducingPower = base.Props.soundAmbientProducingPower.TrySpawnSustainer(SoundInfo.InMap(parent));
            //    }
            //    sustainerProducingPower.Maintain();
            //}
            //else if (sustainerProducingPower != null)
            //{
            //    sustainerProducingPower.End();
            //    sustainerProducingPower = null;
            //}
        }
        public override string CompInspectStringExtra()
        {
            if (PowerNet == null)
            {
                return "PowerNotConnected".Translate();
            }
            string str = ("PowerOutput".Translate() + ": " + PowerOutput.ToString("#####0") + " W");
            string value = (PowerNet.CurrentEnergyGainRate() / WattsToWattDaysPerTick).ToString("F0");
            string value2 = PowerNet.CurrentStoredEnergy().ToString("F0");
            return str + "\n" + "PowerConnectedRateStored".Translate(value, value2);
        }

    }

    public class SectionLayer_ThingsSteamGrid : SectionLayer_Things
    {
        public SectionLayer_ThingsSteamGrid(Section section)
            : base(section)
        {
            requireAddToMapMesh = false;
            relevantChangeTypes = MapMeshFlag.Buildings;
        }

        public override void DrawLayer()
        {
            Designator_Build val = Find.DesignatorManager.SelectedDesignator as Designator_Build;
            if (val != null)
            {
                ThingDef val2 = val.PlacingDef as ThingDef;
                if (val2 != null && val2.comps.OfType<CompProperties_CompSteam>().Any())
                {
                    base.DrawLayer();
                }
            }
        }

        public override void Regenerate()
        {
            ClearSubMeshes(MeshParts.All);
            foreach (IntVec3 item in section.CellRect)
            {
                List<Thing> list = base.Map.thingGrid.ThingsListAt(item);
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    Thing thing = list[i];
                    if ((thing.def.seeThroughFog || !base.Map.fogGrid.fogGrid[CellIndicesUtility.CellToIndex(thing.Position, base.Map.Size.x)]) && thing.def.drawerType != 0 && (thing.def.drawerType != DrawerType.RealtimeOnly || !requireAddToMapMesh) && (!(thing.def.hideAtSnowDepth < 1f) || !(base.Map.snowGrid.GetDepth(thing.Position) > thing.def.hideAtSnowDepth)) && thing.Position.x == item.x && thing.Position.z == item.z)
                    {
                        TakePrintFrom(thing);
                    }
                }
            }
            FinalizeMesh(MeshParts.All);
        }

        protected override void TakePrintFrom(Thing t)
        {
            if (t.Faction == null || t.Faction == Faction.OfPlayer)
            {
                var comp = t.TryGetComp<CompSteam>();
                comp?.PrintForSteamGrid(this);
                //var comp2 = t.TryGetComp<SteamPipeDecorative>();
                //comp2?.PrintForSteamGrid(this);
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class SteamOverlayMats
    {

        private static readonly Shader TransmitterShader;

        public static readonly Graphic LinkedOverlayGraphic;

        static SteamOverlayMats()
        {
            TransmitterShader = ShaderDatabase.MetaOverlay;
            Graphic graphic = GraphicDatabase.Get<Graphic_Single>("Things/Special/Steam/SteamAtlas", TransmitterShader);
            LinkedOverlayGraphic = GraphicUtility.WrapLinked(graphic, LinkDrawerType.Basic);
        }
    }
}
