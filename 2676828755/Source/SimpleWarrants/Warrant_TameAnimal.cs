using RimWorld;
using RimWorld.QuestGen;
using System;
using UnityEngine;
using Verse;

namespace SimpleWarrants
{
    public class Warrant_TameAnimal : Warrant
    {
        public override bool UsesThings => false;

        public int Reward;
        public PawnKindDef AnimalRace;

        public override float AcceptChance()
        {
            // Not used because the player cannot create animation warrants.
            throw new NotImplementedException();
        }

        public override float SuccessChance()
        {
            // Success chance is not used because the player cannot create animal warrants.
            throw new NotImplementedException();
        }

        public override void DoAcceptAction()
        {
            base.DoAcceptAction();

            Slate slate = new Slate();
            slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(Find.World));
            slate.Set("asker", issuer.leader);
            slate.Set("warrant", this);
            slate.Set("reward", Reward);
            slate.Set("animal", AnimalRace);
            var questDef = SW_DefOf.SW_Warrant_Tame;
            var quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate);
            QuestUtility.SendLetterQuestAvailable(quest);
        }

        public override void Draw(Rect rect, bool doAcceptAndDeclineButtons = true, bool doCompensateWarrantButton = false)
        {
            base.Draw(rect, doAcceptAndDeclineButtons, doCompensateWarrantButton);

            var pawnRect = new Rect(new Vector2(rect.x + 100, rect.y + 10), new Vector2(rect.height * 0.7f, rect.height));
            GUI.color = AnimalRace.race.uiIconColor;
            GUI.DrawTexture(pawnRect, AnimalRace.race.uiIcon, ScaleMode.ScaleToFit);
            GUI.color = Color.white;
            Widgets.InfoCardButton(pawnRect.xMax - 24, pawnRect.yMax - 24, AnimalRace.race);

            Text.Font = GameFont.Medium;
            var pawnName = $"<color=#bced93>{"SW.Tame".Translate()}</color> {AnimalRace.LabelCap}";
            var textSize = Text.CalcSize(pawnName);
            var nameInfoBox = new Rect(pawnRect.xMax, pawnRect.y, textSize.x, 30);
            Widgets.Label(nameInfoBox, pawnName);

            var wantedForInfoBox = new Rect(nameInfoBox.x, nameInfoBox.yMax, rect.width - pawnRect.width, nameInfoBox.height);
            Widgets.Label(wantedForInfoBox, "SW.PostedBy".Translate(issuer.NameColored));

            var rewardsForDeadIconBox = new Rect(wantedForInfoBox.x, wantedForInfoBox.yMax, 24, 24);
            GUI.DrawTexture(rewardsForDeadIconBox, Warrant_Pawn.IconCapture);

            var rewardsForDeadInfoBox = new Rect(rewardsForDeadIconBox.xMax + 5, wantedForInfoBox.yMax, wantedForInfoBox.width / 3, wantedForInfoBox.height);
            if (Reward > 0)
            {
                Widgets.Label(rewardsForDeadInfoBox, Reward + " " + ThingDefOf.Silver.LabelCap);
            }
            else
            {
                Widgets.Label(rewardsForDeadInfoBox, "SW.NoReward".Translate());
            }

            var rewardsForLivingIconBox = new Rect(rewardsForDeadInfoBox.xMax, wantedForInfoBox.yMax, 24, 24);
            var rewardsForLivingInfoBox = new Rect(rewardsForLivingIconBox.xMax + 5, wantedForInfoBox.yMax, wantedForInfoBox.width / 3, wantedForInfoBox.height);

            var infoBox = new Rect(rect.width - 250, rewardsForLivingInfoBox.yMax + 40, 250, 24);
            Text.Font = GameFont.Tiny;

            var expireDate = (relatedQuest != null ? acceptedTick : createdTick) + (GenDate.TicksPerDay * 15) - Find.TickManager.TicksGame;
            Widgets.Label(infoBox, "SW.WillExpireIn".Translate(expireDate.ToStringTicksToDays()));

            Text.Font = GameFont.Small;
        }

        public override bool IsWarrantActive() => true;

        public override bool IsThreatForPlayer() => false;

        public override bool ShouldShowCompensateButton() => false;

        public override int MaxRewardValue() => Reward;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref AnimalRace, "animalRace");
            Scribe_Values.Look(ref Reward, "reward");
        }
    }
}
