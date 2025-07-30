using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Wheres_Daddy_Going
{
    public class MentalState_OutToGetSomeMilk : MentalState
    {
        private Pawn child = null;
        private ThingDef thingDef = null;

        public override string InspectLine => string.Format(base.InspectLine, thingDef.label);

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void PreStart()
        {
            base.PreStart();

            ParentalMemory memory = WheresDaddyGoingMod.Instance.MemoriesOfDad.GetOrCreateMemory(pawn);

            List<Pawn> childrenOnMap = pawn.relations.Children.Where(child => child.Map == pawn.Map).ToList();
            if (childrenOnMap.Count > 0)
                child = childrenOnMap.RandomElement();

            thingDef = memory.thingDef;
            if (thingDef == null)
            {
                thingDef = DefDatabase<ThingDef>.GetNamed("Milk");
                
                if (thingDef == null)
                    thingDef = ThingSetMaker_ResourcePod.RandomPodContentsDef(false);

                memory.thingDef = thingDef;
            }
        }

        public override void PostStart(string reason)
        {
            base.PostStart(reason);

            List<Pawn> relativesOnMap = pawn.relations.PotentiallyRelatedPawns.Where(otherPawn => otherPawn.Map == pawn.Map).ToList();

            // Make sure we don't add double thoughts in case of someone marrying their cousin or ex-mother-in-law or something
            List<Pawn> checkedPawns = new List<Pawn>();
            foreach(Pawn relative in relativesOnMap)
            {
                if (checkedPawns.Contains(relative))
                {
                    Logger.MessageFormat(this, "Already checked relation to {0}", relative);
                    continue;
                }

                PawnRelationDef relationDef = relative.GetMostImportantRelation(pawn);
                if (relationDef == null)
                {
                    Logger.MessageFormat(this, "No relation found between {0} and {1}", relative, pawn);
                    continue;
                }

                ThoughtDef lostThoughtDef = relationDef.GetGenderSpecificLostThought(pawn);
                if (lostThoughtDef == null)
                {
                    Logger.MessageFormat(this, "No lostThoughtDef found for {0} to {1}", relationDef, pawn);
                    continue;
                }

                string lostThoughtName = lostThoughtDef.defName;
                string leftThoughtName = lostThoughtName.Replace("Lost", "Left");

                if (leftThoughtName == lostThoughtName)
                {
                    Logger.MessageFormat(this, "Unable to rename thought: {0}", leftThoughtName);
                    continue;
                }

                ThoughtDef leftThoughtDef = DefDatabase<ThoughtDef>.GetNamed(leftThoughtName, false);
                if (leftThoughtDef == null)
                {
                    Logger.MessageFormat(this, "Could not find thought: {0}", leftThoughtName);
                    continue;
                }

                if (relative.needs == null || relative.needs.mood == null || relative.needs.mood.thoughts == null || relative.needs.mood.thoughts.memories == null)
                {
                    Logger.MessageFormat(this, "Could not find needs/mood/thoughts/memories for {0}", relative);
                    continue;
                }

                Logger.MessageFormat(this, "Attempting to add thought {0} to {1}", leftThoughtName, relative);
                relative.needs.mood.thoughts.memories.TryGainMemory(leftThoughtDef, pawn);

                checkedPawns.Add(relative);
            }
        }

        public override void PostEnd()
        {
            base.PostEnd();

            MindCleared();
        }

        public void MindCleared()
        {
            if (pawn.SpawnedOrAnyParentSpawned)
            {
                Logger.MessageFormat(this, "Clearing memory of state.");
                WheresDaddyGoingMod.Instance.MemoriesOfDad.RemoveParent(pawn);
            }
            Logger.MessageFormat(this, "Ending state.");
        }

        public override TaggedString GetBeginLetterText()
        {
            string baseLetter = "";
            if (child == null || pawn.gender == Gender.None)
            {
                baseLetter = def.beginLetter.Formatted(pawn.NameShortColored, thingDef.label, pawn.Named("PAWN")).AdjustedFor(pawn).Resolve().CapitalizeFirst();
            }
            else if (pawn.gender == Gender.Male)
            {
                baseLetter = "CM_Wheres_Daddy_Going_MentalState_OutToGetSomeMilk_Letter_Daddy".Translate(thingDef.label, child.NameShortColored);
            }
            else if (pawn.gender == Gender.Female)
            {
                baseLetter = "CM_Wheres_Daddy_Going_MentalState_OutToGetSomeMilk_Letter_Mommy".Translate(thingDef.label, child.NameShortColored);
            }

            return baseLetter;
            //return def.beginLetter.Formatted(pawn.NameShortColored, thingDef.label, pawn.Named("PAWN")).AdjustedFor(pawn).Resolve().CapitalizeFirst();
            //return def.beginLetter.Formatted(pawn.NameShortColored, thingDef.label, pawn.Named("PAWN"), thingDef.Named("TARGET")).AdjustedFor(pawn).Resolve().CapitalizeFirst();
            //return def.beginLetter.Formatted(pawn.NameShortColored, target.NameShortColored, pawn.Named("PAWN"), target.Named("TARGET")).AdjustedFor(pawn).Resolve().CapitalizeFirst();
        }
    }
}