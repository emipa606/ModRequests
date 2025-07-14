using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanillaSocialInteractionsExpanded;
using Verse;

namespace VSIERationalTraitDevelopment
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappableAttribute]
    public class Dialog_TraitChoices : Window
    {
        private Trait chosenTrait;

        private float scrollHeight;

        private Vector2 scrollPosition;
        public override Vector2 InitialSize => new Vector2(300f, 400);

        public Pawn pawn;

        public List<Trait> traitChoices;

        public string letterTextKey;
        public Dialog_TraitChoices(Pawn pawn, string letterTextKey, List<Trait> traitChoices)
        {
            this.pawn = pawn;
            this.letterTextKey = letterTextKey;
            this.traitChoices = traitChoices;
            this.chosenTrait = traitChoices.First();
            forcePause = true;
            absorbInputAroundWindow = true;
            if (!SelectionsMade())
            {
                closeOnAccept = false;
                closeOnCancel = false;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            float curY = 0;
            Widgets.Label(0f, ref curY, inRect.width, "VSIE.PickTrait".Translate(pawn).Resolve() + ":");
            Rect outRect = new Rect(inRect.x, curY + 15, inRect.width, inRect.height - 120);
            outRect.yMax -= 4f + CloseButSize.y;
            Text.Font = GameFont.Small;
            curY = outRect.y;
            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            DrawTraitChoices(viewRect.width, ref curY);
            if (Event.current.type == EventType.Layout)
            {
                scrollHeight = Mathf.Max(curY - 24 - 15, outRect.height);
            }
            Widgets.EndScrollView();
            var titleRect = new Rect(inRect.x, outRect.yMax + 15, inRect.width, 72);
            var title = letterTextKey.Translate(chosenTrait.CurrentData.GetLabelFor(pawn), "", pawn.Named("PAWN"));
            Widgets.Label(titleRect, title);
            Rect rect = new Rect(0f, titleRect.yMax, inRect.width, CloseButSize.y);
            AcceptanceReport acceptanceReport = CanClose();
            if (VSIERationalTraitDevelopmentSettings.allowCancelUI)
            {
                var cancelButtonRect = new Rect(rect.x, rect.y, CloseButSize.x, CloseButSize.y);
                if (Widgets.ButtonText(cancelButtonRect, "Cancel".Translate()))
                {
                    Close();
                }
            }
            Rect rect2 = new Rect(rect.xMax - CloseButSize.x, rect.y, CloseButSize.x, CloseButSize.y);
            if (Widgets.ButtonText(rect2, "OK".Translate()))
            {
                if (acceptanceReport.Accepted)
                {
                    GainTrait(pawn, chosenTrait, letterTextKey);
                    Close();
                }
                else
                {
                    Messages.Message(acceptanceReport.Reason, null, MessageTypeDefOf.RejectInput, historical: false);
                }
            }
        }

        public static void GainTrait(Pawn pawn, Trait chosenTrait, string letterTextKey)
        {
            pawn.story.traits.GainTrait(chosenTrait);
            var traitName = chosenTrait.CurrentData.GetLabelFor(pawn);
            var traitDesc = chosenTrait.CurrentData.description.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn).Resolve();
            Find.LetterStack.ReceiveLetter("VSIE.TraitChangeTitle".Translate(traitName, pawn.Named("PAWN")), letterTextKey.Translate(traitName, traitDesc, pawn.Named("PAWN")), LetterDefOf.NeutralEvent, pawn);
            Current.Game.GetComponent<SocialInteractionsManager>().pawnsWithAdditionalTrait.Add(pawn);
        }

        private bool SelectionsMade()
        {
            if (!traitChoices.NullOrEmpty() && chosenTrait == null)
            {
                return false;
            }
            return true;
        }
        private AcceptanceReport CanClose()
        {
            if (!SelectionsMade())
            {
                return "VSIE.SelectATrait".Translate();
            }
            return AcceptanceReport.WasAccepted;
        }

        private void DrawTraitChoices(float width, ref float curY)
        {
            if (!traitChoices.NullOrEmpty())
            {
                Listing_Standard listing_Standard = new Listing_Standard();
                Rect rect = new Rect(0f, curY, 230f, 99999f);
                listing_Standard.Begin(rect);
                foreach (Trait traitChoice in traitChoices)
                {
                    if (listing_Standard.RadioButton(traitChoice.LabelCap, chosenTrait == traitChoice, 30f, traitChoice.TipString(pawn)))
                    {
                        chosenTrait = traitChoice;
                    }
                }
                listing_Standard.End();
                curY += listing_Standard.CurHeight + 10f + 4f;
            }
        }
    }
}
