using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace JobEffects
{
    /// <summary>
    /// Dev-only one-click setup that builds a "job showcase" on the current map: powered
    /// workbenches with forever-bills + ingredients, field-work designations (mine, chop,
    /// harvest, sow, smooth, deconstruct, repair, build, clean), a tameable animal, and a
    /// crew of skilled colonists with every work type enabled — so every animated tool and
    /// effect can be observed firing at once. Open via the debug browser: bug icon →
    /// Actions tab → "Show Me Your Tools" category → "Setup job showcase".
    /// </summary>
    public static class ShowcaseSetup
    {
        private static readonly string[] Benches =
        {
            "FueledSmithy", "ElectricSmelter", "ElectricStove", "Campfire",
            "HandTailoringBench", "ElectricTailoringBench", "TableSculpting",
            "TableStonecutter", "TableButcher", "TableMachining", "FabricationBench",
            "DrugLab", "Brewery", "SimpleResearchBench"
        };

        // Variety for the mine/smooth rows and deconstruct wall stuffs, so the new material-aware
        // chips read as visibly different colours per material (granite grey, marble pale, …).
        private static readonly string[] Rocks = { "Granite", "Marble", "Sandstone", "Limestone", "Slate" };
        private static readonly string[] WallStuffs = { "Steel", "BlocksGranite", "BlocksMarble", "WoodLog", "Plasteel" };

        [DebugAction("Show Me Your Tools", "Setup job showcase", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SetupShowcase()
        {
            Map map = Find.CurrentMap;
            if (map == null) { Messages.Message("No current map.", MessageTypeDefOf.RejectInput, false); return; }

            int benchesDone = 0;
            var report = new List<string>();

            // Finish everything (so bench recipes are available), then reopen ONE leaf project
            // and make it the active research so the Notepad (Research) tool has work to show.
            Try(report, "research", () =>
            {
                ResearchManager rm = Find.ResearchManager;
                rm.DebugSetAllProjectsFinished();
                var all = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
                var prereqd = new HashSet<ResearchProjectDef>();
                foreach (ResearchProjectDef pr in all)
                {
                    if (pr.prerequisites != null) foreach (var q in pr.prerequisites) prereqd.Add(q);
                    if (pr.hiddenPrerequisites != null) foreach (var q in pr.hiddenPrerequisites) prereqd.Add(q);
                }
                ResearchProjectDef leaf = all
                    .Where(p => !prereqd.Contains(p) && p.requiredResearchBuilding == null
                           && (p.requiredResearchFacilities == null || p.requiredResearchFacilities.Count == 0)
                           && p.TechprintCount == 0)
                    .OrderByDescending(p => p.baseCost)
                    .FirstOrDefault()
                    ?? all.FirstOrDefault(p => !prereqd.Contains(p));
                if (leaf != null)
                {
                    FieldInfo progField = typeof(ResearchManager).GetField("progress", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (progField?.GetValue(rm) is Dictionary<ResearchProjectDef, float> dict) dict[leaf] = 0f;
                    rm.SetCurrentProject(leaf);
                }
            });

            IntVec3 origin = map.Center;

            // --- Powered workbench row -------------------------------------------------
            int benchZ = origin.z + 7;
            int conduitZ = benchZ - 1;
            int x = origin.x - 24;
            int firstX = x;
            var placedBenches = new List<Building>();

            // Anomaly DoBill benches (bioferrite shaper + serum centrifuge) — only when Anomaly is
            // active, so the two new bench tools (BioferriteChisel / SerumSpoon) get a real billed
            // bench + parked worker. Appended conditionally so there's no missing-def noise otherwise.
            var benchList = new List<string>(Benches);
            if (ModsConfig.AnomalyActive) { benchList.Add("BioferriteShaper"); benchList.Add("SerumCentrifuge"); }

            foreach (string defName in benchList)
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def == null) { report.Add($"missing bench def {defName}"); continue; }
                int w = def.size.x;
                IntVec3 cell = new IntVec3(x + w / 2, 0, benchZ);
                Try(report, defName, () =>
                {
                    ClearFootprint(def, cell, Rot4.South, map);
                    Thing t = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                    t.SetFactionDirect(Faction.OfPlayer);
                    GenSpawn.Spawn(t, cell, map, Rot4.South, WipeMode.Vanish);
                    if (t is Building b)
                    {
                        placedBenches.Add(b);
                        b.TryGetComp<CompRefuelable>()?.Refuel(99999f);
                    }
                });
                x += w + 2;
            }

            // Carpet a conduit row in front of the benches and drop charged batteries.
            Try(report, "power grid", () =>
            {
                ThingDef conduit = DefDatabase<ThingDef>.GetNamedSilentFail("PowerConduit");
                for (int cx = firstX - 1; cx <= x + 1; cx++)
                {
                    IntVec3 c = new IntVec3(cx, 0, conduitZ);
                    if (!c.InBounds(map)) continue;
                    ClearFootprint(conduit, c, Rot4.North, map);
                    Thing co = ThingMaker.MakeThing(conduit);
                    co.SetFactionDirect(Faction.OfPlayer);
                    GenSpawn.Spawn(co, c, map, Rot4.North, WipeMode.Vanish);
                }
                ThingDef batteryDef = DefDatabase<ThingDef>.GetNamedSilentFail("Battery");
                foreach (int bx in new[] { firstX + 2, firstX + 16, x - 4 })
                {
                    IntVec3 bc = new IntVec3(bx, 0, conduitZ - 1);
                    if (!bc.InBounds(map)) continue;
                    ClearFootprint(batteryDef, bc, Rot4.North, map);
                    Thing bat = ThingMaker.MakeThing(batteryDef);
                    bat.SetFactionDirect(Faction.OfPlayer);
                    GenSpawn.Spawn(bat, bc, map, Rot4.North, WipeMode.Vanish);
                    bat.TryGetComp<CompPowerBattery>()?.SetStoredEnergyPct(1f);
                }
            });

            // Add a forever-bill + ingredients to every bench that can take one.
            foreach (Building b in placedBenches)
            {
                Try(report, "bill " + b.def.defName, () =>
                {
                    if (AddShowcaseBill(b, map)) benchesDone++;
                });
            }

            // --- Field work ------------------------------------------------------------
            int fz = origin.z - 4;       // first field row (south of origin)
            Try(report, "mine", () => MineVariety(map, origin.x - 26, fz, 3));
            Try(report, "smoothwall", () => SmoothVariety(map, origin.x - 20, fz, 2));
            Try(report, "smoothfloor", () => SmoothFloorPatch(map, origin.x - 14, fz, 3, 3));
            Try(report, "chop", () => Cluster(map, origin.x - 9, fz, 3, 3, c => SpawnPlant(map, c, "Plant_TreeOak", DesignationDefOf.CutPlant)));
            Try(report, "harvest", () => Cluster(map, origin.x - 3, fz, 3, 3, c => SpawnPlant(map, c, "Plant_Potato", DesignationDefOf.HarvestPlant)));
            Try(report, "sow", () => SowZone(map, origin.x + 3, fz, 4, 3));
            Try(report, "clean", () => Cluster(map, origin.x + 9, fz, 4, 3, c => FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Dirt, 1)));
            Try(report, "deconstruct", () => DeconstructVariety(map, origin.x + 14, fz));
            Try(report, "repair", () => Cluster(map, origin.x + 17, fz, 1, 3, c => SpawnWall(map, c, null, true)));
            // wood / stone / steel / grave frames so the hammer, build-chisel, welder and
            // grave-shovel FinishFrame tools all fire.
            Try(report, "build", () => BuildFrameVariety(map, origin.x + 20, fz));
            // Crowbar's new job: a removable built-floor patch flagged to rip up. RemoveFoundation
            // (bridges) uses the identical crowbar pry, so this one patch validates both new claims.
            Try(report, "removefloor", () => RemoveFloorPatch(map, origin.x - 14, origin.z - 8, 3, 3));

            // Mark the whole area as home so cleaning / repair / construction run.
            Try(report, "home area", () =>
            {
                foreach (IntVec3 c in CellRect.FromLimits(origin.x - 26, origin.z - 15, x + 2, benchZ + 2).ClipInsideMap(map))
                    map.areaManager.Home[c] = true;
            });

            // --- Tameable animal -------------------------------------------------------
            Try(report, "tame target", () =>
            {
                Pawn animal = PawnGenerator.GeneratePawn(PawnKindDef.Named("Muffalo"), null);
                IntVec3 c = CellFinder.RandomClosewalkCellNear(new IntVec3(origin.x + 6, 0, origin.z), map, 4);
                GenSpawn.Spawn(animal, c, map);
                map.designationManager.AddDesignation(new Designation(animal, DesignationDefOf.Tame));
            });

            // --- New-tool stations (a row further south) --------------------------------
            int nz = origin.z - 12;
            Try(report, "slaughter", () => SlaughterAnimals(map, new IntVec3(origin.x - 22, 0, nz), 3));
            Try(report, "shear", () => ShearAnimals(map, new IntVec3(origin.x - 15, 0, nz), 3));
            Try(report, "paint", () => PaintWalls(map, origin.x - 8, nz, 4));
            Try(report, "deepdrill", () => DeepDrillStation(map, new IntVec3(origin.x - 1, 0, nz)));
            Try(report, "firefight", () => FireStation(map, origin.x + 5, nz, 3));
            Try(report, "medical", () => MedicalStation(map, origin.x + 12, nz, 6));
            Try(report, "surgery", () => SurgeryStation(map, origin.x + 18, nz));

            // --- Recreation / faith / learning + special-job stations -------------------
            // Tools whose jobs had no showcase setup before. Joy / faith / learning / mech /
            // hive / ignite jobs have no clean player work path, so a dedicated colonist is
            // spawned and the job force-issued; designation-backed jobs also get the designation.
            int rz = origin.z - 20;    // south row A (compact stations)
            int rz2 = origin.z - 28;   // south row B (wide stations)
            Try(report, "ignite", () => IgniteStation(map, origin.x - 30, rz));
            Try(report, "fillin", () => FillInStation(map, origin.x - 24, rz));
            Try(report, "hack", () => HackStation(map, origin.x - 18, rz));
            Try(report, "rearmturret", () => RearmTurretStation(map, origin.x - 12, rz));
            Try(report, "extractskull", () => ExtractSkullStation(map, origin.x - 6, rz));
            Try(report, "telescope", () => TelescopeStation(map, origin.x, rz));
            Try(report, "instrument", () => InstrumentStation(map, origin.x + 6, rz));
            Try(report, "meditation", () => MeditationStation(map, origin.x + 12, rz));
            Try(report, "maintain", () => MaintainStation(map, origin.x + 18, rz));
            Try(report, "repairmech", () => RepairMechStation(map, origin.x + 24, rz));
            Try(report, "floordrawing", () => FloordrawingStation(map, origin.x + 30, rz));
            Try(report, "fishing", () => FishingStation(map, origin.x + 36, rz));
            Try(report, "genexenogerm", () => GeneAssemblerStation(map, origin.x - 16, rz2));
            Try(report, "school", () => SchoolStation(map, origin.x + 8, rz2));

            // --- Modded-tool stations (Medieval Overhaul + Dubs Bad Hygiene) ------------
            // A southern block stands up every modded workbench any bench tool maps to (reusing
            // the lineup bench builder, skipping the vanilla benches already placed above), so MO
            // bench tools work for real. Then dedicated stations for the modded jobs that aren't a
            // vanilla DoBill: MO plow-soil (hoe) + mending (needle's custom job), and the three
            // DBH hygiene jobs (scrub brush / plunger / muck scoop). Each silently no-ops when its
            // mod/defs aren't present, so this stays inert without the mods.
            int rz3 = origin.z - 36;
            Try(report, "dbh stations", () => DubsBadHygieneStations(map, origin.x - 30, rz3));
            Try(report, "mo plow", () => PlowSoilStation(map, origin.x - 8, rz3));
            Try(report, "mo mending", () => MendingStation(map, origin.x + 2, rz3));
            Try(report, "modded benches", () => ModdedBenchRow(map, new IntVec3(origin.x - 34, 0, origin.z - 44)));

            // --- Colonists -------------------------------------------------------------
            Try(report, "colonists", () =>
            {
                // Cycle every humanlike body type across the crew so each one's arm anchoring can be
                // verified side-by-side. Forcing story.bodyType then SetAllGraphicsDirty re-renders
                // the body with that silhouette (gender-independent in RimWorld).
                var bodyTypes = new List<BodyTypeDef>();
                foreach (string n in new[] { "Thin", "Male", "Female", "Fat", "Hulk" })
                {
                    BodyTypeDef bt = DefDatabase<BodyTypeDef>.GetNamedSilentFail(n);
                    if (bt != null) bodyTypes.Add(bt);
                }

                for (int i = 0; i < 16; i++)
                {
                    Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                    IntVec3 c = CellFinder.RandomClosewalkCellNear(origin, map, 10);
                    GenSpawn.Spawn(p, c, map);
                    if (bodyTypes.Count > 0 && p.story != null)
                    {
                        p.story.bodyType = bodyTypes[i % bodyTypes.Count];
                        p.Drawer?.renderer?.SetAllGraphicsDirty();
                    }
                    if (p.skills != null)
                        foreach (SkillRecord s in p.skills.skills) { if (!s.TotallyDisabled) { s.Level = 14; s.passion = Passion.Major; } }
                    p.workSettings?.EnableAndInitialize();
                    foreach (WorkTypeDef wt in DefDatabase<WorkTypeDef>.AllDefs)
                        if (!p.WorkTypeIsDisabled(wt)) p.workSettings.SetPriority(wt, 3);
                }
            });

            string msg = $"Show Me Your Tools showcase ready: {benchesDone}/{placedBenches.Count} benches billed, 5 stone types staged for material-tinted chips, 16 colonists.";
            if (report.Count > 0) msg += " Issues: " + string.Join(", ", report.Take(8));
            Messages.Message(msg, MessageTypeDefOf.TaskCompletion, false);
            Messages.Message("Tip: compare chip colours across the 5 mined stones; let a worker finish a job and walk off to watch the tool holster then fade; the surgery patients (far right) get an amputation (bonesaw) and a peg-leg install (scalpel); use 'Advance 1 quadrum' to recolour the harvest/chop leaves by season; rip up the concrete patch (mid-west) with the crowbar.",
                MessageTypeDefOf.NeutralEvent, false);
            Messages.Message("Two extra rows to the south stage the rest of the tools: torch/shovel/hack/turret/skull/telescope/harp/meditation/wrench-hive/mech, plus a gene assembler, a school desk and a fishing pond (DLC-gated). Most are forced one-shot demos — re-run the setup to replay them. The build row now stages wood, stone, steel and grave frames so the hammer, build-chisel, welder and grave-shovel all fire.",
                MessageTypeDefOf.NeutralEvent, false);
            Messages.Message("Modded coverage (only if the mod is active): a southern bench block stands up every Medieval Overhaul workbench (spindle, hide scraper, smith hammer, furnace, alchemy, jewelry, …) with bills + workers; plus MO plow-soil (hoe) and mending (needle), and the three Dubs Bad Hygiene jobs (scrub brush / plunger / muck scoop). Inactive mods are skipped silently.",
                MessageTypeDefOf.NeutralEvent, false);
        }

        // Flip the map's season (for testing seasonal leaf tint) by advancing game time one
        // quadrum — TicksAbs = ticksGame + gameStartAbsTick, and season derives from TicksAbs.
        [DebugAction("Show Me Your Tools", "Advance 1 quadrum (season)", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AdvanceSeason()
        {
            TickManager tm = Find.TickManager;
            tm.DebugSetTicksGame(tm.TicksGame + GenDate.TicksPerQuadrum);
            Map map = Find.CurrentMap;
            string s = map != null ? GenLocalDate.Season(map).ToString() : "?";
            Messages.Message($"Show Me Your Tools: +1 quadrum — season is now {s}. Fell a tree or harvest a crop to see the leaf tint.",
                MessageTypeDefOf.TaskCompletion, false);
        }

        // ---- New-tool stations ----------------------------------------------------------

        // Slaughter (knife): a few player-owned animals marked for slaughter.
        private static void SlaughterAnimals(Map map, IntVec3 near, int n)
        {
            for (int i = 0; i < n; i++)
            {
                Pawn a = PawnGenerator.GeneratePawn(PawnKindDef.Named("Muffalo"), Faction.OfPlayer);
                GenSpawn.Spawn(a, CellFinder.RandomClosewalkCellNear(near, map, 3), map);
                if (map.designationManager.DesignationOn(a, DesignationDefOf.Slaughter) == null)
                    map.designationManager.AddDesignation(new Designation(a, DesignationDefOf.Slaughter));
            }
        }

        // Shear (shears): player-owned woolly animals with their wool topped up to full.
        private static void ShearAnimals(Map map, IntVec3 near, int n)
        {
            FieldInfo fullnessField = typeof(CompHasGatherableBodyResource)
                .GetField("fullness", BindingFlags.Instance | BindingFlags.NonPublic);
            for (int i = 0; i < n; i++)
            {
                Pawn a = PawnGenerator.GeneratePawn(PawnKindDef.Named("Muffalo"), Faction.OfPlayer);
                GenSpawn.Spawn(a, CellFinder.RandomClosewalkCellNear(near, map, 3), map);
                CompShearable cs = a.TryGetComp<CompShearable>();
                if (cs != null) fullnessField?.SetValue(cs, 1f);
            }
        }

        // Paint (paintbrush): steel walls flagged for painting in a structure colour.
        private static void PaintWalls(Map map, int x0, int z0, int n)
        {
            ThingDef wallDef = DefDatabase<ThingDef>.GetNamedSilentFail("Wall");
            if (wallDef == null) return;
            ColorDef color = DefDatabase<ColorDef>.AllDefsListForReading.FirstOrDefault(c => c.colorType == ColorType.Structure)
                             ?? DefDatabase<ColorDef>.AllDefsListForReading.FirstOrDefault();
            if (color == null) return;
            for (int i = 0; i < n; i++)
            {
                IntVec3 c = new IntVec3(x0 + i, 0, z0);
                if (!c.InBounds(map)) continue;
                ClearFootprint(wallDef, c, Rot4.North, map);
                Thing wall = ThingMaker.MakeThing(wallDef, ThingDefOf.Steel);
                wall.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(wall, c, map, Rot4.North, WipeMode.Vanish);
                map.designationManager.AddDesignation(new Designation(wall, DesignationDefOf.PaintBuilding, color));
            }
        }

        // Deep drill (auger): a player deep drill over a patch of deep resources to mine.
        private static void DeepDrillStation(Map map, IntVec3 center)
        {
            ThingDef drillDef = DefDatabase<ThingDef>.GetNamedSilentFail("DeepDrill");
            if (drillDef == null) return;
            ClearFootprint(drillDef, center, Rot4.South, map);
            Thing drill = ThingMaker.MakeThing(drillDef, drillDef.MadeFromStuff ? GenStuff.DefaultStuffFor(drillDef) : null);
            drill.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(drill, center, map, Rot4.South, WipeMode.Vanish);
            ThingDef res = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.deepCommonality > 0f) ?? ThingDefOf.Steel;
            foreach (IntVec3 c in GenRadial.RadialCellsAround(center, 5f, true))
                if (c.InBounds(map)) map.deepResourceGrid.SetAt(c, res, 800);
        }

        // Beat fire (fire blanket): small fires on a stone firebreak so colonists fight them.
        private static void FireStation(Map map, int x0, int z0, int n)
        {
            TerrainDef stone = DefDatabase<TerrainDef>.GetNamedSilentFail("FlagstoneGranite")
                               ?? DefDatabase<TerrainDef>.GetNamedSilentFail("Concrete");
            for (int dx = -2; dx <= n + 1; dx++)
                for (int dz = -2; dz <= 2; dz++)
                {
                    IntVec3 c = new IntVec3(x0 + dx, 0, z0 + dz);
                    if (!c.InBounds(map)) continue;
                    foreach (Thing t in c.GetThingList(map).ToList())
                        if (t.def.category == ThingCategory.Plant || t is Filth) t.Destroy();
                    if (stone != null) map.terrainGrid.SetTerrain(c, stone);
                }
            for (int i = 0; i < n; i++)
            {
                IntVec3 c = new IntVec3(x0 + i, 0, z0);
                if (c.InBounds(map)) FireUtility.TryStartFireIn(c, map, 0.4f, null);
            }
        }

        // Tend: a row of medical beds with genuinely-injured (downed) patients, plus a stockpile of
        // ALL THREE medicine types. Each patient's medical care is set so the doctor reaches for a
        // specific medicine — patients cycle herbal / industrial / glitterworld — which exercises the
        // herbal bundle, default kit, and glitterworld kit tend animations in one station.
        private static void MedicalStation(Map map, int x0, int z0, int n)
        {
            ThingDef bedDef = DefDatabase<ThingDef>.GetNamedSilentFail("HospitalBed")
                              ?? DefDatabase<ThingDef>.GetNamedSilentFail("Bed");

            // Stock all three medicine tiers so the doctor can honor each patient's care setting.
            ThingDef herbal = DefDatabase<ThingDef>.GetNamedSilentFail("MedicineHerbal");
            ThingDef industrial = DefDatabase<ThingDef>.GetNamedSilentFail("MedicineIndustrial");
            ThingDef glitter = DefDatabase<ThingDef>.GetNamedSilentFail("MedicineUltratech");
            if (herbal != null) DropStacks(herbal, 16, new IntVec3(x0, 0, z0 + 2), map);
            if (industrial != null) DropStacks(industrial, 16, new IntVec3(x0 + 2, 0, z0 + 2), map);
            if (glitter != null) DropStacks(glitter, 16, new IntVec3(x0 + 4, 0, z0 + 2), map);

            // Per-patient care: cycles herbal -> industrial -> glitterworld so each medicine (and
            // therefore each tend kit) gets used. HerbalOrWorse forces herbal; NormalOrWorse caps at
            // industrial; Best reaches for glitterworld when available.
            MedicalCareCategory[] careCycle =
            {
                MedicalCareCategory.HerbalOrWorse,
                MedicalCareCategory.NormalOrWorse,
                MedicalCareCategory.Best,
            };

            // TWO PASSES, deliberately. Interleaving "spawn bed i / spawn patient i" meant the
            // `bb.Medical = true` setter on bed i+1 could run while an already-downed patient was
            // lying on its cell; the setter kicks occupants, and RestUtility.KickOutOfBed then logs
            // "Tried to kick pawn X out of a bed they're not currently in" (the pawn's CurrentBed is
            // a DIFFERENT bed to the one being cleared). Building every bed first means no Medical
            // setter ever fires while a patient occupies a bed cell.
            for (int i = 0; i < n; i++)
            {
                IntVec3 bedCell = new IntVec3(x0 + i * 2, 0, z0);
                if (bedDef == null || !bedCell.InBounds(map)) continue;
                ClearFootprint(bedDef, bedCell, Rot4.North, map);
                Thing bed = ThingMaker.MakeThing(bedDef, bedDef.MadeFromStuff ? GenStuff.DefaultStuffFor(bedDef) : null);
                bed.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(bed, bedCell, map, Rot4.North, WipeMode.Vanish);
                if (bed is Building_Bed bb && bedDef.building != null && bedDef.building.bed_canBeMedical) bb.Medical = true;
            }

            for (int i = 0; i < n; i++)
            {
                // Downed patients are laid on a deterministic cell SOUTH of the bed row, never via
                // RandomClosewalkCellNear — that could land a patient on a neighbouring bed's cell,
                // which is what made them look "in bed" to the kick-out path in the first place.
                IntVec3 rest = new IntVec3(x0 + i * 2, 0, z0 + 1);
                if (!rest.InBounds(map) || !rest.Standable(map) || rest.GetEdifice(map) is Building_Bed)
                {
                    if (!ToolGallery.TryFindOpenCell(map, rest, out rest)) continue;
                }
                Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                GenSpawn.Spawn(p, rest, map);
                if (p.playerSettings != null)
                    p.playerSettings.medCare = careCycle[i % careCycle.Length];
                // Real injuries that down the pawn (so a doctor rescues + tends them) but stop short
                // of bleeding wounds, so nobody bleeds out before treatment.
                HealthUtility.DamageUntilDowned(p, allowBleedingWounds: false);
            }
        }

        // Surgery (surgical saw + scalpel): two downed patients on medical beds with queued
        // operations — an amputation (RemoveBodyPart -> saw) and a peg-leg install
        // (InstallPegLeg -> scalpel). Patients are downed (blunt, no bleeding) so a free colonist
        // rescues them into a medical bed, after which a doctor performs the operation.
        private static void SurgeryStation(Map map, int x0, int z0)
        {
            ThingDef bedDef = DefDatabase<ThingDef>.GetNamedSilentFail("HospitalBed")
                              ?? DefDatabase<ThingDef>.GetNamedSilentFail("Bed");
            RecipeDef remove = DefDatabase<RecipeDef>.GetNamedSilentFail("RemoveBodyPart");
            RecipeDef peg = DefDatabase<RecipeDef>.GetNamedSilentFail("InstallPegLeg");
            ThingDef med = DefDatabase<ThingDef>.GetNamedSilentFail("MedicineIndustrial")
                           ?? DefDatabase<ThingDef>.GetNamedSilentFail("MedicineHerbal");
            ThingDef wood = DefDatabase<ThingDef>.GetNamedSilentFail("WoodLog");
            if (med != null) DropStacks(med, 16, new IntVec3(x0, 0, z0 + 2), map);
            if (wood != null) DropStacks(wood, 20, new IntVec3(x0, 0, z0 + 2), map);

            // Beds first, patients second — see the note in MedicalStation. Interleaving lets a later
            // bed's `Medical = true` setter kick a patient who is lying on a DIFFERENT bed's cell,
            // which trips RestUtility.KickOutOfBed's "not currently in" consistency error.
            for (int i = 0; i < 2; i++)
            {
                IntVec3 bedCell = new IntVec3(x0 + i * 2, 0, z0);
                if (bedDef == null || !bedCell.InBounds(map)) continue;
                ClearFootprint(bedDef, bedCell, Rot4.North, map);
                Thing b = ThingMaker.MakeThing(bedDef, bedDef.MadeFromStuff ? GenStuff.DefaultStuffFor(bedDef) : null);
                b.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(b, bedCell, map, Rot4.North, WipeMode.Vanish);
                if (b is Building_Bed bb && bedDef.building != null && bedDef.building.bed_canBeMedical) bb.Medical = true;
            }

            for (int i = 0; i < 2; i++)
            {
                // Deterministic cell south of the bed row; never RandomClosewalkCellNear, which could
                // drop a downed patient onto a neighbouring bed.
                IntVec3 rest = new IntVec3(x0 + i * 2, 0, z0 + 1);
                if (!rest.InBounds(map) || !rest.Standable(map) || rest.GetEdifice(map) is Building_Bed)
                {
                    if (!ToolGallery.TryFindOpenCell(map, rest, out rest)) continue;
                }
                Pawn patient = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                GenSpawn.Spawn(patient, rest, map);
                HealthUtility.DamageUntilDowned(patient, false, DamageDefOf.Blunt);

                BodyPartRecord leg = patient.RaceProps.body.AllParts
                    .FirstOrDefault(p => p.def.defName == "Leg" && !patient.health.hediffSet.PartIsMissing(p));
                if (leg == null) continue;
                RecipeDef recipe = ((i == 0) ? remove : peg) ?? remove ?? peg;
                if (recipe != null)
                    HealthCardUtility.CreateSurgeryBill(patient, recipe, leg, null, false);
            }
        }

        // ==== Recreation / faith / learning + special-job stations =====================
        // Each stages a "station" for a tool that had no showcase setup. Where the job has no
        // natural player work path, a dedicated colonist is spawned and the job force-issued so
        // the animation fires at least once; designation-backed jobs also get the designation so
        // the driver's own checks pass. Every call is wrapped in Try() at the call site, so a
        // missing def or inactive DLC just skips that station.

        // Spawn a fully-work-enabled colonist near a cell (for forced one-shot demos).
        private static Pawn SpawnWorker(Map map, IntVec3 near)
        {
            IntVec3 c = near;
            if (!c.InBounds(map) || !c.Standable(map)) ToolGallery.TryFindOpenCell(map, near, out c);
            Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
            GenSpawn.Spawn(p, c, map);
            EnableAllWork(p);
            return p;
        }

        // A child colonist (Biotech) in the learning life stage, for floordrawing / school.
        private static Pawn SpawnChild(Map map, IntVec3 near)
        {
            PawnGenerationRequest req = new PawnGenerationRequest(
                PawnKindDefOf.Colonist, Faction.OfPlayer,
                forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Child,
                fixedBiologicalAge: 8f, fixedChronologicalAge: 8f);
            Pawn child = PawnGenerator.GeneratePawn(req);
            IntVec3 c = near;
            if (!c.InBounds(map) || !c.Standable(map)) ToolGallery.TryFindOpenCell(map, near, out c);
            GenSpawn.Spawn(child, c, map);
            return child;
        }

        private static void Force(Pawn p, Job job)
        {
            if (p != null && job != null) p.jobs?.TryTakeOrderedJob(job, JobTag.Misc);
        }

        private static ThingDef FirstDef(params string[] names)
        {
            foreach (string n in names)
            {
                ThingDef d = DefDatabase<ThingDef>.GetNamedSilentFail(n);
                if (d != null) return d;
            }
            return null;
        }

        private static TerrainDef FirstTerrain(params string[] names)
        {
            foreach (string n in names)
            {
                TerrainDef d = DefDatabase<TerrainDef>.GetNamedSilentFail(n);
                if (d != null) return d;
            }
            return null;
        }

        // FishingRod (Fish): a small painted pond zoned for fishing, with a forced angler on the
        // shore. JobDriver_Fish only requires the target cell to sit in an Allowed Zone_Fishing
        // (allowed defaults true), so the casting pose plays even though an artificial pond yields
        // no catch. (Odyssey)
        private static void FishingStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.OdysseyActive) return;
            JobDef fish = DefDatabase<JobDef>.GetNamedSilentFail("Fish");
            TerrainDef water = FirstTerrain("WaterDeep", "WaterOceanDeep", "WaterShallow", "WaterMovingShallow", "Marsh");
            if (fish == null || water == null) return;
            IntVec3 stand = new IntVec3(x0, 0, z0 - 1);
            if (!stand.InBounds(map)) return;
            ToolGallery.ClearPad(map, new IntVec3(x0 + 1, 0, z0), 3.5f);

            // paint a 3x2 pond north of the shore
            var waterCells = new List<IntVec3>();
            for (int dx = 0; dx < 3; dx++)
                for (int dz = 0; dz < 2; dz++)
                {
                    IntVec3 wc = new IntVec3(x0 + dx, 0, z0 + dz);
                    if (!wc.InBounds(map) || map.zoneManager.ZoneAt(wc) != null) continue;
                    foreach (Thing t in wc.GetThingList(map).ToList())
                        if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
                    map.terrainGrid.SetTerrain(wc, water);
                    waterCells.Add(wc);
                }
            if (waterCells.Count == 0) return;

            // a solid shore cell for the angler to stand on
            TerrainDef ground = FirstTerrain("Soil", "Sand", "Gravel");
            foreach (Thing t in stand.GetThingList(map).ToList())
                if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
            if (ground != null) map.terrainGrid.SetTerrain(stand, ground);

            Zone_Fishing zone = new Zone_Fishing(map.zoneManager);
            map.zoneManager.RegisterZone(zone);
            foreach (IntVec3 wc in waterCells) zone.AddCell(wc);

            IntVec3 fishCell = new IntVec3(x0, 0, z0);   // pond cell directly north of the shore
            Force(SpawnWorker(map, stand), JobMaker.MakeJob(fish, fishCell, stand));
        }

        private static Thing SpawnBuilding(Map map, ThingDef def, IntVec3 center, Rot4 rot, bool playerFaction = true)
        {
            ClearFootprint(def, center, rot, map);
            Thing t = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
            if (playerFaction) t.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(t, center, map, rot, WipeMode.Vanish);
            return t;
        }

        // Torch (Ignite): no player work path — force a colonist to set a wood pile alight.
        // Placed south of the home area so nobody auto-fights the fire.
        private static void IgniteStation(Map map, int x0, int z0)
        {
            JobDef ignite = DefDatabase<JobDef>.GetNamedSilentFail("Ignite");
            if (ignite == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2f);
            Thing wood = ThingMaker.MakeThing(ThingDefOf.WoodLog);
            wood.stackCount = 75;
            Thing placed = GenSpawn.Spawn(wood, c, map);
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 2)), JobMaker.MakeJob(ignite, placed));
        }

        // Shovel (FillIn): a crater designated to be filled in, plus a forced filler.
        private static void FillInStation(Map map, int x0, int z0)
        {
            ThingDef craterDef = FirstDef("CraterSmall", "CraterMedium", "CraterLarge");
            JobDef fillIn = DefDatabase<JobDef>.GetNamedSilentFail("FillIn");
            if (craterDef == null || fillIn == null || DesignationDefOf.FillIn == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2f);
            Thing crater = SpawnBuilding(map, craterDef, c, Rot4.North, false);
            if (map.designationManager.DesignationOn(crater, DesignationDefOf.FillIn) == null)
                map.designationManager.AddDesignation(new Designation(crater, DesignationDefOf.FillIn));
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 2)), JobMaker.MakeJob(fillIn, crater));
        }

        // HackDevice (Hack): an ancient hackable terminal designated to hack. (Ideology)
        private static void HackStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.IdeologyActive) return;
            ThingDef terminal = FirstDef("AncientTerminal", "AncientTerminal_Worshipful");
            JobDef hack = DefDatabase<JobDef>.GetNamedSilentFail("Hack");
            DesignationDef hackDes = DefDatabase<DesignationDef>.GetNamedSilentFail("Hack");
            if (terminal == null || hack == null || hackDes == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2.5f);
            Thing t = SpawnBuilding(map, terminal, c, Rot4.South, false);
            if (map.designationManager.DesignationOn(t, hackDes) == null)
                map.designationManager.AddDesignation(new Designation(t, hackDes));
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 2)), JobMaker.MakeJob(hack, t));
        }

        // Shell (RearmTurret): a player mini-turret with an empty barrel + a steel pile, so a
        // worker auto-rearms it (WorkGiver_Refuel_Turret — no designation needed).
        private static void RearmTurretStation(Map map, int x0, int z0)
        {
            ThingDef turretDef = FirstDef("Turret_MiniTurret");
            if (turretDef == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2.5f);
            Thing t = SpawnBuilding(map, turretDef, c, Rot4.South, true);
            CompRefuelable fuel = t.TryGetComp<CompRefuelable>();
            if (fuel != null && fuel.Fuel > 0f) fuel.ConsumeFuel(fuel.Fuel);   // drain so it needs rearming
            DropStacks(ThingDefOf.Steel, 150, new IntVec3(x0, 0, z0 - 2), map);
            foreach (IntVec3 cc in GenRadial.RadialCellsAround(c, 5f, true))
                if (cc.InBounds(map)) map.areaManager.Home[cc] = true;
            SpawnWorker(map, new IntVec3(x0, 0, z0 + 2));
        }

        // Bonesaw (ExtractSkull): a fresh human corpse designated for skull extraction. (Ideology)
        private static void ExtractSkullStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.IdeologyActive) return;
            JobDef extract = DefDatabase<JobDef>.GetNamedSilentFail("ExtractSkull");
            if (extract == null || DesignationDefOf.ExtractSkull == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2f);
            Pawn dead = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, null);
            GenSpawn.Spawn(dead, c, map);
            dead.Kill(null);
            Corpse corpse = dead.Corpse;
            if (corpse == null || !corpse.Spawned) return;
            if (map.designationManager.DesignationOn(corpse, DesignationDefOf.ExtractSkull) == null)
                map.designationManager.AddDesignation(new Designation(corpse, DesignationDefOf.ExtractSkull));
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 2)), JobMaker.MakeJob(extract, corpse));
        }

        // Spyglass (UseTelescope): a telescope + a forced stargazer.
        private static void TelescopeStation(Map map, int x0, int z0)
        {
            ThingDef tel = FirstDef("Telescope");
            JobDef use = DefDatabase<JobDef>.GetNamedSilentFail("UseTelescope");
            if (tel == null || use == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 3f);
            Thing t = SpawnBuilding(map, tel, c, Rot4.South, true);
            Job job = t.def.hasInteractionCell ? JobMaker.MakeJob(use, t, t.InteractionCell) : JobMaker.MakeJob(use, t);
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 3)), job);
        }

        // Recorder (Play_MusicalInstrument): a harp + a forced player. (Royalty)
        private static void InstrumentStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.RoyaltyActive) return;
            ThingDef inst = FirstDef("Harp", "Harpsichord", "Piano");
            JobDef play = DefDatabase<JobDef>.GetNamedSilentFail("Play_MusicalInstrument");
            if (inst == null || play == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 3f);
            Thing t = SpawnBuilding(map, inst, c, Rot4.South, true);
            Job job = t.def.hasInteractionCell ? JobMaker.MakeJob(play, t, t.InteractionCell) : JobMaker.MakeJob(play, t);
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 3)), job);
        }

        // Prayer beads (Meditate): a meditation spot + a forced meditator. (Royalty)
        private static void MeditationStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.RoyaltyActive) return;
            ThingDef spotDef = FirstDef("MeditationSpot");
            JobDef meditate = DefDatabase<JobDef>.GetNamedSilentFail("Meditate");
            if (spotDef == null || meditate == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 3f);
            SpawnBuilding(map, spotDef, c, Rot4.North, true);
            Force(SpawnWorker(map, new IntVec3(x0 + 1, 0, z0)), JobMaker.MakeJob(meditate, c));
        }

        // Wrench (Maintain): a dormant insect hive (the only CompMaintainable thing) + a forced
        // maintainer. Spawned to the player faction and left dormant so it never spawns insects.
        private static void MaintainStation(Map map, int x0, int z0)
        {
            ThingDef hiveDef = FirstDef("Hive");
            JobDef maintain = DefDatabase<JobDef>.GetNamedSilentFail("Maintain");
            if (hiveDef == null || maintain == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2.5f);
            Thing hive = SpawnBuilding(map, hiveDef, c, Rot4.North, true);
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 2)), JobMaker.MakeJob(maintain, hive));
        }

        // Screwdriver (RepairMech): a damaged player mech held still + a forced repairer. (Biotech)
        private static void RepairMechStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.BiotechActive) return;
            PawnKindDef mechKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Mech_Lifter");
            JobDef repair = DefDatabase<JobDef>.GetNamedSilentFail("RepairMech");
            if (mechKind == null || repair == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if ((!c.InBounds(map) || !c.Standable(map)) && !ToolGallery.TryFindOpenCell(map, c, out c)) return;
            ToolGallery.ClearPad(map, c, 2.5f);
            Pawn mech = PawnGenerator.GeneratePawn(mechKind, Faction.OfPlayer);
            GenSpawn.Spawn(mech, c, map);
            Job hold = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture);
            hold.expiryInterval = -1;
            mech.jobs.StartJob(hold, JobCondition.InterruptForced);
            mech.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 18f));   // an injury so MechRepairUtility.CanRepair is true

            // The repairer MUST be a mechanitor. MechRepairSpeed is declared with
            // <workerClass>StatWorker_Mechanitor</workerClass>, whose ShouldShowFor returns
            // MechanitorUtility.IsMechanitor(pawn) — so on a plain colonist the stat is DISABLED, and
            // vanilla's repair driver trips its own "Attempted to calculate value for disabled stat
            // MechRepairSpeed" consistency check the moment the forced job starts. A mechlink implant
            // is the cheapest way to make the pawn legitimately eligible.
            Pawn repairer = SpawnWorker(map, new IntVec3(x0, 0, z0 + 2));
            HediffDef mechlink = DefDatabase<HediffDef>.GetNamedSilentFail("MechlinkImplant");
            if (repairer != null && mechlink != null
                && repairer.health?.hediffSet?.GetFirstHediffOfDef(mechlink) == null)
            {
                try { repairer.health.AddHediff(mechlink); } catch { }
            }
            Force(repairer, JobMaker.MakeJob(repair, mech));
        }

        // GeneSyringe (CreateXenogerm): a powered gene assembler + bank (loaded with the
        // least-complex of several generated genepacks) + gene processors for complexity
        // headroom, started and worked by a forced colonist. (Biotech)
        private static void GeneAssemblerStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.BiotechActive) return;
            ThingDef asmDef = FirstDef("GeneAssembler");
            ThingDef bankDef = FirstDef("GeneBank");
            ThingDef procDef = FirstDef("GeneProcessor");
            JobDef create = DefDatabase<JobDef>.GetNamedSilentFail("CreateXenogerm");
            if (asmDef == null || bankDef == null || create == null) return;
            IntVec3 asmCell = new IntVec3(x0 + 1, 0, z0);
            if (!asmCell.InBounds(map)) return;
            ToolGallery.ClearPad(map, asmCell, 7f);

            PowerRow(map, x0 - 4, x0 + 9, z0 - 1);   // conduit line + charged battery one row south

            Thing bank = SpawnBuilding(map, bankDef, new IntVec3(x0 - 2, 0, z0), Rot4.South, true);
            Thing asm = SpawnBuilding(map, asmDef, asmCell, Rot4.South, true);
            if (procDef != null)
            {
                SpawnBuilding(map, procDef, new IntVec3(x0 + 5, 0, z0), Rot4.North, true);
                SpawnBuilding(map, procDef, new IntVec3(x0 + 8, 0, z0), Rot4.North, true);
            }

            CompGenepackContainer cont = bank.TryGetComp<CompGenepackContainer>();
            Genepack best = null;
            if (cont != null)
            {
                var temp = new List<Genepack>();
                for (int i = 0; i < 6; i++) temp.Add((Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack));
                best = temp.OrderBy(PackComplexity).FirstOrDefault();
                foreach (Genepack g in temp)
                {
                    if (g == best && cont.innerContainer.TryAdd(g, false)) continue;
                    g.Destroy();
                }
            }

            if (cont != null && best != null && cont.ContainedGenepacks.Contains(best) && asm is Building_GeneAssembler assembler)
                assembler.Start(new List<Genepack> { best }, 0, "Showcase xenotype", XenotypeIconDefOf.Basic);

            Force(SpawnWorker(map, new IntVec3(x0 + 1, 0, z0 + 3)), JobMaker.MakeJob(create, asm));
        }

        private static int PackComplexity(Genepack g)
        {
            int sum = 0;
            if (g?.GeneSet != null)
                foreach (GeneDef gene in g.GeneSet.GenesListForReading) sum += gene.biostatCpx;
            return sum;
        }

        // Crayon (Floordrawing): a child forced to scribble on the floor in front of itself. (Biotech)
        private static void FloordrawingStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.BiotechActive) return;
            JobDef draw = DefDatabase<JobDef>.GetNamedSilentFail("Floordrawing");
            if (draw == null) return;
            IntVec3 stand = new IntVec3(x0, 0, z0);
            IntVec3 target = new IntVec3(x0, 0, z0 - 1);
            if (!stand.InBounds(map) || !target.InBounds(map)) return;
            ToolGallery.ClearPad(map, stand, 2.5f);
            Pawn child = SpawnChild(map, stand);
            Force(child, JobMaker.MakeJob(draw, stand, target));
        }

        // TeachingBook (Lessongiving): a school desk with a child student + an adult teacher; the
        // paired lesson jobs are force-issued so each side's mutual-job check passes. (Biotech)
        private static void SchoolStation(Map map, int x0, int z0)
        {
            if (!ModsConfig.BiotechActive) return;
            ThingDef deskDef = FirstDef("SchoolDesk");
            JobDef giving = DefDatabase<JobDef>.GetNamedSilentFail("Lessongiving");
            JobDef taking = DefDatabase<JobDef>.GetNamedSilentFail("Lessontaking");
            if (deskDef == null || giving == null || taking == null) return;
            IntVec3 deskCell = new IntVec3(x0, 0, z0);
            if (!deskCell.InBounds(map)) return;
            ToolGallery.ClearPad(map, deskCell, 3.5f);
            Thing desk = SpawnBuilding(map, deskDef, deskCell, Rot4.South, true);
            Pawn student = SpawnChild(map, new IntVec3(x0 - 1, 0, z0 + 2));
            Pawn teacher = SpawnWorker(map, new IntVec3(x0 + 1, 0, z0 + 2));
            if (student == null || teacher == null) return;
            student.jobs?.TryTakeOrderedJob(JobMaker.MakeJob(taking, desk, teacher), JobTag.Misc);
            teacher.jobs?.TryTakeOrderedJob(JobMaker.MakeJob(giving, desk, student), JobTag.Misc);
        }

        // A conduit line + one charged battery (powers the gene-assembler station).
        private static void PowerRow(Map map, int xFrom, int xTo, int z)
        {
            ThingDef conduit = DefDatabase<ThingDef>.GetNamedSilentFail("PowerConduit");
            ThingDef batteryDef = DefDatabase<ThingDef>.GetNamedSilentFail("Battery");
            if (conduit != null)
                for (int x = xFrom; x <= xTo; x++)
                {
                    IntVec3 c = new IntVec3(x, 0, z);
                    if (!c.InBounds(map)) continue;
                    ClearFootprint(conduit, c, Rot4.North, map);
                    Thing co = ThingMaker.MakeThing(conduit);
                    co.SetFactionDirect(Faction.OfPlayer);
                    GenSpawn.Spawn(co, c, map, Rot4.North, WipeMode.Vanish);
                }
            if (batteryDef != null)
            {
                IntVec3 bc = new IntVec3(xFrom, 0, z - 1);
                if (bc.InBounds(map))
                {
                    ClearFootprint(batteryDef, bc, Rot4.North, map);
                    Thing bat = ThingMaker.MakeThing(batteryDef);
                    bat.SetFactionDirect(Faction.OfPlayer);
                    GenSpawn.Spawn(bat, bc, map, Rot4.North, WipeMode.Vanish);
                    bat.TryGetComp<CompPowerBattery>()?.SetStoredEnergyPct(1f);
                }
            }
        }

        // ==== Modded-tool stations (Medieval Overhaul + Dubs Bad Hygiene) ================
        // All gated by def resolution: when the mod isn't active its defNames don't resolve and
        // the station silently no-ops, exactly like the DLC-gated stations above.

        // Stand up a real billed station for EVERY modded workbench any bench tool maps to,
        // reusing the lineup bench builder. The vanilla benches placed in the main row (plus the
        // mending bench, billed separately via the damaged-item path) are skipped so they aren't
        // duplicated. Surfaces MO's spindle / hide-scraper / smith-hammer / furnace / alchemy /
        // jewelry / spoon-at-ovens / timber-axe / bench-pick / bench-shovel / quill, and any
        // future modded bench, with no per-mod code here.
        private static void ModdedBenchRow(Map map, IntVec3 rowOrigin)
        {
            var benchTools = DefDatabase<JobToolDef>.AllDefsListForReading
                .Where(t => t != null && !t.surgeryOnly
                    && t.jobDefs != null && t.jobDefs.Contains("DoBill")
                    && t.workbenchDefs != null && t.workbenchDefs.Count > 0)
                .ToList();
            if (benchTools.Count == 0) return;
            var skip = new HashSet<string>(Benches);
            if (ModsConfig.AnomalyActive) { skip.Add("BioferriteShaper"); skip.Add("SerumCentrifuge"); }
            skip.Add("DankPyon_MendingBench");   // billed via MendingStation (damaged-item path)
            BuildLineupBenchStations(map, rowOrigin, benchTools,
                new List<Pawn>(), new List<Thing>(), new List<JobToolDef>(), skip);
        }

        // DBH: three hygiene stations, one per DBH tool.
        private static void DubsBadHygieneStations(Map map, int x0, int z0)
        {
            BedpanStation(map, x0, z0);          // scrub brush (cleanBedpan)
            BlockageStation(map, x0 + 6, z0);    // plunger     (clearBlockage)
            LatrineStation(map, x0 + 12, z0);    // muck scoop  (emptyLatrine)
        }

        // cleanBedpan -> scrub brush. A player bedpan + a forced cleaner.
        private static void BedpanStation(Map map, int x0, int z0)
        {
            ThingDef bedpanDef = FirstDef("BedPan");
            JobDef clean = DefDatabase<JobDef>.GetNamedSilentFail("cleanBedpan");
            if (bedpanDef == null || clean == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2.5f);
            Thing pan = SpawnBuilding(map, bedpanDef, c, Rot4.South, true);
            FlagHome(map, c, 4f);
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 2)), JobMaker.MakeJob(clean, pan));
        }

        // clearBlockage -> plunger. A DBH sewage outlet forced into a blocked state (its
        // CompBlockage.DoBreakdown registers it with the map's blockage manager so the WorkGiver
        // sees it), then a forced plumber. CompBlockage is reached by reflection (DBH unreferenced).
        private static void BlockageStation(Map map, int x0, int z0)
        {
            ThingDef outletDef = FirstDef("SewageOutlet");
            JobDef clear = DefDatabase<JobDef>.GetNamedSilentFail("clearBlockage");
            if (outletDef == null || clear == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2.5f);
            Thing outlet = SpawnBuilding(map, outletDef, c, Rot4.South, true);
            ThingComp blockage = (outlet as ThingWithComps)?.AllComps
                ?.FirstOrDefault(cp => cp.GetType().Name == "CompBlockage");
            blockage?.GetType().GetMethod("DoBreakdown", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(blockage, null);
            FlagHome(map, c, 4f);
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 2)), JobMaker.MakeJob(clear, outlet));
        }

        // emptyLatrine -> muck scoop. A pit latrine filled to its limit (public sewage field on
        // Building_Latrine, set by reflection), then a forced emptier.
        private static void LatrineStation(Map map, int x0, int z0)
        {
            ThingDef latrineDef = FirstDef("PitLatrine");
            JobDef empty = DefDatabase<JobDef>.GetNamedSilentFail("emptyLatrine");
            if (latrineDef == null || empty == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 2.5f);
            Thing latrine = SpawnBuilding(map, latrineDef, c, Rot4.South, true);
            System.Type lt = latrine.GetType();
            FieldInfo sewF = lt.GetField("sewage");
            if (sewF != null)
            {
                float lim = 100f;
                FieldInfo limF = lt.GetField("sewageLimit");
                try { if (limF != null) lim = System.Convert.ToSingle(limF.GetValue(latrine)); } catch { }
                try { sewF.SetValue(latrine, lim > 0f ? lim : 100f); } catch { }
            }
            FlagHome(map, c, 4f);
            Force(SpawnWorker(map, new IntVec3(x0, 0, z0 + 2)), JobMaker.MakeJob(empty, latrine));
        }

        // MO hoe: a soil patch with plowed-soil terrain blueprints. The general crew plows them
        // (a FinishFrame on the DankPyon_PlowedSoil TerrainDef, which the hoe claims via
        // buildTerrainDefs).
        private static void PlowSoilStation(Map map, int x0, int z0)
        {
            TerrainDef plowed = DefDatabase<TerrainDef>.GetNamedSilentFail("DankPyon_PlowedSoil");
            if (plowed == null) return;
            TerrainDef soil = FirstTerrain("Soil", "SoilRich", "Sand");
            for (int i = 0; i < 4; i++)
            {
                IntVec3 c = new IntVec3(x0 + i, 0, z0);
                if (!c.InBounds(map)) continue;
                foreach (Thing t in c.GetThingList(map).ToList())
                    if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
                if (soil != null) map.terrainGrid.SetTerrain(c, soil);
                GenConstruct.PlaceBlueprintForBuild(plowed, c, map, Rot4.North, Faction.OfPlayer, null);
                map.areaManager.Home[c] = true;
            }
        }

        // MO mending: the mending bench with forever bills for its recipe(s) + a stack of damaged
        // apparel. MO's WorkGiver_DoMending issues the custom DankPyon_DoBillMending job, which the
        // sewing needle now claims (its targetA is the bench).
        private static void MendingStation(Map map, int x0, int z0)
        {
            ThingDef benchDef = FirstDef("DankPyon_MendingBench");
            if (benchDef == null) return;
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            ToolGallery.ClearPad(map, c, 3.5f);
            PowerRow(map, x0 - 3, x0 + 3, z0 - 1);
            Thing bench = SpawnBuilding(map, benchDef, c, Rot4.South, true);
            (bench as Building)?.TryGetComp<CompRefuelable>()?.Refuel(99999f);
            if (bench is IBillGiver giver)
                foreach (RecipeDef recipe in bench.def.AllRecipes)
                {
                    if (recipe == null || !recipe.AvailableNow) continue;
                    Bill bill = recipe.MakeNewBill();
                    if (bill is Bill_Production prod) prod.repeatMode = BillRepeatModeDefOf.Forever;
                    bill.ingredientSearchRadius = 999f;
                    giver.BillStack.AddBill(bill);
                }
            foreach (string ap in new[] { "Apparel_Pants", "Apparel_BasicShirt", "Apparel_Tuque", "Apparel_Parka" })
            {
                ThingDef apDef = DefDatabase<ThingDef>.GetNamedSilentFail(ap);
                if (apDef == null) continue;
                Thing item = ThingMaker.MakeThing(apDef, apDef.MadeFromStuff ? GenStuff.DefaultStuffFor(apDef) : null);
                item.HitPoints = Mathf.Max(1, item.MaxHitPoints / 5);
                GenPlace.TryPlaceThing(item, new IntVec3(x0, 0, z0 + 2), map, ThingPlaceMode.Near);
            }
            FlagHome(map, c, 5f);
        }

        // Flag a radius around a cell as home so haul / clean / work runs there.
        private static void FlagHome(Map map, IntVec3 center, float radius)
        {
            foreach (IntVec3 cc in GenRadial.RadialCellsAround(center, radius, true))
                if (cc.InBounds(map)) map.areaManager.Home[cc] = true;
        }

        // ---- Lineup bench stations (real working benches, one worker each) ---------------

        /// <summary>
        /// Lineup support: stand up a real, powered + fueled, forever-billed workbench (with a
        /// worker parked at it) for EVERY workbench any bench tool maps to — vanilla AND modded.
        /// Instead of picking just the first bench per tool, this spawns one station per distinct
        /// resolvable workbenchDef across <paramref name="benchTools"/>, so modded-mod compat
        /// (VFE - Production, Stoneborn - Cuisine, Medieval Overhaul, Combat Extended, …) is fully
        /// exercised: every patched-in bench gets its own billed station + worker. Benches from an
        /// inactive mod (def doesn't resolve) are skipped silently. Stations are laid in rows that
        /// wrap once wide, with a conduit + battery island under each row and the whole region
        /// flagged home. A tool whose benches ALL fail to station (none resolve, or none have a
        /// provisionable recipe) is returned in <paramref name="failed"/> so the caller can fall
        /// back to a frozen pose for it. Spawned things append to <paramref name="props"/> /
        /// <paramref name="workers"/> for caller cleanup.
        /// </summary>
        public static void BuildLineupBenchStations(Map map, IntVec3 rowOrigin,
            List<JobToolDef> benchTools, List<Pawn> workers, List<Thing> props, List<JobToolDef> failed,
            HashSet<string> skipBenchDefs = null)
        {
            if (map == null || benchTools == null) return;

            // Finish research so every bench recipe is craftable.
            try { Find.ResearchManager.DebugSetAllProjectsFinished(); } catch { }

            // Distinct resolvable workbench defs across ALL bench tools, each mapped back to its tool.
            // Spawning per-DEF (not first-bench-per-tool) is what surfaces every modded bench in the
            // lineup; defs from an inactive mod just don't resolve and are skipped.
            var benchDefs = new List<ThingDef>();
            var benchToTool = new Dictionary<ThingDef, JobToolDef>();
            var seenName = new HashSet<string>();
            foreach (JobToolDef tool in benchTools)
            {
                if (tool?.workbenchDefs == null) continue;
                foreach (string n in tool.workbenchDefs)
                {
                    if (!seenName.Add(n)) continue;
                    if (skipBenchDefs != null && skipBenchDefs.Contains(n)) continue;   // already placed elsewhere
                    ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(n);
                    if (def == null || benchToTool.ContainsKey(def)) continue;
                    benchToTool[def] = tool;
                    benchDefs.Add(def);
                }
            }
            if (benchDefs.Count == 0)
            {
                foreach (JobToolDef tool in benchTools) failed.Add(tool);
                return;
            }

            const int maxRowSpan = 60;   // cells of width before a row wraps
            const int rowPitch = 7;      // vertical cells between bench rows (bench + worker + ingredients)

            int firstX = rowOrigin.x;
            int x = firstX;
            int benchZ = rowOrigin.z;
            int rightMostX = firstX;
            var interactionRows = new List<int> { benchZ - 1 };
            var placed = new List<KeyValuePair<Building, JobToolDef>>();

            foreach (ThingDef def in benchDefs)
            {
                int w = Mathf.Max(1, def.size.x);
                if (x > firstX && x - firstX + w > maxRowSpan)   // wrap to a new row
                {
                    rightMostX = Mathf.Max(rightMostX, x);
                    x = firstX;
                    benchZ -= rowPitch;
                    interactionRows.Add(benchZ - 1);
                }
                IntVec3 cell = new IntVec3(x + w / 2, 0, benchZ);
                if (!cell.InBounds(map)) { x += w + 5; continue; }

                Building b = null;
                try
                {
                    ClearFootprint(def, cell, Rot4.South, map);
                    Thing t = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null);
                    t.SetFactionDirect(Faction.OfPlayer);
                    GenSpawn.Spawn(t, cell, map, Rot4.South, WipeMode.Vanish);
                    b = t as Building;
                    if (b != null) { b.TryGetComp<CompRefuelable>()?.Refuel(99999f); props.Add(b); }
                }
                catch { }
                if (b != null) placed.Add(new KeyValuePair<Building, JobToolDef>(b, benchToTool[def]));
                x += w + 5;     // gap for the ingredient pile + the parked worker
            }
            rightMostX = Mathf.Max(rightMostX, x);
            int bottomZ = benchZ - 1;

            // Clear plants/filth across the whole bench region so workers can stand on bare ground.
            try
            {
                foreach (IntVec3 c in CellRect.FromLimits(firstX - 1, bottomZ - 1, rightMostX + 1, rowOrigin.z + 1).ClipInsideMap(map))
                    foreach (Thing t in c.GetThingList(map).ToList())
                        if (t.def.category == ThingCategory.Plant || t is Filth) t.Destroy();
            }
            catch { }

            // Conduit + battery island under EACH bench row so the electric benches have power.
            try
            {
                ThingDef conduit = DefDatabase<ThingDef>.GetNamedSilentFail("PowerConduit");
                ThingDef batteryDef = DefDatabase<ThingDef>.GetNamedSilentFail("Battery");
                foreach (int iz in interactionRows)
                {
                    if (conduit != null)
                        for (int cx = firstX - 1; cx <= rightMostX + 1; cx++)
                        {
                            IntVec3 c = new IntVec3(cx, 0, iz);
                            if (!c.InBounds(map)) continue;
                            ClearFootprint(conduit, c, Rot4.North, map);
                            Thing co = ThingMaker.MakeThing(conduit);
                            co.SetFactionDirect(Faction.OfPlayer);
                            GenSpawn.Spawn(co, c, map, Rot4.North, WipeMode.Vanish);
                            props.Add(co);
                        }
                    if (batteryDef != null)
                        foreach (int bx in new[] { firstX + 2, (firstX + rightMostX) / 2, rightMostX - 4 })
                        {
                            IntVec3 bc = new IntVec3(bx, 0, iz - 1);
                            if (!bc.InBounds(map)) continue;
                            ClearFootprint(batteryDef, bc, Rot4.North, map);
                            Thing bat = ThingMaker.MakeThing(batteryDef);
                            bat.SetFactionDirect(Faction.OfPlayer);
                            GenSpawn.Spawn(bat, bc, map, Rot4.North, WipeMode.Vanish);
                            bat.TryGetComp<CompPowerBattery>()?.SetStoredEnergyPct(1f);
                            props.Add(bat);
                        }
                }
            }
            catch { }

            // Flag the whole bench region home so haul/clean don't drag the workers off their bills.
            try
            {
                foreach (IntVec3 c in CellRect.FromLimits(firstX - 2, bottomZ - 2, rightMostX + 2, rowOrigin.z + 2).ClipInsideMap(map))
                    map.areaManager.Home[c] = true;
            }
            catch { }

            // Bill each bench and park a fully-work-enabled worker at it. A tool counts as covered
            // once ANY of its benches gets a working bill; tools with no working bench are posed.
            var covered = new HashSet<JobToolDef>();
            foreach (KeyValuePair<Building, JobToolDef> kv in placed)
            {
                Building b = kv.Key;
                bool billed = false;
                try { billed = AddShowcaseBill(b, map, 24); } catch { }
                if (!billed) continue;   // no provisionable recipe -> this bench shows nothing

                try
                {
                    IntVec3 stand;
                    if (b.InteractionCell.Standable(map)) stand = b.InteractionCell;
                    else if (!ToolGallery.TryFindOpenCell(map, b.Position, out stand)) continue;
                    Pawn p = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                    GenSpawn.Spawn(p, stand, map);
                    EnableAllWork(p);
                    workers.Add(p);
                    covered.Add(kv.Value);
                }
                catch { }
            }

            // Any bench tool with no working station falls back to a frozen pose in the grid.
            foreach (JobToolDef tool in benchTools)
                if (!covered.Contains(tool)) failed.Add(tool);
        }

        // Max skills + every non-disabled work type at priority 3, so a freshly-spawned worker
        // immediately takes the billed bench in front of it.
        private static void EnableAllWork(Pawn p)
        {
            if (p == null) return;
            if (p.skills != null)
                foreach (SkillRecord s in p.skills.skills)
                    if (!s.TotallyDisabled) { s.Level = 14; s.passion = Passion.Major; }
            p.workSettings?.EnableAndInitialize();
            if (p.workSettings != null)
                foreach (WorkTypeDef wt in DefDatabase<WorkTypeDef>.AllDefs)
                    if (!p.WorkTypeIsDisabled(wt)) p.workSettings.SetPriority(wt, 3);
        }

        // ---- Bench billing + ingredient provisioning ------------------------------------

        private static bool AddShowcaseBill(Building bench, Map map, int provisionMult = 4)
        {
            if (!(bench is IBillGiver giver)) return false;
            IntVec3 drop = bench.InteractionCell.Standable(map) ? bench.InteractionCell : bench.Position;

            foreach (RecipeDef recipe in bench.def.AllRecipes)
            {
                if (recipe == null || !recipe.AvailableNow) continue;
                if (recipe.ingredients == null || recipe.ingredients.Count == 0) continue;
                if (!CanProvision(recipe)) continue;

                Provision(recipe, drop, map, provisionMult);
                Bill bill = recipe.MakeNewBill();
                if (bill is Bill_Production prod) prod.repeatMode = BillRepeatModeDefOf.Forever;
                bill.ingredientSearchRadius = 999f;
                giver.BillStack.AddBill(bill);
                return true;
            }
            return false;
        }

        private static bool CanProvision(RecipeDef recipe)
        {
            foreach (IngredientCount ing in recipe.ingredients)
                if (ResolveIngredient(ing) == null) return false;
            return true;
        }

        private static ThingDef ResolveIngredient(IngredientCount ing)
        {
            foreach (ThingDef d in ing.filter.AllowedThingDefs)
            {
                if (d == null) continue;
                if (d.IsCorpse) return d;               // butcher: spawn a fresh corpse later
                if (!d.EverHaulable) continue;
                if (d.category != ThingCategory.Item) continue;
                return d;
            }
            return null;
        }

        private static void Provision(RecipeDef recipe, IntVec3 near, Map map, int provisionMult = 4)
        {
            foreach (IngredientCount ing in recipe.ingredients)
            {
                ThingDef td = ResolveIngredient(ing);
                if (td == null) continue;
                int count = Mathf.Max(1, Mathf.CeilToInt(ing.GetBaseCount()));
                if (td.IsCorpse)
                {
                    SpawnAnimalCorpses(map, near, Mathf.Clamp(provisionMult / 2, 2, 12));
                    continue;
                }
                DropStacks(td, count * provisionMult, near, map);   // ×provisionMult so the forever-bill runs a long while
            }
        }

        private static void DropStacks(ThingDef td, int count, IntVec3 near, Map map)
        {
            int guard = 0;
            while (count > 0 && guard++ < 40)
            {
                int s = Mathf.Min(count, td.stackLimit);
                Thing t = ThingMaker.MakeThing(td, td.MadeFromStuff ? GenStuff.DefaultStuffFor(td) : null);
                t.stackCount = s;
                GenPlace.TryPlaceThing(t, near, map, ThingPlaceMode.Near);
                count -= s;
            }
        }

        private static void SpawnAnimalCorpses(Map map, IntVec3 near, int n)
        {
            for (int i = 0; i < n; i++)
            {
                Pawn a = PawnGenerator.GeneratePawn(PawnKindDef.Named("Muffalo"), null);
                IntVec3 c = CellFinder.RandomClosewalkCellNear(near, map, 3);
                GenSpawn.Spawn(a, c, map);
                a.Kill(null);
            }
        }

        // ---- Field-work helpers ---------------------------------------------------------

        private static void Cluster(Map map, int x0, int z0, int w, int h, Action<IntVec3> perCell)
        {
            for (int dx = 0; dx < w; dx++)
                for (int dz = 0; dz < h; dz++)
                {
                    IntVec3 c = new IntVec3(x0 + dx, 0, z0 - dz);
                    if (c.InBounds(map)) perCell(c);
                }
        }

        private static void SmoothFloorPatch(Map map, int x0, int z0, int w, int h)
        {
            TerrainDef rough = DefDatabase<TerrainDef>.AllDefs.FirstOrDefault(t => t.smoothedTerrain != null);
            if (rough == null) return;
            Cluster(map, x0, z0, w, h, c =>
            {
                foreach (Thing t in c.GetThingList(map).ToList()) if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
                map.terrainGrid.SetTerrain(c, rough);
                if (map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor) == null)
                    map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.SmoothFloor));
            });
        }

        // Remove floor (crowbar): a patch of removable built floor flagged for ripping up. The
        // crowbar also claims RemoveFoundation (bridges) now, which uses the identical pry
        // animation, so this single patch exercises both of the crowbar's new job defs.
        private static void RemoveFloorPatch(Map map, int x0, int z0, int w, int h)
        {
            TerrainDef floor = DefDatabase<TerrainDef>.GetNamedSilentFail("Concrete")
                               ?? DefDatabase<TerrainDef>.GetNamedSilentFail("PavedTile")
                               ?? DefDatabase<TerrainDef>.GetNamedSilentFail("WoodPlankFloor");
            if (floor == null) return;
            Cluster(map, x0, z0, w, h, c =>
            {
                foreach (Thing t in c.GetThingList(map).ToList())
                    if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
                map.terrainGrid.SetTerrain(c, floor);
                if (map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFloor) == null)
                    map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.RemoveFloor));
            });
        }

        private static void SpawnPlant(Map map, IntVec3 c, string plantDefName, DesignationDef des)
        {
            ThingDef plant = DefDatabase<ThingDef>.GetNamedSilentFail(plantDefName);
            if (plant == null) return;
            foreach (Thing t in c.GetThingList(map).ToList()) if (t.def.category == ThingCategory.Plant) t.Destroy();
            if (c.GetEdifice(map) != null) return;
            Plant p = (Plant)GenSpawn.Spawn(plant, c, map, WipeMode.Vanish);
            p.Growth = 1f;
            if (map.designationManager.DesignationOn(p, des) == null)
                map.designationManager.AddDesignation(new Designation(p, des));
        }

        private static void SowZone(Map map, int x0, int z0, int w, int h)
        {
            ThingDef potato = DefDatabase<ThingDef>.GetNamedSilentFail("Plant_Potato");
            TerrainDef soil = DefDatabase<TerrainDef>.GetNamedSilentFail("Soil");
            var cells = new List<IntVec3>();
            Cluster(map, x0, z0, w, h, c =>
            {
                foreach (Thing t in c.GetThingList(map).ToList()) if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
                if (soil != null) map.terrainGrid.SetTerrain(c, soil);
                if (c.GetEdifice(map) == null && map.zoneManager.ZoneAt(c) == null) cells.Add(c);
            });
            if (cells.Count == 0) return;
            Zone_Growing zone = new Zone_Growing(map.zoneManager);
            map.zoneManager.RegisterZone(zone);
            foreach (IntVec3 c in cells) zone.AddCell(c);
            if (potato != null) zone.SetPlantDefToGrow(potato);
        }

        private static void SpawnWall(Map map, IntVec3 c, DesignationDef des, bool damage)
        {
            ThingDef wallDef = DefDatabase<ThingDef>.GetNamedSilentFail("Wall");
            if (wallDef == null) return;
            ClearFootprint(wallDef, c, Rot4.North, map);
            Thing wall = ThingMaker.MakeThing(wallDef, ThingDefOf.Steel);
            wall.SetFactionDirect(Faction.OfPlayer);
            GenSpawn.Spawn(wall, c, map, Rot4.North, WipeMode.Vanish);
            if (damage) wall.HitPoints = Mathf.Max(1, wall.MaxHitPoints / 4);
            if (des != null) map.designationManager.AddDesignation(new Designation(wall, des));
        }

        // Build frames in four materials so every FinishFrame tool fires: wood (hammer),
        // stone blocks (build chisel), steel (welder), and a grave (grave shovel).
        private static void BuildFrameVariety(Map map, int x0, int z0)
        {
            ThingDef wallDef = DefDatabase<ThingDef>.GetNamedSilentFail("Wall");
            ThingDef graveDef = DefDatabase<ThingDef>.GetNamedSilentFail("Grave");
            ThingDef granite = DefDatabase<ThingDef>.GetNamedSilentFail("BlocksGranite");
            ThingDef wood = ThingDefOf.WoodLog;
            ThingDef steel = ThingDefOf.Steel;
            int row = 0;
            if (wallDef != null && wood != null) PlaceWallBP(map, x0, z0 - row++ * 2, wallDef, wood);
            if (wallDef != null && granite != null) PlaceWallBP(map, x0, z0 - row++ * 2, wallDef, granite);
            if (wallDef != null) PlaceWallBP(map, x0, z0 - row++ * 2, wallDef, steel);
            if (graveDef != null)
            {
                IntVec3 gc = new IntVec3(x0, 0, z0 - row++ * 2);
                if (gc.InBounds(map))
                {
                    foreach (Thing t in gc.GetThingList(map).ToList()) if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
                    GenConstruct.PlaceBlueprintForBuild(graveDef, gc, map, Rot4.North, Faction.OfPlayer, null);
                }
            }
            if (wood != null) DropStacks(wood, 150, new IntVec3(x0 - 2, 0, z0), map);
            if (granite != null) DropStacks(granite, 150, new IntVec3(x0 - 2, 0, z0 - 2), map);
            DropStacks(steel, 200, new IntVec3(x0 - 2, 0, z0 - 4), map);
        }

        private static void PlaceWallBP(Map map, int x0, int z0, ThingDef wallDef, ThingDef stuff)
        {
            IntVec3 c = new IntVec3(x0, 0, z0);
            if (!c.InBounds(map)) return;
            foreach (Thing t in c.GetThingList(map).ToList()) if (t.def.destroyable && t.def.category != ThingCategory.Pawn) t.Destroy();
            ThingDef useStuff = (wallDef.MadeFromStuff && stuff != null && stuff.IsStuff)
                ? stuff
                : (wallDef.MadeFromStuff ? GenStuff.DefaultStuffFor(wallDef) : null);
            GenConstruct.PlaceBlueprintForBuild(wallDef, c, map, Rot4.North, Faction.OfPlayer, useStuff);
        }

        // A mining row laid out as one column per stone type, so the material-aware pickaxe
        // chips can be compared side by side (granite/marble/sandstone/limestone/slate).
        private static void MineVariety(Map map, int x0, int z0, int rows)
        {
            for (int i = 0; i < Rocks.Length; i++)
            {
                ThingDef rock = DefDatabase<ThingDef>.GetNamedSilentFail(Rocks[i]);
                if (rock == null) continue;
                for (int dz = 0; dz < rows; dz++)
                {
                    IntVec3 c = new IntVec3(x0 + i, 0, z0 - dz);
                    if (!c.InBounds(map)) continue;
                    foreach (Thing t in c.GetThingList(map).ToList()) if (t.def.destroyable) t.Destroy();
                    GenSpawn.Spawn(rock, c, map, WipeMode.Vanish);
                    if (map.designationManager.DesignationAt(c, DesignationDefOf.Mine) == null)
                        map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.Mine));
                }
            }
        }

        // Smoothing row, one column per stone type — the chisel's material-tinted chips differ.
        private static void SmoothVariety(Map map, int x0, int z0, int rows)
        {
            int cols = Mathf.Min(4, Rocks.Length);
            for (int i = 0; i < cols; i++)
            {
                ThingDef rock = DefDatabase<ThingDef>.GetNamedSilentFail(Rocks[i]);
                if (rock == null) continue;
                for (int dz = 0; dz < rows; dz++)
                {
                    IntVec3 c = new IntVec3(x0 + i, 0, z0 - dz);
                    if (!c.InBounds(map)) continue;
                    foreach (Thing t in c.GetThingList(map).ToList()) if (t.def.destroyable) t.Destroy();
                    GenSpawn.Spawn(rock, c, map, WipeMode.Vanish);
                    if (map.designationManager.DesignationAt(c, DesignationDefOf.SmoothWall) == null)
                        map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.SmoothWall));
                }
            }
        }

        // Deconstruct row, one wall per stuff — the crowbar's material-tinted chips differ.
        private static void DeconstructVariety(Map map, int x0, int z0)
        {
            ThingDef wallDef = DefDatabase<ThingDef>.GetNamedSilentFail("Wall");
            if (wallDef == null) return;
            for (int i = 0; i < WallStuffs.Length; i++)
            {
                ThingDef stuff = DefDatabase<ThingDef>.GetNamedSilentFail(WallStuffs[i]);
                if (stuff == null || !stuff.IsStuff) stuff = ThingDefOf.Steel;
                IntVec3 c = new IntVec3(x0, 0, z0 - i);
                if (!c.InBounds(map)) continue;
                ClearFootprint(wallDef, c, Rot4.North, map);
                Thing wall = ThingMaker.MakeThing(wallDef, stuff);
                wall.SetFactionDirect(Faction.OfPlayer);
                GenSpawn.Spawn(wall, c, map, Rot4.North, WipeMode.Vanish);
                map.designationManager.AddDesignation(new Designation(wall, DesignationDefOf.Deconstruct));
            }
        }

        // ---- Misc -----------------------------------------------------------------------

        private static void ClearFootprint(ThingDef def, IntVec3 center, Rot4 rot, Map map)
        {
            foreach (IntVec3 c in GenAdj.OccupiedRect(center, rot, def.size))
            {
                if (!c.InBounds(map)) continue;
                foreach (Thing t in c.GetThingList(map).ToList())
                    if (t.def.category != ThingCategory.Pawn && t.def.destroyable) t.Destroy();
            }
        }

        private static void Try(List<string> report, string label, Action a)
        {
            try { a(); }
            catch (Exception e) { report.Add(label); Log.Warning($"[Show Me Your Tools] showcase '{label}' failed: {e.Message}"); }
        }
    }
}
