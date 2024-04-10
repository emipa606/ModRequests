using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterInfestations
{
    public class IncidentWorker_Infestation : IncidentWorker
    {
        public const float HivePoints = 220f;
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (BetterInfestationsMod.settings != null)
            {
                Map map = (Map)parms.target;
                if (base.CanFireNowSub(parms) && HiveUtility.TotalSpawnedHivesCount(map) < BetterInfestationsMod.settings.maxHivesPerMap)
                {
                    return Patches.Patch_InfestationCellFinder_TryFindCell.TryFindCell(out _, map);
                }
            }
            return false;
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (BetterInfestationsMod.settings != null)
            {
                Map map = (Map)parms.target;
                if (BetterInfestationsMod.settings.infestationsAllowed && HiveUtility.TotalSpawnedHivesCount(map) < BetterInfestationsMod.settings.maxHivesPerMap)
                {
                    Thing t = InfestationUtility.SpawnTunnels(Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1), map, true);
                    if (t != null)
                    {
                        if (BetterInfestationsMod.settings.notificationInf)
                        {
                            SendStandardLetter(parms, t);
                            Find.TickManager.slower.SignalForceNormalSpeedShort();
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class IncidentWorker_SwarmingHorde : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (BetterInfestationsMod.settings != null)
            {
                Map map = (Map)parms.target;
                if (base.CanFireNowSub(parms) && HiveUtility.SwarmingHordeCheck(map))
                {
                    return true;
                }
            }
            return false;
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (BetterInfestationsMod.settings != null)
            {
                Map map = (Map)parms.target;
                if (BetterInfestationsMod.settings.swarmingHordeEventAllowed && HiveUtility.SwarmingHordeCheck(map))
                {
                    Thing t = HiveUtility.ExecuteSwarmingHorde(map);
                    if (t != null)
                    {
                        SendStandardLetter(parms, t);
                        Find.TickManager.slower.SignalForceNormalSpeedShort();
                    }
                    return true;
                }
            }
            return false;
        }
    }

    public class IncidentWorker_WorldInfestation : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            return Patches.Patch_FactionGenerator_GenerateFactionsIntoWorld.RandomInfestationTile(out _);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int index;
            if (Patches.Patch_FactionGenerator_GenerateFactionsIntoWorld.RandomInfestationTile(out index))
            {
                Site site = (Site)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("BI_InfestationWorldObject"));
                site.Tile = index;
                site.parts.Add(new SitePart(site, DefDatabase<SitePartDef>.GetNamed("BI_Infestation_SitePart"), DefDatabase<SitePartDef>.GetNamed("BI_Infestation_SitePart").Worker.GenerateDefaultParams(0f, site.Tile, Faction.OfInsects)));
                site.SetFaction(Faction.OfInsects);
                Find.WorldObjects.Add(site);
                Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, def.letterDef, site);
                return true;
            }
            return false;
        }
    }

    public class IncidentWorker_InsectRaid : IncidentWorker
    {
        public static float accumPoints = 0f;
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            return RCellFinder.TryFindRandomPawnEntryCell(out _, (Map)parms.target, CellFinder.EdgeRoadChance_Animal);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (BetterInfestationsMod.settings != null)
            {
                if (BetterInfestationsMod.settings.raidsAllowed)
                {
                    Map map = (Map)parms.target;
                    IntVec3 result = parms.spawnCenter;
                    if (!result.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Animal))
                    {
                        return false;
                    }
                    List<Pawn> list = GenerateInsects(map.Tile, parms.points);
                    Rot4 rot = Rot4.FromAngleFlat((map.Center - result).AngleFlat);
                    Lord lord = LordMaker.MakeNewLord(Faction.OfInsects, Activator.CreateInstance(typeof(LordJob_AssaultColony), null) as LordJob, map);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Pawn pawn = list[i];
                        IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 10);
                        QuestUtility.AddQuestTag(GenSpawn.Spawn(pawn, loc, map, rot), parms.questTag);
                        lord.AddPawn(pawn);
                    }
                    SendStandardLetter("BI_LetterLabelInsectRaid".Translate(), "BI_LetterTextInsectRaid".Translate(Faction.OfInsects.Name.ApplyTag(Faction.OfInsects)), LetterDefOf.ThreatBig, parms, list[0]);
                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                    LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
                    LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);
                    return true;
                }
            }
            return false;
        }
        public static List<Pawn> GenerateInsects(int tile, float points)
        {
            List<Pawn> list = new List<Pawn>();
            for (int i = 0; i < 500; i++)
            {
                if (accumPoints >= points) break;

                PawnKindDef pawnKindDef = RandomPawnKindDef();
                Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, Faction.OfInsects, PawnGenerationContext.NonPlayer, tile));
                list.Add(item);
                accumPoints += pawnKindDef.combatPower;
            }
            return list;
        }
        public static PawnKindDef RandomPawnKindDef()
        {
            IEnumerable<PawnKindDef> source = new List<PawnKindDef> { RimWorld.PawnKindDefOf.Megascarab, RimWorld.PawnKindDefOf.Spelopede, RimWorld.PawnKindDefOf.Megaspider };
            if (source.TryRandomElement(out PawnKindDef result))
            {
                return result;
            }
            return null;
        }
    }
}