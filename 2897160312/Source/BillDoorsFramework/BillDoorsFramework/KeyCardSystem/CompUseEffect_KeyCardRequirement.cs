using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    public class CompUsable_KeyCardRequirement : CompUsable
    {
        public CompProperties_Useable_KeyCardRequirement Props => (CompProperties_Useable_KeyCardRequirement)props;

        public Thing FindBestKeyCard(Pawn pawn)
        {
            if (pawn == null || Props.keyCardDefs.NullOrEmpty()) return null;
            List<Thing> keyCards = new List<Thing>();

            foreach (ThingDef keyCardDef in Props.keyCardDefs)
            {
                Thing keyCard = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(keyCardDef), PathEndMode.Touch, TraverseParms.For(pawn), validator: (Thing key) => (pawn.CanReserve(key) || pawn.HasReserved(key)) && !key.IsForbidden(pawn));
                if (keyCard != null) keyCards.Add(keyCard);
            }

            List<Thing> inventoryKeyCards = FindKeyCardsFromInventory(pawn).ToList();
            keyCards = keyCards.Concat(inventoryKeyCards).ToList();

            if (keyCards.NullOrEmpty()) return null;
            if (keyCards.Count() == 1) return keyCards[0];
            if (Props.consumeCard)
            {
                keyCards.SortBy((Thing t) => { return t.MarketValue; });
                return keyCards[0];
            }
            else
            {
                if (inventoryKeyCards.Any())
                {
                    return inventoryKeyCards[0];
                }
                return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, keyCards, PathEndMode.Touch, TraverseParms.For(pawn));
            }
        }

        public IEnumerable<Thing> FindKeyCardsFromInventory(Pawn pawn)
        {
            foreach (Thing t in pawn.inventory.innerContainer)
            {
                if (Props.keyCardDefs.Contains(t.def))
                {
                    yield return t;
                }
            }
        }

        public bool isRequiredKeyCard(ThingDef def)
        {
            if (Props.keyCardDefs.NullOrEmpty()) return false;

            return Props.keyCardDefs.Contains(def);
        }

        public bool isRequiredKeyCard(Thing thing)
        {
            if (thing == null) return false;

            return isRequiredKeyCard(thing.def);
        }


        public override AcceptanceReport CanBeUsedBy(Pawn p, bool forced = false, bool ignoreReserveAndReachable = false)
        {
            Thing keycard = FindBestKeyCard(p);
            if (keycard == null && !isRequiredKeyCard(p.carryTracker.CarriedThing?.def))
            {
                return "BDF_NoReachableKeycard".Translate().CapitalizeFirst();
            }
            return base.CanBeUsedBy(p);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, StatDefOf.BDsKeyCardRequirmentStat);
        }

        public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget, bool forced = false)
        {
            if (!pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, forced) || !CanBeUsedBy(pawn))
            {
                return;
            }
            Thing keycard = FindBestKeyCard(pawn);
            if (keycard == null)
            {
                return;
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (Props.consumeCard)
            {
                stringBuilder.AppendLine("BDF_ConsumeKeyCardWarning".Translate(keycard.LabelNoParenthesis));
                if (keycard.MarketValue > 250)
                {
                    stringBuilder.AppendLine("BDF_ConsumeHighValueKeyCardWarning".Translate(keycard.LabelNoParenthesis));
                }
            }
            foreach (CompUseEffect comp in parent.GetComps<CompUseEffect>())
            {
                TaggedString taggedString = comp.ConfirmMessage(pawn);
                if (!taggedString.NullOrEmpty())
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.AppendTagged(taggedString);
                }
            }
            string text = stringBuilder.ToString();
            if (text.NullOrEmpty())
            {
                StartJob();
            }
            else
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, StartJob));
            }
            void StartJob()
            {
                Job job = JobMaker.MakeJob(Props.useJob, parent, keycard);
                job.count = 1;
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }

        public override void UsedBy(Pawn p)
        {
            base.UsedBy(p);
            foreach (CompKeyCardUsedEffect item in parent.GetComps<CompKeyCardUsedEffect>())
            {
                try
                {
                    item.UsedEffect(this);
                }
                catch (Exception ex)
                {
                    Log.Error("Error in CompKeyCardUsedEffect: " + ex);
                }
            }
        }
    }

    public class CompProperties_Useable_KeyCardRequirement : CompProperties_Usable
    {
        public CompProperties_Useable_KeyCardRequirement()
        {
            compClass = typeof(CompUsable_KeyCardRequirement);
        }

        public List<ThingDef> keyCardDefs;

        public bool consumeCard;
    }
}
