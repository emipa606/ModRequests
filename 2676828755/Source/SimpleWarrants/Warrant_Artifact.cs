using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace SimpleWarrants
{
    [HotSwapAll]
    [StaticConstructorOnStartup]
    public class Warrant_Artifact : Warrant
    {
        public static readonly Texture2D IconRetrieve = ContentFinder<Texture2D>.Get("UI/Warrants/IconRetrieve");
        public int reward;

        public override void Draw(Rect rect, bool doAcceptAndDeclineButtons = true, bool doCompensateWarrantButton = false)
        {
            base.Draw(rect, doAcceptAndDeclineButtons, doCompensateWarrantButton);
            var thingRect = new Rect(new Vector2(rect.x + 100, rect.y + 10), new Vector2(rect.height * 0.7f, rect.height));
            GUI.DrawTexture(thingRect, thing.Graphic.MatSouth.mainTexture, ScaleMode.ScaleToFit);

            Widgets.InfoCardButton(thingRect.xMax - 24, thingRect.yMax - 24, thing);
            Text.Font = GameFont.Medium;
            var textSize = Text.CalcSize(thing.LabelCap);
            var nameInfoBox = new Rect(thingRect.xMax, thingRect.y, textSize.x, 30);
            Widgets.Label(nameInfoBox, thing.LabelCap);
            if (issuer.IsPlayer && MaxRewardValue() < (thing.MarketValue * 0.75f))
            {
                var insufficientRewardBox = new Rect(nameInfoBox.xMax + 5, nameInfoBox.y + 3, 24, 24);
                GUI.DrawTexture(insufficientRewardBox, InsufficientRewardIcon);
                TooltipHandler.TipRegion(insufficientRewardBox, "SW.InsufficientReward".Translate());
            }
            var postedByInfoBox = new Rect(nameInfoBox.x, nameInfoBox.yMax, 400, nameInfoBox.height);
            Widgets.Label(postedByInfoBox, "SW.PostedBy".Translate(issuer.NameColored));

            var rewardIconBox = new Rect(nameInfoBox.x, postedByInfoBox.yMax, 24, 24);
            GUI.DrawTexture(rewardIconBox, IconRetrieve);
            var rewardInfoBox = new Rect(rewardIconBox.xMax + 5, postedByInfoBox.yMax, 400, nameInfoBox.height);
            Widgets.Label(rewardInfoBox, reward + " " + ThingDefOf.Silver.LabelCap);

            var infoBox = new Rect(rect.width - 250, rewardInfoBox.yMax + 40, 250, 24);
            Text.Font = GameFont.Tiny;
            if (issuer != Faction.OfPlayer)
            {
                var expireDate = (relatedQuest != null ? acceptedTick : createdTick) + (GenDate.TicksPerDay * 15) - Find.TickManager.TicksGame;
                Widgets.Label(infoBox, "SW.WillExpireIn".Translate(expireDate.ToStringTicksToDays()));
            }
            else
            {
                if (accepteer != null)
                {
                    Widgets.Label(infoBox, "SW.ApproximateComplectionDate".Translate(ApproximateCompletionDate.ToStringTicksToDays()));
                }
                else
                {
                    Widgets.Label(infoBox, "SW.ApproximateAcceptionDate".Translate(ApproximateAcceptionDate.ToStringTicksToDays()));
                }
            }
            Text.Font = GameFont.Small;

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref reward, "reward");
        }

        public override void DoAcceptAction()
        {
            base.DoAcceptAction();
            Slate slate = new Slate();
            slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(Find.World));
            slate.Set("asker", issuer.leader);
            slate.Set("artifactLabel", thing.Label);
            slate.Set("artifact", thing);
            slate.Set("warrant", this);
            slate.Set("reward", reward);
            var quest = QuestUtility.GenerateQuestAndMakeAvailable(SW_DefOf.SW_Warrant_Artifact, slate);
            QuestUtility.SendLetterQuestAvailable(quest);
        }

        public override void GiveReward(Caravan caravan, Thing thingHandedIn)
        {
            base.GiveReward(caravan, thingHandedIn);
            if (reward <= 0)
                return;

            var silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = reward;
            GiveThing(caravan, silver);
            Log.Message(this + " - Giving reward: " + silver + " - " + silver.stackCount + " for " + thing);
        }

        public override bool IsWarrantActive()
        {
            return !thing.Destroyed;
        }

        public override float AcceptChance()
        {
            acceptChanceCached ??= reward / thing.MarketValue;
            return acceptChanceCached.Value;
        }

        public override float SuccessChance()
        {
            successChanceCached ??= reward / thing.MarketValue;
            return successChanceCached.Value;
        }

        public override bool ShouldShowCompensateButton()
        {
            return false;
        }

        public override bool IsThreatForPlayer()
        {
            return false;
        }

        public override int MaxRewardValue()
        {
            return reward;
        }
    }
}