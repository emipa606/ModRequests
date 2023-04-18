using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SimpleWarrants
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HotSwapAllAttribute : Attribute {}

	[HotSwapAll]
	[StaticConstructorOnStartup]
	public class MainTabWindow_Warrants : MainTabWindow
	{
        private readonly List<TabRecord> tabs = new List<TabRecord>();
        private string buffCurCapturePayment;
        private string buffCurDeathPayment;
        private string buffCurReward;

        private bool capturePaymentEnabled;
        private Pawn curAnimal;
        private Thing curArtifact;
        private int curCapturePayment; 
        private int curDeathPayment;

        private Pawn curPawn;
        private string curReason;
        private int curReward;
        private WarrantsTab curTab;
        private TargetType curType;
        private bool deathPaymentEnabled;
        private Vector2 scrollPosition;

        public override void PreOpen()
		{
			base.PreOpen();
			tabs.Clear();
			tabs.Add(new TabRecord("SW.PublicWarrants".Translate(), delegate
			{
				curTab = WarrantsTab.PublicWarrants;
			}, () => curTab == WarrantsTab.PublicWarrants));
			tabs.Add(new TabRecord("SW.RelatedWarrants".Translate(), delegate
			{
				curTab = WarrantsTab.RelatedWarrants;
			}, () => curTab == WarrantsTab.RelatedWarrants));
		}

        public override void DoWindowContents(Rect rect)
		{
			Rect rect2 = rect;
			rect2.yMin += 45f;
			TabDrawer.DrawTabs(rect2, tabs);
			switch (curTab)
			{
				case WarrantsTab.PublicWarrants:
					DoPublicWarrants(rect2);
					break;
				case WarrantsTab.RelatedWarrants:
					DoRelatedWarrants(rect2);
					break;
			}
			DoWarrantCreation(rect2);
		}

        private void DoPublicWarrants(Rect rect)
        {
			var warrants = WarrantsManager.Instance.availableWarrants.Where(x => x.thing?.Faction != Faction.OfPlayer).OrderByDescending(x => x.createdTick).ToList();
			var posY = rect.y + 10;
			var sectionWidth = 750;
			var outRect = new Rect(rect.x, posY, sectionWidth, 590);
			var viewRect = new Rect(outRect.x, posY, sectionWidth - 16, warrants.Count * 165);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			if (warrants.Count > 0)
            {
				for (var i = 0; i < warrants.Count; i++)
				{
					var warrantBox = new Rect(rect.x, posY, sectionWidth - 30, 150);
					warrants[i].Draw(warrantBox);
					posY = warrantBox.yMax + 15;
				}
			}
			else
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.Font = GameFont.Medium;
				Widgets.Label(outRect, "SW.NoPublicWarrantsAvailable".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			}
			Widgets.EndScrollView();
        }

        private void DoRelatedWarrants(Rect rect)
		{
			var warrants = WarrantsManager.Instance.acceptedWarrants.Concat(WarrantsManager.Instance.createdWarrants).Concat(WarrantsManager.Instance.takenWarrants)
				.Concat(WarrantsManager.Instance.availableWarrants.Where(x => x.thing?.Faction == Faction.OfPlayer)).OrderByDescending(x => x.createdTick).ToList();
			var posY = rect.y + 10;
			var sectionWidth = 750;
			var outRect = new Rect(rect.x, posY, sectionWidth, 590);
			var viewRect = new Rect(outRect.x, posY, sectionWidth - 16, warrants.Count * 165);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			if (warrants.Count > 0)
            {
				for (var i = 0; i < warrants.Count; i++)
				{
					var warrantBox = new Rect(rect.x, posY, sectionWidth - 30, 150);
					warrants[i].Draw(warrantBox, false, true);
					posY = warrantBox.yMax + 15;
				}
			}
			else
            {
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.Font = GameFont.Medium;
				Widgets.Label(outRect, "SW.NoRelatedWarrantsAvailable".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			}
			Widgets.EndScrollView();
		}

        private void DoWarrantCreation(Rect rect)
		{
			var posY = rect.y + 10;
			var createWarrant = new Rect(790, posY, 160, 30);

            if (Widgets.ButtonText(createWarrant, "SW.CreateWarrant".Translate()))
            {
				var warrant = CreateWarrant(out string failReason);
				if (!failReason.NullOrEmpty())
                {
					Find.WindowStack.Add(new Dialog_MessageBox(failReason));
                }
				else
				{
					warrant.OnCreate();
					WarrantsManager.Instance.createdWarrants.Add(warrant);
				}
            }

			Text.Font = GameFont.Medium;
			var warrantSubject = new Rect(createWarrant.x, createWarrant.yMax + 20, createWarrant.width, createWarrant.height);
			Widgets.Label(warrantSubject, "SW.WarrantSubject".Translate());

			var dropdownRect = new Rect(createWarrant.x, warrantSubject.yMax, createWarrant.width, createWarrant.height);
			if (Widgets.ButtonTextSubtle(dropdownRect, GetLabel(curType)))
            {
				var floatList = new List<FloatMenuOption>();
				foreach (var value in Enum.GetValues(typeof(TargetType)).Cast<TargetType>())
                {
					floatList.Add(new FloatMenuOption(GetLabel(value), delegate
					{
						curType = value;
					}));
                }
				Find.WindowStack.Add(new FloatMenu(floatList));
            }

			if (curType == TargetType.Human || curType == TargetType.Animal)
            {
                if (curType == TargetType.Human && curPawn is null)
                {
                    if (!Find.WorldPawns.AllPawnsAlive.Where(pawn => pawn?.story != null && pawn.RaceProps.Humanlike
                        && !WarrantsManager.Instance.createdWarrants.Any(warrant => pawn == warrant.thing)).TryRandomElement(out curPawn))
                    {
                        var randomKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike).RandomElement();
                        Faction faction = null;
                        if (randomKind.defaultFactionType != null)
                        {
                            faction = Find.FactionManager.FirstFactionOfDef(randomKind.defaultFactionType);
                        }
                        faction ??= Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction && !x.defeated && !x.IsPlayer && !x.Hidden).RandomElement();
                        curPawn = PawnGenerator.GeneratePawn(randomKind, faction);
                    }
                }
                else if (curType == TargetType.Animal && curAnimal is null)
                {
                    curAnimal = PawnGenerator.GeneratePawn(Utils.AllWorthAnimalDefs.RandomElement());
                }

                DrawPawnWarrant(curType == TargetType.Human ? curPawn : curAnimal, createWarrant, dropdownRect);
            }
            else
            {
				if (curArtifact is null) 
				{
					var artifactDef = Utils.AllArtifactDefs.RandomElement();
					curArtifact = ThingMaker.MakeThing(artifactDef);
				}

				var thingRect = new Rect(new Vector2(createWarrant.x + 40, dropdownRect.yMax + 10), new Vector2(100 * 0.722f, 100 * 0.722f));
				GUI.DrawTexture(thingRect, curArtifact.Graphic.MatSouth.mainTexture);
				Widgets.InfoCardButton(thingRect.xMax, thingRect.y + 10, curArtifact);

				var nameRect = new Rect(createWarrant.x, thingRect.yMax, createWarrant.width, createWarrant.height);
				Widgets.Label(nameRect, curArtifact.LabelCap);

				dropdownRect = new Rect(createWarrant.x, nameRect.yMax, createWarrant.width, createWarrant.height);
				if (Widgets.ButtonTextSubtle(dropdownRect, "SW.Select".Translate()))
				{
					var selectArtifact = new Dialog_SelectArtifact(this);
					Find.WindowStack.Add(selectArtifact);
				}

				Text.Font = GameFont.Small;
				var rewardPayment = new Rect(createWarrant.x - 30, dropdownRect.yMax + 10, 130, 24);
				Widgets.Label(rewardPayment, "SW.RewardPayment".Translate());
				var rewardPaymentInput = new Rect(rewardPayment.xMax, rewardPayment.y, 60, 24);
				Widgets.TextFieldNumeric(rewardPaymentInput, ref curReward, ref buffCurReward);
			}
			Text.Font = GameFont.Small;
		}

        private void DrawPawnWarrant(Pawn pawn, Rect createWarrant, Rect dropdownRect)
        {
            var pawnRect = new Rect(new Vector2(createWarrant.x + 40, dropdownRect.yMax + 10), new Vector2(100 * 0.722f, 100));
            Vector2 pos = new Vector2(pawnRect.width, pawnRect.height);
            GUI.DrawTexture(pawnRect, PortraitsCache.Get(pawn, pos, Rot4.South, new Vector3(0f, 0f, 0f), 1.2f));
            Widgets.InfoCardButton(pawnRect.xMax, pawnRect.y + 10, pawn);

            var nameRect = new Rect(createWarrant.x, pawnRect.yMax, createWarrant.width, createWarrant.height);
			if (curType == TargetType.Human)
            {
				Widgets.Label(nameRect, pawn.Name.ToString());
			}
			else
            {
				Widgets.Label(nameRect, pawn.def.LabelCap);
			}

			dropdownRect = new Rect(createWarrant.x, nameRect.yMax, createWarrant.width, createWarrant.height);
            if (Widgets.ButtonTextSubtle(dropdownRect, "SW.Select".Translate()))
            {
				if (curType == TargetType.Human)
                {
					Find.WindowStack.Add(new Dialog_SelectPawn(this));
				}
				else
                {
					Find.WindowStack.Add(new Dialog_SelectAnimal(this));
				}
			}

			var reasonRect = new Rect(dropdownRect.x, dropdownRect.yMax + 10, createWarrant.width, createWarrant.height);
			if (curType == TargetType.Human)
            {
				if (curReason.NullOrEmpty())
				{
					curReason = Utils.GenerateTextFromRule(SW_DefOf.SW_WantedFor, pawn.thingIDNumber);
				}

				if (Widgets.ButtonTextSubtle(reasonRect, "SW.Reason".Translate(curReason)))
				{
					var floatList = new List<FloatMenuOption>();
					foreach (var value in Utils.GenerateAllTextFromRule(SW_DefOf.SW_WantedFor).OrderBy(x => x))
					{
						floatList.Add(new FloatMenuOption(value, delegate
						{
							curReason = value;
						}));
					}
					Find.WindowStack.Add(new FloatMenu(floatList));
				}
			}


            Text.Font = GameFont.Small;
            var capturePayment = new Rect(reasonRect.x - 30, reasonRect.yMax + 10, 120, 24);

			Widgets.Label(capturePayment, "SW.CapturePayment".Translate());
			var capturePaymentInput = new Rect(capturePayment.xMax, capturePayment.y, 60, 24);
			if (capturePaymentEnabled)
			{
				Widgets.TextFieldNumeric(capturePaymentInput, ref curCapturePayment, ref buffCurCapturePayment);
			}
			Widgets.Checkbox(capturePaymentInput.xMax + 5, capturePaymentInput.y, ref capturePaymentEnabled);

			var deathPayment = new Rect(capturePayment.x, capturePayment.yMax + 5, capturePayment.width, capturePayment.height);
			Widgets.Label(deathPayment, "SW.DeathPayment".Translate());
			var deathPaymentInput = new Rect(deathPayment.xMax, deathPayment.y, 60, 24);
			if (deathPaymentEnabled)
			{
				Widgets.TextFieldNumeric(deathPaymentInput, ref curDeathPayment, ref buffCurDeathPayment);
			}
			Widgets.Checkbox(deathPaymentInput.xMax + 5, deathPaymentInput.y, ref deathPaymentEnabled);
        }

        public void AssignPawn(Pawn pawn)
        {
			curPawn = pawn;
        }

        public void AssignArtifact(Thing artifact)
        {
			curArtifact = artifact;
		}

        public void AssignAnimal(Pawn animal)
		{
			curAnimal = animal;
		}

        private Warrant CreateWarrant(out string failReason)
        {
			failReason = "";

            const int MAX_WARRANT_COUNT = 10;

            if (WarrantsManager.Instance.createdWarrants.Count >= MAX_WARRANT_COUNT)
            {
                failReason = "SW.TooManyPlayerWarrants".Translate(MAX_WARRANT_COUNT);
                return null;
            }

			switch (curType)
            {
				case TargetType.Human:
				case TargetType.Animal: 
					return CreatePawnWarrant(ref failReason);
				case TargetType.Artifact: return CreateArtifactWarrant(ref failReason);
			}
			return null;
		}

        private Warrant CreateArtifactWarrant(ref string failReason)
        {
            var warrant = new Warrant_Artifact
            {
                loadID = WarrantsManager.Instance.GetWarrantID(),
                issuer = Faction.OfPlayer,
                createdTick = Find.TickManager.TicksGame
            };
            warrant.thing = curArtifact;
            warrant.reward = curReward;
            if (curReward <= 0)
            {
                failReason = "SW.YouMustFillAmountForReward".Translate();
            }
			else
            {
				curArtifact = null;
            }
            return warrant;
        }

        private Warrant CreatePawnWarrant(ref string failReason)
        {
            var warrant = new Warrant_Pawn
            {
                loadID = WarrantsManager.Instance.GetWarrantID(),
                issuer = Faction.OfPlayer,
                createdTick = Find.TickManager.TicksGame
            };

            warrant.thing = curType == TargetType.Human ? curPawn : curAnimal;

            warrant.rewardForLiving = curCapturePayment;
            warrant.rewardForDead = curDeathPayment;
            warrant.reason = curReason;
            if (deathPaymentEnabled && curDeathPayment <= 0)
            {
                failReason = "SW.YouMustFillAmountForDeadReward".Translate();
            }
            else if (capturePaymentEnabled && curCapturePayment <= 0)
            {
                failReason = "SW.YouMustFillAmountForCaptureReward".Translate();
            }
			if (failReason.NullOrEmpty())
            {
				if (curType == TargetType.Human)
                {
					curPawn = null;
                }
				else
                {
					curAnimal = null;
                }
            }
            return warrant;
        }

        public string GetLabel(TargetType targetType)
        {
			switch (targetType)
            {
				case TargetType.Human: return "SW.Pawn".Translate();
				case TargetType.Artifact: return "SW.Artifact".Translate();
				case TargetType.Animal: return "SW.Animal".Translate();
            }
			return null;
        }
    }
}
