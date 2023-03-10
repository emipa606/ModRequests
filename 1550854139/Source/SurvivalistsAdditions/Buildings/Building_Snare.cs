using System;
using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SurvivalistsAdditions
{

    public class Building_Snare : Building_TrapDamager
    {

        private Pawn affectedPawn;
        private bool rearmAfterCleared;
        public float stunDuration = 20;

        private List<Pawn> touchingPawns = new List<Pawn>();

        #region properties


        public int Difficulty
        {
            get
            {
                return 1;
            }
        }

        public float FailChance
        {
            get
            {
                return (SrvSettings.Snare_FailChance * Difficulty) / 100f;
            }
        }

        public float BreakChance
        {
            get
            {
                return (SrvSettings.Snare_BreakChance * Difficulty) / 100f;
            }
        }

        #endregion

        #region Overrides

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rearmAfterCleared, "rearmAfterCleared", true);
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

        }

        public override void Tick()
        {

            if (base.Spawned)
            {

                //check to see if we're sprung
                List<Thing> thingList = base.Position.GetThingList(base.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Pawn pawn = thingList[i] as Pawn;
                    if (pawn != null && !touchingPawns.Contains(pawn))
                    {
                        touchingPawns.Add(pawn);
                        CheckSpring(pawn);
                    }
                }
                
            }
            
        }

        protected override float SpringChance(Pawn p)
        {
            float num;
            if (!IsValidAnimal(p))
            {
                return 0;
            }
            if (KnowsOfSnare(p))
            {
                num = 0.008f;
            }
            else
            {
                num = this.GetStatValue(StatDefOf.TrapSpringChance, true);
            }
            if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
            {
                num *= 0.5f;
            }
            return Mathf.Clamp01(num);
        }

        public override ushort PathWalkCostFor(Pawn p)
        {
            if (KnowsOfSnare(p))
            {
                return 30;
            }
            return 0;
        }

        public override ushort PathFindCostFor(Pawn p)
        {
            if (!KnowsOfSnare(p))
            {
                return 0;
            }
            return 800;
        }

        public override bool IsDangerousFor(Pawn p)
        {
            return KnowsOfSnare(p);
        }

        protected override void SpringSub(Pawn p)
        {
            if (p != null && Rand.Value > FailChance)
            {
                affectedPawn = p;
                ApplyDamage(p);

            }

            foreach (Pawn pawn in touchingPawns)
            {
                if (!pawn.Dead && (pawn.health.hediffSet.HasHediff(SrvDefOf.SRV_SnaredLarge) || pawn.health.hediffSet.HasHediff(SrvDefOf.SRV_SnaredSmall)))
                {
                    Map.designationManager.RemoveAllDesignationsOn(this);
                    rearmAfterCleared = true;
                    break;
                }
            }

            CheckAutoRebuild(base.Map);

            base.SpringSub(p);
        }

        #endregion

        public bool KnowsOfSnare(Pawn p)
        {
            if (p.Faction == null && p.RaceProps.Animal)
            {
                return false;
            }

            return KnowsOfTrap(p);

        }

        private bool IsValidAnimal(Pawn p)
        {
            if (!p.RaceProps.Animal)
            {
                return false;
            }
            if (p.BodySize > 1f)
            {
                return false;
            }
            if ((p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer) &&
                     p.RaceProps.trainability.intelligenceOrder >= TrainabilityDefOf.Intermediate.intelligenceOrder)
            {
                return false;
            }
            return true;
        }

        private void CheckSpring(Pawn p)
        {
            if (Rand.Chance(SpringChance(p)))
            {
                Spring(p);
            }
        }

        private void CheckAutoRebuild(Map map)
        {
            if (map != null)
            {
                GenConstruct.PlaceBlueprintForBuild(def, base.Position, map, base.Rotation, Faction.OfPlayer, base.Stuff);
            }
        }

        private void ApplyDamage(Pawn p)
        {
            if (p.BodySize > 1f)
            {
                return;
            }
            if (p.BodySize >= 0.6f && p.BodySize <= 1f)
            {
                p.health.AddHediff(SrvDefOf.SRV_SnaredLarge);
            }
            else
            {
                if (!p.health.hediffSet.HasHediff(SrvDefOf.SRV_SnaredSmall))
                {
                    NotifyPlayer(p);
                }
                p.health.AddHediff(SrvDefOf.SRV_SnaredSmall);
            }
        }


        private void RemoveHediff()
        {
            if (affectedPawn != null)
            {
                Hediff largeDiff = affectedPawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == SrvDefOf.SRV_SnaredLarge);
                if (largeDiff != null)
                {
                    affectedPawn.health.RemoveHediff(largeDiff);
                }
                Hediff smallDiff = affectedPawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == SrvDefOf.SRV_SnaredSmall);
                if (smallDiff != null)
                {
                    affectedPawn.health.RemoveHediff(smallDiff);
                }
                // Try to enter either a berserk or panicking state as a result of being snared
                // Larger animals are more likely to go berserk, smaller animals are more likely to flee
                if (Rand.Chance(affectedPawn.BodySize))
                {
                    affectedPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, true);
                }
                else if (!Rand.Chance(affectedPawn.BodySize))
                {
                    affectedPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null, true);
                }
            }
        }


        private void NotifyPlayer(Pawn p)
        {
            Map map = base.Map;
            // If the notification type isn't None, try to notify the player
            if (SrvSettings.Snare_NotificationType != NotificationType.None)
            {
                // Determine if this is a positive notification or not
                bool isPositive = !(p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer);
                // Check if this notification is allowed
                if ((isPositive && SrvSettings.Snare_AllowPositiveNotification) || (!isPositive && SrvSettings.Snare_AllowNegativeNotification))
                {

                    // If the notification type is SilentText
                    if (SrvSettings.Snare_NotificationType == NotificationType.SilentText)
                    {
                        Messages.Message("SRV_LetterSnareTriggered".Translate(p.LabelShort, p),
                            new TargetInfo(Position, Map),
                            MessageTypeDefOf.SilentInput
                        );
                    }
                    // If the notification type is TextWithSound
                    else if (SrvSettings.Snare_NotificationType == NotificationType.TextWithSound)
                    {
                        Messages.Message("SRV_LetterSnareTriggered".Translate(p.LabelShort, p),
                            new TargetInfo(Position, Map),
                            (isPositive) ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NegativeEvent
                        );
                    }
                    // If the notification type is Letter
                    else
                    {
                        Find.LetterStack.ReceiveLetter("SRV_LetterSnareTriggeredLabel".Translate(p.LabelShort, p),
                            "SRV_LetterSnareTriggered".Translate(p.LabelShort, p),
                            (isPositive) ? LetterDefOf.PositiveEvent : LetterDefOf.NegativeEvent,
                            new TargetInfo(Position, Map, false)
                        );
                    }
                }
            }
        }



    }
}
