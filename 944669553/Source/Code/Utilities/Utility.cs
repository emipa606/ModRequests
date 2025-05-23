﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HPLovecraft;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cthulhu
{
    public static class Utility
    {
        public enum SanLossSev
        {
            None = 0,
            Hidden,
            Initial,
            Minor,
            Major,
            Extreme
        }

        public const string SanityLossDef = "ROM_SanityLoss";
        public const string AltSanityLossDef = "ROM_SanityLoss";

        public static bool modCheck;
        public static bool loadedCosmicHorrors;
        public static bool loadedIndustrialAge;
        public static bool loadedCults;
        public static bool loadedFactions;

        public static string Prefix => ModProps.main + " :: " + ModProps.mod + " " + ModProps.version + " :: ";


        public static bool IsMorning(Map map)
        {
            return GenLocalDate.HourInteger(map) > 6 && GenLocalDate.HourInteger(map) < 10;
        }

        public static bool IsEvening(Map map)
        {
            return GenLocalDate.HourInteger(map) > 18 && GenLocalDate.HourInteger(map) < 22;
        }

        public static bool IsNight(Map map)
        {
            return GenLocalDate.HourInteger(map) > 22;
        }

        public static T GetMod<T>(string s) where T : Mod
        {
            //Call of Cthulhu - Cosmic Horrors
            T result = default;
            foreach (var ResolvedMod in LoadedModManager.ModHandles)
            {
                if (ResolvedMod.Content.Name == s)
                {
                    result = ResolvedMod as T;
                }
            }

            return result;
        }

        public static List<IncidentDef> CosmicHorrorIncidents()
        {
            return new List<IncidentDef>
            {
                IncidentDef.Named("ROM_RaidCosmicHorrors"),
                IncidentDef.Named("ROM_StarVampireAttack"),
                IncidentDef.Named("ROM_ChthonianPit")
            };
        }

        public static bool IsCosmicHorror(Pawn thing)
        {
            if (!IsCosmicHorrorsLoaded())
            {
                return false;
            }

            var type = Type.GetType("CosmicHorror.CosmicHorrorPawn");
            if (type == null)
            {
                return false;
            }

            if (thing.GetType() == type)
            {
                return true;
            }

            return false;
        }


        //public static float GetSanityLossRate(PawnKindDef kindDef)
        //{
        //    float sanityLossRate = 0f;
        //    if (kindDef.ToString() == "ROM_StarVampire")
        //        sanityLossRate = 0.04f;
        //    if (kindDef.ToString() == "StarSpawnOfCthulhu")
        //        sanityLossRate = 0.02f;
        //    if (kindDef.ToString() == "DarkYoung")
        //        sanityLossRate = 0.004f;
        //    if (kindDef.ToString() == "DeepOne")
        //        sanityLossRate = 0.008f;
        //    if (kindDef.ToString() == "DeepOneGreat")
        //        sanityLossRate = 0.012f;
        //    if (kindDef.ToString() == "MiGo")
        //        sanityLossRate = 0.008f;
        //    if (kindDef.ToString() == "Shoggoth")
        //        sanityLossRate = 0.012f;
        //    return sanityLossRate;
        //}


        public static bool IsActorAvailable(Pawn preacher, bool downedAllowed = false)
        {
            var s = new StringBuilder();
            s.Append("ActorAvailble Checks Initiated");
            s.AppendLine();
            if (preacher == null)
            {
                return ResultFalseWithReport(s);
            }

            s.Append("ActorAvailble: Passed null Check");
            s.AppendLine();
            //if (!preacher.Spawned)
            //    return ResultFalseWithReport(s);
            //s.Append("ActorAvailble: Passed not-spawned check");
            //s.AppendLine();
            if (preacher.Dead)
            {
                return ResultFalseWithReport(s);
            }

            s.Append("ActorAvailble: Passed not-dead");
            s.AppendLine();
            if (preacher.Downed && !downedAllowed)
            {
                return ResultFalseWithReport(s);
            }

            s.Append("ActorAvailble: Passed downed check & downedAllowed = " + downedAllowed);
            s.AppendLine();
            if (preacher.Drafted)
            {
                return ResultFalseWithReport(s);
            }

            s.Append("ActorAvailble: Passed drafted check");
            s.AppendLine();
            if (preacher.InAggroMentalState)
            {
                return ResultFalseWithReport(s);
            }

            s.Append("ActorAvailble: Passed drafted check");
            s.AppendLine();
            if (preacher.InMentalState)
            {
                return ResultFalseWithReport(s);
            }

            s.Append("ActorAvailble: Passed InMentalState check");
            s.AppendLine();
            s.Append("ActorAvailble Checks Passed");
            Settings.DebugString(s.ToString());
            return true;
        }

        public static bool ResultFalseWithReport(StringBuilder s)
        {
            s.Append("ActorAvailble: Result = Unavailable");
            Settings.DebugString(s.ToString());
            return false;
        }

        public static Pawn GenerateNewPawnFromSource(ThingDef newDef, Pawn sourcePawn)
        {
            var pawn = (Pawn) ThingMaker.MakeThing(newDef);
            //Settings.DebugString("Declare a new thing");
            pawn.Name = sourcePawn.Name;
            //Settings.DebugString("The name!");
            pawn.SetFactionDirect(Faction.OfPlayer);
            pawn.kindDef = sourcePawn.kindDef;
            //Settings.DebugString("The def!");
            pawn.pather = new Pawn_PathFollower(pawn);
            //Settings.DebugString("The pather!");
            pawn.ageTracker = new Pawn_AgeTracker(pawn);
            pawn.health = new Pawn_HealthTracker(pawn);
            pawn.jobs = new Pawn_JobTracker(pawn);
            pawn.mindState = new Pawn_MindState(pawn);
            pawn.filth = new Pawn_FilthTracker(pawn);
            pawn.needs = new Pawn_NeedsTracker(pawn);
            pawn.stances = new Pawn_StanceTracker(pawn);
            pawn.natives = new Pawn_NativeVerbs(pawn);
            pawn.relations = sourcePawn.relations;
            PawnComponentsUtility.CreateInitialComponents(pawn);

            if (pawn.RaceProps.ToolUser)
            {
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
                pawn.carryTracker = new Pawn_CarryTracker(pawn);
                pawn.apparel = new Pawn_ApparelTracker(pawn);
                pawn.inventory = new Pawn_InventoryTracker(pawn);
            }

            if (pawn.RaceProps.intelligence <= Intelligence.ToolUser)
            {
                pawn.caller = new Pawn_CallTracker(pawn);
            }

            pawn.gender = sourcePawn.gender;
            pawn.needs.SetInitialLevels();
            GenerateRandomAge(pawn, sourcePawn.Map);
            CopyPawnRecords(sourcePawn, pawn);
            //Settings.DebugString("We got so far.");
            return pawn;
        }

        public static void CopyPawnRecords(Pawn pawn, Pawn newPawn)
        {
            //Who has a relationship with this pet?
            Pawn pawnMaster = null;
            var map = pawn.Map;
            foreach (var current in map.mapPawns.AllPawns)
            {
                if (current.relations.DirectRelationExists(PawnRelationDefOf.Bond, pawn))
                {
                    pawnMaster = current;
                }
            }

            //Fix the relations
            if (pawnMaster != null)
            {
                pawnMaster.relations.TryRemoveDirectRelation(PawnRelationDefOf.Bond, pawn);
                pawnMaster.relations.AddDirectRelation(PawnRelationDefOf.Bond, newPawn);
                //Train that stuff!

                var oldMap = (DefMap<TrainableDef, int>) typeof(Pawn_TrainingTracker)
                    .GetField("steps", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(pawn.training);
                var newMap = (DefMap<TrainableDef, int>) typeof(Pawn_TrainingTracker)
                    .GetField("steps", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(newPawn.training);

                foreach (var def in DefDatabase<TrainableDef>.AllDefs)
                {
                    if (newMap == null)
                    {
                        continue;
                    }

                    if (oldMap != null)
                    {
                        newMap[def] = oldMap[def];
                    }
                }
            }


            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                newPawn.health.AddHediff(hediff);
            }
        }

        public static void GenerateRandomAge(Pawn pawn, Map map)
        {
            var num = 0;
            int num2;
            do
            {
                if (pawn.RaceProps.ageGenerationCurve != null)
                {
                    num2 = Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve));
                }
                else if (pawn.RaceProps.IsMechanoid)
                {
                    num2 = Rand.Range(0, 2500);
                }
                else
                {
                    if (!pawn.RaceProps.Animal)
                    {
                        goto IL_84;
                    }

                    num2 = Rand.Range(1, 10);
                }

                num++;
                if (num > 100)
                {
                    goto IL_95;
                }
            } while (num2 > pawn.kindDef.maxGenerationAge || num2 < pawn.kindDef.minGenerationAge);

            goto IL_A5;
            IL_84:
            Log.Warning("Didn't get age for " + pawn);
            return;
            IL_95:
            Log.Error("Tried 100 times to generate age for " + pawn);
            IL_A5:
            pawn.ageTracker.AgeBiologicalTicks = (long) (num2 * 3600000f) + Rand.Range(0, 3600000);
            int num3;
            if (Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
            {
                var value = Rand.Value;
                if (value < 0.7f)
                {
                    num3 = Rand.Range(0, 100);
                }
                else if (value < 0.95f)
                {
                    num3 = Rand.Range(100, 1000);
                }
                else
                {
                    var num4 = GenLocalDate.Year(map) - 2026 - pawn.ageTracker.AgeBiologicalYears;
                    num3 = Rand.Range(1000, num4);
                }
            }
            else
            {
                num3 = 0;
            }

            var num5 = GenTicks.TicksAbs - pawn.ageTracker.AgeBiologicalTicks;
            num5 -= num3 * 3600000L;
            pawn.ageTracker.BirthAbsTicks = num5;
            if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
            {
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
            }
        }


        /// <summary>
        ///     A very complicated method for finding a proper place for objects to spawn in Cthulhu Utility.
        /// </summary>
        /// <param name="def"></param>
        /// <param name="nearLoc"></param>
        /// <param name="map"></param>
        /// <param name="maxDist"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static bool TryFindSpawnCell(ThingDef def, IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
        {
            return CellFinder.TryFindRandomCellNear(nearLoc, map, maxDist, delegate(IntVec3 x)
            {
                // Check if the entire area is safe based on the size of the object definition.
                foreach (var current in GenAdj.OccupiedRect(x, Rot4.North, new IntVec2(def.size.x + 2, def.size.z + 2)))
                {
                    if (!current.InBounds(map) || current.Fogged(map) || !current.Standable(map) ||
                        current.Roofed(map) && current.GetRoof(map).isThickRoof)
                    {
                        return false;
                    }

                    if (!current.SupportsStructureType(map, def.terrainAffordanceNeeded))
                    {
                        return false;
                    }

                    //
                    //  If it has an interaction cell, check to see if it can be reached by colonists.
                    //
                    var intCanBeReached = true;
                    if (def.interactionCellOffset != IntVec3.Zero)
                    {
                        foreach (var colonist in map.mapPawns.FreeColonistsSpawned)
                        {
                            if (!colonist.CanReach(current + def.interactionCellOffset, PathEndMode.ClosestTouch,
                                Danger.Deadly))
                            {
                                intCanBeReached = false;
                            }
                        }
                    }

                    if (!intCanBeReached)
                    {
                        return false;
                    }
                    //

                    //Don't wipe existing objets...
                    var thingList = current.GetThingList(map);
                    foreach (var thing in thingList)
                    {
                        if (thing.def.category != ThingCategory.Plant && GenSpawn.SpawningWipes(def, thing.def))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }, out pos);
        }

        public static BodyPartRecord GetMouth(HediffSet set)
        {
            foreach (var current in set.GetNotMissingParts())
            {
                for (var i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.defName == "TalkingSource")
                    {
                        return current;
                    }
                }
            }

            return null;
        }

        public static BodyPartRecord GetEar(HediffSet set)
        {
            foreach (var current in set.GetNotMissingParts())
            {
                for (var i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.defName == "HearingSource")
                    {
                        return current;
                    }
                }
            }

            return null;
        }

        public static BodyPartRecord GetNose(HediffSet set)
        {
            foreach (var current in set.GetNotMissingParts())
            {
                for (var i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.defName == "Nose")
                    {
                        return current;
                    }
                }
            }

            return null;
        }

        public static BodyPartRecord GetEye(HediffSet set)
        {
            foreach (var current in set.GetNotMissingParts())
            {
                foreach (var bodyPartTagDef in current.def.tags)
                {
                    if (bodyPartTagDef.defName == "SightSource")
                    {
                        return current;
                    }
                }
            }

            return null;
        }

        public static BodyPartRecord GetHeart(HediffSet set)
        {
            foreach (var current in set.GetNotMissingParts())
            {
                foreach (var bodyPartTagDef in current.def.tags)
                {
                    if (bodyPartTagDef.defName == "BloodPumpingSource")
                    {
                        return current;
                    }
                }
            }

            return null;
        }


        public static void SpawnThingDefOfCountAt(ThingDef of, int count, TargetInfo target)
        {
            while (count > 0)
            {
                var thing = ThingMaker.MakeThing(of);

                thing.stackCount = Math.Min(count, of.stackLimit);
                GenPlace.TryPlaceThing(thing, target.Cell, target.Map, ThingPlaceMode.Near);
                count -= thing.stackCount;
            }
        }

        public static void SpawnPawnsOfCountAt(PawnKindDef kindDef, IntVec3 at, Map map, int count, out Pawn returnable,
            Faction fac = null, bool berserk = false, bool target = false)
        {
            Pawn result = null;
            for (var i = 1; i <= count; i++)
            {
                var at1 = at;
                if (!(from cell in GenAdj.CellsAdjacent8Way(new TargetInfo(at, map))
                    where at1.Walkable(map)
                    select cell).TryRandomElement(out at))
                {
                    continue;
                }

                var pawn = PawnGenerator.GeneratePawn(kindDef, fac);
                if (result == null)
                {
                    result = pawn;
                }

                if (GenPlace.TryPlaceThing(pawn, at, map, ThingPlaceMode.Near))
                {
                    //if (target) Map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = at;
                    //continue;
                }

                //Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                if (berserk)
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
                }
            }

            returnable = result;
        }

        public static void SpawnPawnsOfCountAt(PawnKindDef kindDef, IntVec3 at, Map map, int count, Faction fac = null,
            bool berserk = false, bool target = false)
        {
            for (var i = 1; i <= count; i++)
            {
                var at1 = at;
                if (!(from cell in GenAdj.CellsAdjacent8Way(new TargetInfo(at, map))
                    where at1.Walkable(map)
                    select cell).TryRandomElement(out at))
                {
                    continue;
                }

                var pawn = PawnGenerator.GeneratePawn(kindDef, fac);
                if (GenPlace.TryPlaceThing(pawn, at, map, ThingPlaceMode.Near))
                {
                    //if (target) Map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = at;
                    //continue;
                }

                //Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                if (berserk)
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
                }
            }
        }


        public static void ChangeResearchProgress(ResearchProjectDef projectDef, float progressValue,
            bool deselectCurrentResearch = false)
        {
            var researchProgressInfo =
                typeof(ResearchManager).GetField("progress", BindingFlags.Instance | BindingFlags.NonPublic);
            var researchProgress = researchProgressInfo?.GetValue(Find.ResearchManager);
            var itemPropertyInfo = researchProgress?.GetType().GetProperty("Item");
            itemPropertyInfo?.SetValue(researchProgress, progressValue, new object[] {projectDef});
            if (deselectCurrentResearch)
            {
                Find.ResearchManager.currentProj = null;
            }

            Find.ResearchManager.ReapplyAllMods();
        }

        public static float CurrentSanityLoss(Pawn pawn)
        {
            var sanityLossDef = AltSanityLossDef;
            if (IsCosmicHorrorsLoaded())
            {
                sanityLossDef = SanityLossDef;
            }

            var pawnSanityHediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(sanityLossDef));
            if (pawnSanityHediff != null)
            {
                return pawnSanityHediff.Severity;
            }

            return 0f;
        }


        public static void ApplyTaleDef(string defName, Map map)
        {
            var randomPawn = map.mapPawns.FreeColonists.RandomElement();
            var taleToAdd = TaleDef.Named(defName);
            TaleRecorder.RecordTale(taleToAdd, randomPawn);
        }

        public static void ApplyTaleDef(string defName, Pawn pawn)
        {
            var taleToAdd = TaleDef.Named(defName);
            if ((pawn.IsColonist || pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
            {
                TaleRecorder.RecordTale(taleToAdd, pawn);
            }
        }


        public static bool HasSanityLoss(Pawn pawn)
        {
            var sanityLossDef = !IsCosmicHorrorsLoaded() ? AltSanityLossDef : SanityLossDef;
            var pawnSanityHediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(sanityLossDef));

            return pawnSanityHediff != null;
        }

        /// <summary>
        ///     This method handles the application of Sanity Loss in multiple mods.
        ///     It returns true and false depending on if it applies successfully.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="sanityLoss"></param>
        /// <param name="sanityLossMax"></param>
        public static void ApplySanityLoss(Pawn pawn, float sanityLoss = 0.3f, float sanityLossMax = 1.0f)
        {
            //Log.Message("1");
            if (pawn == null)
            {
                return;
            }
            //Log.Message("2");

            var sanityLossDef = !IsCosmicHorrorsLoaded() ? AltSanityLossDef : SanityLossDef;
            //Log.Message(sanityLossDef);

            var pawnSanityHediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail(sanityLossDef));
            if (pawnSanityHediff != null)
            {
                //Log.Message("3a");

                if (pawnSanityHediff.Severity > sanityLossMax)
                {
                    sanityLossMax = pawnSanityHediff.Severity;
                }

                var result = pawnSanityHediff.Severity;
                result += sanityLoss;
                result = Mathf.Clamp(result, 0.0f, sanityLossMax);
                pawnSanityHediff.Severity = result;
                pawn.health.Notify_HediffChanged(pawnSanityHediff);
                Settings.DebugString("Applied Sanity loss to: " + pawn.LabelShort);
            }
            else if (sanityLoss > 0)
            {
                //Log.Message("3b");

                //HediffGiverUtility.TryApply(pawn, HediffDef.Named(sanityLossDef), new List<BodyPartDef>() { BodyPartDefOf.Brain });
                //ApplySanityLoss(pawn, sanityLoss);

                var sanityLossHediff =
                    HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamedSilentFail(sanityLossDef), pawn);
                if (sanityLossHediff == null)
                {
                    return;
                }

                sanityLossHediff.Severity = sanityLoss;
                pawn.health.AddHediff(sanityLossHediff);
                pawn.health.Notify_HediffChanged(null);
                Settings.DebugString("Made and applied Sanity loss to: " + pawn.LabelShort);

                Messages.Message(new Message("HPLovecraft_SanityLossBegun".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.NegativeEvent, pawn));
                //Log.Message("4");
            }
        }


        public static int GetSocialSkill(Pawn p)
        {
            return p.skills.GetSkill(SkillDefOf.Social).Level;
        }

        public static int GetResearchSkill(Pawn p)
        {
            return p.skills.GetSkill(SkillDefOf.Intellectual).Level;
        }

        public static bool IsCosmicHorrorsLoaded()
        {
            if (!modCheck)
            {
                ModCheck();
            }

            return loadedCosmicHorrors;
        }


        public static bool IsIndustrialAgeLoaded()
        {
            if (!modCheck)
            {
                ModCheck();
            }

            return loadedIndustrialAge;
        }


        public static bool IsCultsLoaded()
        {
            if (!modCheck)
            {
                ModCheck();
            }

            return loadedCults;
        }

        public static bool IsRandomWalkable8WayAdjacentOf(IntVec3 cell, Map map, out IntVec3 resultCell)
        {
            if (cell != IntVec3.Invalid)
            {
                _ = cell.RandomAdjacentCell8Way();
                if (map != null)
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var temp = cell.RandomAdjacentCell8Way();
                        if (!temp.Walkable(map))
                        {
                            continue;
                        }

                        resultCell = temp;
                        return true;
                    }
                }
            }

            resultCell = IntVec3.Invalid;
            return false;
        }

        public static void TemporaryGoodwill(Faction faction, bool reset = false)
        {
            var playerFaction = Faction.OfPlayer;
            if (!reset)
            {
                if (faction.GoodwillWith(playerFaction) == 0f)
                {
                    faction.RelationWith(playerFaction).baseGoodwill = faction.PlayerGoodwill;
                }

                faction.RelationWith(playerFaction).baseGoodwill = 100;
                faction.SetRelation(new FactionRelation {kind = FactionRelationKind.Hostile, other = playerFaction});
            }
            else
            {
                faction.RelationWith(playerFaction).baseGoodwill = 0;
                faction.SetRelation(new FactionRelation {kind = FactionRelationKind.Neutral, other = playerFaction});
            }
        }


        public static void ModCheck()
        {
            loadedCosmicHorrors = false;
            loadedIndustrialAge = false;
            foreach (var ResolvedMod in LoadedModManager.RunningMods)
            {
                if (loadedCosmicHorrors && loadedIndustrialAge && loadedCults)
                {
                    break; //Save some loading
                }

                if (ResolvedMod.Name.Contains("Call of Cthulhu - Cosmic Horrors"))
                {
                    Settings.DebugString("Loaded - Call of Cthulhu - Cosmic Horrors");
                    loadedCosmicHorrors = true;
                }

                if (ResolvedMod.Name.Contains("Call of Cthulhu - Industrial Age"))
                {
                    Settings.DebugString("Loaded - Call of Cthulhu - Industrial Age");
                    loadedIndustrialAge = true;
                }

                if (ResolvedMod.Name.Contains("Call of Cthulhu - Cults"))
                {
                    Settings.DebugString("Loaded - Call of Cthulhu - Cults");
                    loadedCults = true;
                }

                if (!ResolvedMod.Name.Contains("Call of Cthulhu - Factions"))
                {
                    continue;
                }

                Settings.DebugString("Loaded - Call of Cthulhu - Factions");
                loadedFactions = true;
            }

            modCheck = true;
        }
    }
}