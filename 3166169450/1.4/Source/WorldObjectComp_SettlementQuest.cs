using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SettlementQuests
{
    public class WorldObjectCompProperties_SettlementQuest : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_SettlementQuest()
        {
            this.compClass = typeof(WorldObjectComp_SettlementQuest);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappableAttribute]
    public class WorldObjectComp_SettlementQuest : WorldObjectComp
    {
        private const int CooldownDuration = GenDate.TicksPerDay * 10;
        public int lastUsedTick;
        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            if (parent.Faction != Faction.OfPlayer && parent.Faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile)
            {
                var command = new Command_ActionWithCooldown(lastUsedTick, CooldownDuration)
                {
                    defaultLabel = "SQ.AskForQuest".Translate(),
                    defaultDesc = "SQ.AskForQuestDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark"),
                    action = () =>
                    {
                        var actions = new List<Pair<Action, float>>
                        {
                            new Pair<Action, float>(delegate
                            {
                                if (!TryGenerateQuest())
                                {
                                    var failOutcome = FailOutcomes(caravan).RandomElementByWeight(x => x.second);
                                    failOutcome.first();
                                }
                            }, 0.8f),
                        };
                        actions.AddRange(FailOutcomes(caravan));
                        var action = actions.RandomElementByWeight(x => x.second);
                        action.first();
                        lastUsedTick = Find.TickManager.TicksGame;
                    }
                };
                if (lastUsedTick > 0 && lastUsedTick + CooldownDuration > Find.TickManager.TicksGame)
                {
                    command.Disable("SQ.OnCooldown".Translate(((lastUsedTick + CooldownDuration) - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
                }
                yield return command;
            }
        }

        private List<Pair<Action, float>> FailOutcomes(Caravan caravan)
        {
            return new List<Pair<Action, float>>()
            {
                new Pair<Action, float>(delegate
                {
                    CaravanFight(caravan);
                }, 0.05f),
                new Pair<Action, float>(delegate
                {
                    CaravanInsult(caravan);
                }, 0.05f),
                new Pair<Action, float>(delegate
                {
                    CaravanNoLeads(caravan);
                }, 0.05f),
                new Pair<Action, float>(delegate
                {
                    CaravanParty(caravan);
                }, 0.05f)
            };
        }

        private void CaravanFight(Caravan caravan)
        {
            foreach (var pawn in caravan.PawnsListForReading.ToList())
            {
                pawn.TakeDamage(new DamageInfo(Rand.Bool ? DamageDefOf.Blunt : DamageDefOf.Cut, Rand.Range(1, 5)));
            }
            Messages.Message("SQ.FailOutcomeFight".Translate(), caravan, MessageTypeDefOf.NegativeHealthEvent);
        }

        private void CaravanInsult(Caravan caravan)
        {
            parent.Faction.TryAffectGoodwillWith(caravan.Faction, -Rand.Range(1, 5));
            Messages.Message("SQ.FailOutcomeInsult".Translate(), caravan, MessageTypeDefOf.NegativeEvent);
        }

        private void CaravanNoLeads(Caravan caravan)
        {
            Messages.Message("SQ.FailOutcomeNoLeads".Translate(), caravan, MessageTypeDefOf.NeutralEvent);
        }

        private void CaravanParty(Caravan caravan)
        {
            foreach (var pawn in caravan.PawnsListForReading.ToList())
            {
                HealthUtility.AdjustSeverity(pawn, HediffDefOf.AlcoholHigh, 0.25f);
            }
            Messages.Message("SQ.CaravanParty".Translate(), caravan, MessageTypeDefOf.PositiveEvent);
        }

        private bool TryGenerateQuest()
        {
            QuestNode_GetSiteTile_TryFindTile_Patch.worldObject = parent;
            List<QuestScriptDef> questDefsToProcess = DefDatabase<QuestScriptDef>.AllDefs.Where(x => !x.isRootSpecial && x.IsRootAny).ToList();
            var count = 0;
            while (count < 100)
            {
                count++;
                try
                {
                    var slate = new Slate();
                    slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(Find.World));
                    var questDef = questDefsToProcess.RandomElement();
                    if (questDef == QuestScriptDefOf.LongRangeMineralScannerLump)
                    {
                        slate.Set("targetMineable", ThingDefOf.MineableGold);
                        slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
                    }
                    if (questDef.CanRun(slate))
                    {
                        Quest quest = QuestGen.Generate(questDef, slate);
                        var rewardChoice = quest.PartsListForReading.OfType<QuestPart_Choice>().FirstOrDefault();
                        //Log.Message(quest + " - " + string.Join(", ", rewardChoice.choices.Select(x => string.Join(", ", x.rewards))));
                        if (rewardChoice != null && rewardChoice.choices.Any(x => x.rewards.Any(x => x is Reward_Goodwill goodwill && goodwill.faction == parent.Faction)))
                        {
                            Find.QuestManager.Add(quest);
                            if (!quest.hidden && quest.root.sendAvailableLetter)
                            {
                                QuestUtility.SendLetterQuestAvailable(quest);
                            }
                            QuestNode_GetSiteTile_TryFindTile_Patch.worldObject = null;
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            QuestNode_GetSiteTile_TryFindTile_Patch.worldObject = null;
            return false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastUsedTick, "lastUsedTick");
        }
    }
}
