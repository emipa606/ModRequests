// Nightvision NightVision SolarRaid_IncidentWorker.cs
// 
// 17 10 2018
// 
// 18 10 2018

using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace NightVision
{
    [UsedImplicitly]
    public class SolarRaid_IncidentWorker : IncidentWorker_RaidEnemy
    {
        public static readonly PawnsArrivalModeDef ForcedArriveMode = PawnsArrivalModeDefOf.CenterDrop;

        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            return !f.IsPlayer
                   && !f.defeated
                   && (desperate
                       || f.def.allowedArrivalTemperatureRange.Includes(f: map.mapTemperature.OutdoorTemp)
                       && f.def.allowedArrivalTemperatureRange.Includes(f: map.mapTemperature.SeasonalTemp))
                   && f.HostileTo(other: Faction.OfPlayer)
                   && (desperate || GenDate.DaysPassed >= f.def.earliestRaidDays)
                   && f.def.techLevel >= ForcedArriveMode.minTechLevel;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var map = (Map)parms.target;

            return Find.World.GameConditionManager.ConditionIsActive(def: Defs_Rimworld.SolarFlare)
                   && (parms.faction != null || CandidateFactions(map: map, desperate: false).Any());
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            parms.raidArrivalMode = ForcedArriveMode;

            ResolveRaidPoints(parms: parms);

            if (!TryResolveRaidFaction(parms: parms))
            {
                Log.Message(text: "Failed solar raid: no faction found.");

                return false;
            }

            PawnGroupKindDef combat = Defs_Rimworld.CombatGroup;
            ResolveRaidStrategy(parms: parms, groupKind: combat);

            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms: parms))
            {
                return false;
            }

            parms.points = AdjustedRaidPoints(
                points: parms.points,
                raidArrivalMode: parms.raidArrivalMode,
                raidStrategy: parms.raidStrategy,
                faction: parms.faction,
                groupKind: combat
            );

            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(
                groupKind: combat,
                parms: parms,
                ensureCanGenerateAtLeastOnePawn: false
            );

            SolarRaidGroupMaker.TryGetRandomPawnGroupMaker(parms: defaultPawnGroupMakerParms, pawnGroupMaker: out PawnGroupMaker pawnGroupMaker);

            List<Pawn> list = SolarRaid_PawnGenerator.GeneratePawns(parms: defaultPawnGroupMakerParms, groupMaker: pawnGroupMaker).ToList();

            if (list.Count == 0)
            {
                return false;
            }

            parms.raidArrivalMode.Worker.Arrive(pawns: list, parms: parms);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(value: "Points = " + parms.points.ToString(format: "F0"));

            foreach (Pawn pawn in list)
            {
                string str = pawn.equipment?.Primary == null ? "unarmed" : pawn.equipment.Primary.LabelCap;
                stringBuilder.AppendLine(value: pawn.KindLabel + " - " + str);
            }

#if RW10
            string letterLabel = GetLetterLabel(parms: parms);
            string letterText = GetLetterText(parms: parms, pawns: list);
#else
TaggedString letterLabel = GetLetterLabel(parms: parms);
            TaggedString letterText = GetLetterText(parms: parms, pawns: list);
#endif

            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(
                seenPawns: list,
                letterLabel: ref letterLabel,
                letterText: ref letterText,
                relationsInfoHeader: GetRelatedPawnsInfoLetterText(parms: parms),
                informEvenIfSeenBefore: true,
                writeSeenPawnsNames: true
            );

            var list2 = new List<TargetInfo>();

            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(pawns: list, groups: parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy(selector: x => x.Count);

                if (list4.Any())
                {
                    list2.Add(item: list4[index: 0]);
                }

                for (var i = 0; i < list3.Count; i++)
                {
                    if (list3[index: i] != list4)
                    {
                        if (list3[index: i].Any())
                        {
                            list2.Add(item: list3[index: i][index: 0]);
                        }
                    }
                }
            }
            else if (list.Any())
            {
                list2.Add(item: list[index: 0]);
            }

            Find.LetterStack.ReceiveLetter(
                label: letterLabel,
                text: letterText,
                textLetterDef: GetLetterDef(),
                lookTargets: list2,
                relatedFaction: parms.faction,
                debugInfo: stringBuilder.ToString()
            );

            parms.raidStrategy.Worker.MakeLords(parms: parms, pawns: list);
            LessonAutoActivator.TeachOpportunity(conc: ConceptDefOf.EquippingWeapons, opp: OpportunityType.Critical);

            if (!PlayerKnowledgeDatabase.IsComplete(conc: ConceptDefOf.ShieldBelts))
            {
                for (var j = 0; j < list.Count; j++)
                {
                    Pawn pawn2 = list[index: j];

                    if (pawn2.apparel.WornApparel.Any(predicate: ap => ap is ShieldBelt))
                    {
                        LessonAutoActivator.TeachOpportunity(conc: ConceptDefOf.ShieldBelts, opp: OpportunityType.Critical);

                        break;
                    }
                }
            }

            Find.TickManager.slower.SignalForceNormalSpeedShort();
            Find.StoryWatcher.statsRecord.numRaidsEnemy++;

            return true;
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            var map = (Map)parms.target;

            if (parms.faction != null)
            {
                return true;
            }

            float num = parms.points;

            if (num <= 0f)
            {
                num = 999999f;
            }

            return PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(
                       points: num,
                       faction: out parms.faction,
                       validator: f => FactionCanBeGroupSource(f: f, map: map, desperate: false),
                       allowNonHostileToPlayer: false,
                       allowHidden: true,
                       allowDefeated: true,
                       allowNonHumanlike: false
                   )
                   || PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(
                       points: num,
                       faction: out parms.faction,
                       validator: f => FactionCanBeGroupSource(f: f, map: map, desperate: true),
                       allowNonHostileToPlayer: false,
                       allowHidden: true,
                       allowDefeated: true,
                       allowNonHumanlike: false
                   );
        }

#if RW10
        protected override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }

            var map = (Map)parms.target;

            if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs where d.Worker.CanUseWith(parms: parms, groupKind: groupKind) select d)
                .TryRandomElementByWeight(
                    weightSelector: d => d.Worker.SelectionWeight(map: map, basePoints: parms.points),
                    result: out parms.raidStrategy
                ))
            {
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            }
        }
#else
        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }

            var map = (Map)parms.target;

            if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs where d.Worker.CanUseWith(parms: parms, groupKind: groupKind) select d)
                        .TryRandomElementByWeight(
                            weightSelector: d => d.Worker.SelectionWeight(map: map, basePoints: parms.points),
                            result: out parms.raidStrategy
                        ))
            {
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            }
        }
#endif

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = "NightVisionFlareArrivalText".Translate(parms.faction.def.pawnsPlural, parms.faction.Name);
            text += "\n\n";
            text += "NightVisionFlarePawnText".Translate();
            Pawn pawn = pawns.Find(match: x => x.Faction.leader == x);

            if (pawn != null)
            {
                text += "\n\n";

                text += "EnemyRaidLeaderPresent".Translate(
                    arg1: pawn.Faction.def.pawnsPlural,
                    arg2: pawn.LabelShort,
                    arg3: pawn.Named(label: "LEADER")
                );
            }

            return text;
        }

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidEnemy".Translate(arg1: Faction.OfPlayer.def.pawnsPlural, arg2: parms.faction.def.pawnsPlural);
        }

        public static float ChanceForFlareRaid(Map target)
        {
            return SolarRaid_StoryWorker.FlareRaidDef.baseChance;
        }
    }
}