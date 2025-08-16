using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RedAlert
{
    public class MapComponent_RedAlert : MapComponent
    {
        public Dictionary<ThingWithComps, ColorInt> savedColorsPerLamps = new Dictionary<ThingWithComps, ColorInt>();

        public Dictionary<ThingWithComps, int> lampsToEnable = new Dictionary<ThingWithComps, int>();
        public Dictionary<ThingWithComps, int> lampsToDisable = new Dictionary<ThingWithComps, int>();

        public Dictionary<Pawn, Area> pawnsWithOldAreas = new Dictionary<Pawn, Area>();

        public int lastPlayedSoundTick;

        public bool manualTrigger;

        public Sustainer sustainer;

        public static bool debug = false;
        public List<ThingWithComps> CurrentLamps
        {
            get
            {
                List<ThingWithComps> list = new List<ThingWithComps>();
                foreach (var def in Core.allLamps)
                {
                    list.AddRange(map.listerThings.ThingsOfDef(def).Where(x => x.Faction == Faction.OfPlayer && x.TryGetComp<CompRedAlertToggleable>().toggled).OfType<ThingWithComps>());
                }
                return list;
            }
        }
        public MapComponent_RedAlert(Map map) : base(map)
        {

        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (map.IsPlayerHome && ResearchProjectDefOf.ColoredLights.IsFinished)
            {
                var threats = Core.AllActiveThreats(map);
                if (RedAlertMod.settings.autoRestrictUndraftedPawnsToHomeArea)
                {
                    if (threats.Any())
                    {
                        foreach (var colonist in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
                            .Where(x => x.IsColonist && x.Drafted is false))
                        {
                            if (colonist.playerSettings.AreaRestriction != map.areaManager.Home)
                            {
                                pawnsWithOldAreas[colonist] = colonist.playerSettings.AreaRestriction;
                                colonist.playerSettings.AreaRestriction = map.areaManager.Home;
                            }
                        }
                    }
                    else
                    {
                        List<Pawn> keys = new List<Pawn>();
                        foreach (var kvp in pawnsWithOldAreas)
                        {
                            if (kvp.Key.Drafted is false)
                            {
                                kvp.Key.playerSettings.AreaRestriction = kvp.Value;
                                keys.Add(kvp.Key);
                            }
                        }
                        foreach (var key in keys)
                        {
                            pawnsWithOldAreas.Remove(key);
                        }
                    }
                }
                if (threats.Any() || manualTrigger)
                {
                    var threatColor = Core.ThreatColorFor(threats);
                    if (RedAlertMod.settings.playRedAlertSound)
                    {
                        if (RedAlertMod.settings.playLooped)
                        {
                            if (sustainer == null || sustainer.Ended)
                            {
                                sustainer = RA_DefOf.RA_RedAlert_Sustainer.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerTick));
                            }
                            sustainer.Maintain();
                        }
                        else if (Find.TickManager.TicksGame - lastPlayedSoundTick > GenDate.TicksPerHour)
                        {
                            RA_DefOf.RA_RedAlert.PlayOneShotOnCamera();
                            lastPlayedSoundTick = Find.TickManager.TicksGame;
                        }
                    }

                    foreach (var kvp in lampsToEnable.ToList())
                    {
                        if (Find.TickManager.TicksGame >= kvp.Value)
                        {
                            EnableLamp(threatColor, kvp.Key);
                        }
                    }
                    foreach (var kvp in lampsToDisable.ToList())
                    {
                        if (Find.TickManager.TicksGame >= kvp.Value)
                        {
                            lampsToDisable.Remove(kvp.Key);
                            DisableLamp(threatColor, kvp.Key, true);
                        }
                    }

                    foreach (var lamp in CurrentLamps)
                    {
                        if (!lampsToEnable.ContainsKey(lamp))
                        {
                            DisableLamp(threatColor, lamp, false);
                        }
                    }
                }
                else if (savedColorsPerLamps.Any())
                {
                    foreach (var savedLamp in savedColorsPerLamps.ToList())
                    {
                        ResetLamp(savedLamp);
                    }
                }
            }
        }

        public void Message(string message)
        {
            if (debug) 
                Log.Message(message);
        }
        private void EnableLamp(ColorInt threatColor, ThingWithComps lamp)
        {
            lampsToEnable.Remove(lamp);
            Message("Enabling " + lamp);

            if (RedAlertMod.settings.flashingLights)
            {
                lampsToDisable[lamp] = Find.TickManager.TicksGame + 60;
                Message("Should disable " + lamp);
            }

            var comp = lamp.TryGetComp<CompGlower>();
            if (comp != null)
            {
                comp.GlowColor = threatColor;
                map.mapDrawer.MapMeshDirty(comp.parent.Position, MapMeshFlag.Things);
                map.glowGrid.RegisterGlower(comp);
            }
            else
            {
                var compOffset = lamp.AllComps.FirstOrDefault(x => x.GetType().Name == "CompOffsetGlow");
                if (compOffset != null)
                {
                    var compGlower = Core.CompGlowerWallLights.GetValue(compOffset) as CompGlower;
                    Core.SetGlowColorWallLights.Invoke(compOffset, new object[] { threatColor });
                    map.mapDrawer.MapMeshDirty(compOffset.parent.Position, MapMeshFlag.Things);
                    map.glowGrid.RegisterGlower(compGlower);
                    Core.UpdateLitWallLights.Invoke(compOffset, null);
                }
                else
                {
                    var compGlowerExtended = lamp.AllComps.FirstOrDefault(x => x.GetType().Name == "CompGlowerExtended");
                    if (compGlowerExtended != null)
                    {
                        var compGlower = Core.CompGlowerGlowerExtended.GetValue(compGlowerExtended) as CompGlower;
                        Core.SetGlowColorGlowerExtended.Invoke(compGlowerExtended, new object[] { threatColor });
                        map.mapDrawer.MapMeshDirty(compGlowerExtended.parent.Position, MapMeshFlag.Things);
                        map.glowGrid.RegisterGlower(compGlower);
                        Core.UpdateLitGlowerExtended.Invoke(compGlowerExtended, null);
                    }
                }
            }
        }

        private void DisableLamp(ColorInt threatColor, ThingWithComps lamp, bool directDisable)
        {
            var comp = lamp.TryGetComp<CompGlower>();
            if (comp != null)
            {
                if (comp.GlowColor != threatColor || directDisable)
                {
                    if (!savedColorsPerLamps.ContainsKey(lamp))
                    {
                        savedColorsPerLamps[lamp] = comp.GlowColor;
                    }

                    map.mapDrawer.MapMeshDirty(comp.parent.Position, MapMeshFlag.Things);
                    map.glowGrid.DeRegisterGlower(comp);
                    Message("Disabling " + lamp);
                    var tickToEnable = Find.TickManager.TicksGame + 60;
                    lampsToEnable[lamp] = tickToEnable;
                    Message("Should enable " + lamp);
                }
            }

            var compOffset = lamp.AllComps.FirstOrDefault(x => x.GetType().Name == "CompOffsetGlow");
            if (compOffset != null)
            {
                var color = (ColorInt)Core.GetGlowColorWallLights.Invoke(compOffset, null);
                if (color != threatColor || directDisable)
                {
                    if (!savedColorsPerLamps.ContainsKey(lamp))
                    {
                        savedColorsPerLamps[lamp] = color;
                    }
                    Message("Disabling " + lamp);
                    var compGlower = Core.CompGlowerWallLights.GetValue(compOffset) as CompGlower;
                    map.mapDrawer.MapMeshDirty(compOffset.parent.Position, MapMeshFlag.Things);
                    map.glowGrid.DeRegisterGlower(compGlower);
                    var tickToEnable = Find.TickManager.TicksGame + 60;
                    lampsToEnable[lamp] = tickToEnable;
                    Message("Should enable " + lamp);
                }
            }
            else
            {
                var compGlowerExtended = lamp.AllComps.FirstOrDefault(x => x.GetType().Name == "CompGlowerExtended");
                if (compGlowerExtended != null)
                {
                    var color = (ColorInt)Core.GetGlowColorGlowerExtended.Invoke(compGlowerExtended, null);
                    if (color != threatColor || directDisable)
                    {
                        if (!savedColorsPerLamps.ContainsKey(lamp))
                        {
                            savedColorsPerLamps[lamp] = color;
                        }
                        Message("Disabling " + lamp);
                        var compGlower = Core.CompGlowerGlowerExtended.GetValue(compGlowerExtended) as CompGlower;
                        map.mapDrawer.MapMeshDirty(compGlowerExtended.parent.Position, MapMeshFlag.Things);
                        map.glowGrid.DeRegisterGlower(compGlower);
                        var tickToEnable = Find.TickManager.TicksGame + 60;
                        lampsToEnable[lamp] = tickToEnable;
                        Message("Should enable " + lamp);
                    }
                }
            }
        }

        public void ResetLamp(KeyValuePair<ThingWithComps, ColorInt> savedLamp)
        {
            var comp = savedLamp.Key.TryGetComp<CompGlower>();
            if (comp != null)
            {
                comp.GlowColor = savedLamp.Value;
            }
            var compOffset = savedLamp.Key.AllComps.FirstOrDefault(x => x.GetType().Name == "CompOffsetGlow");
            if (compOffset != null)
            {
                var color = (ColorInt)Core.GetGlowColorWallLights.Invoke(compOffset, null);
                Core.SetGlowColorWallLights.Invoke(compOffset, new object[] { savedLamp.Value });
                Core.UpdateLitWallLights.Invoke(compOffset, null);
            }
            else
            {
                var compGlowerExtended = savedLamp.Key.AllComps.FirstOrDefault(x => x.GetType().Name == "CompGlowerExtended");
                if (compGlowerExtended != null)
                {
                    var color = (ColorInt)Core.GetGlowColorGlowerExtended.Invoke(compGlowerExtended, null);
                    Core.SetGlowColorGlowerExtended.Invoke(compGlowerExtended, new object[] { savedLamp.Value });
                    Core.UpdateLitGlowerExtended.Invoke(compGlowerExtended, null);
                }
            }
            savedColorsPerLamps.Remove(savedLamp.Key);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref savedColorsPerLamps, "savedColorsPerLamps", LookMode.Reference, LookMode.Value, ref things, ref colors);
            Scribe_Collections.Look(ref lampsToEnable, "lampsToEnable", LookMode.Reference, LookMode.Value, ref things2, ref ints);
            Scribe_Collections.Look(ref lampsToDisable, "lampsToDisable", LookMode.Reference, LookMode.Value, ref things2, ref ints);
            Scribe_Collections.Look(ref pawnsWithOldAreas, "pawnsWithOldAreas", LookMode.Reference, LookMode.Reference, ref pawns, ref areas);
            Scribe_Values.Look(ref manualTrigger, "manualTrigger");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                savedColorsPerLamps ??= new Dictionary<ThingWithComps, ColorInt>();
                lampsToEnable ??= new Dictionary<ThingWithComps, int>();
                lampsToDisable ??= new Dictionary<ThingWithComps, int>();
                pawnsWithOldAreas ??= new Dictionary<Pawn, Area>();
            }
        }

        public List<ThingWithComps> things = new List<ThingWithComps>();
        public List<ColorInt> colors= new List<ColorInt>();

        public List<ThingWithComps> things2 = new List<ThingWithComps>();
        public List<int> ints = new List<int>();

        public List<Pawn> pawns = new List<Pawn>();
        public List<Area> areas = new List<Area>();
    }
}
