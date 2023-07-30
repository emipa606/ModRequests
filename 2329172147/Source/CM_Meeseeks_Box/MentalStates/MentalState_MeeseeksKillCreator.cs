using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CM_Meeseeks_Box
{
    public class MentalState_MeeseeksKillCreator : MentalState
    {
        private Pawn target;

        private Pawn grandCreatorMeeseeks;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref target, "target");
            Scribe_References.Look(ref target, "grandCreatorMeeseeks");
        }

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void PreStart()
        {
            base.PreStart();

            CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();
            Pawn creator = memory.Creator;

            CompMeeseeksMemory creatorMemory = creator.GetComp<CompMeeseeksMemory>();

            grandCreatorMeeseeks = pawn;

            // If our creator is a Meeseeks, try his creator and so on until we get a target, also remembering who the highest level Meeseeks creator is
            while (creator != null && creatorMemory != null)
            {
                // Well, he's the one that roped me into this
                grandCreatorMeeseeks = creator;

                creator = creatorMemory.Creator;
                if (creator == null)
                    creatorMemory = null;
                else
                    creatorMemory = creator.GetComp<CompMeeseeksMemory>();
            }

            // Probably impossible as this should be checked by the mental state worker before we get here
            if (creator == null)
            {
                Logger.WarningFormat(this, "Meeseeks had KillCreator mental break but could not find non-Meeseeks creator.");
                target = pawn;
                return;
            }

            target = creator;
        }

        public override void PostStart(string reason)
        {
            base.PostStart(reason);

            Logger.MessageFormat(this, "Meeseeks attempting to incite other Meeseeks to join in killing the creator.");

            Faction meeseeksFaction = FactionUtility.DefaultFactionFrom(MeeseeksDefOf.CM_Meeseeks_Box_Faction_Hostile_Meeseeks);
            if (meeseeksFaction == null)
                meeseeksFaction = Faction.OfAncientsHostile;

            Lord lord = LordMaker.MakeNewLord(meeseeksFaction, new LordJob_MeeseeksKillCreator(target), pawn.MapHeld);

            // Now add all the meeseeks in this tree to the list
            List<Pawn> foundMeeseeks = new List<Pawn> { grandCreatorMeeseeks };
            List<Pawn> accumulatedMeeseeks = new List<Pawn>();

            while (foundMeeseeks.Count > 0)
            {
                foreach (Pawn meeseeks in foundMeeseeks)
                {
                    meeseeks.SetFaction(meeseeksFaction);

                    CompMeeseeksMemory meeseeksMemory = meeseeks.GetComp<CompMeeseeksMemory>();

                    // Set job memory, wont be using it in this lord, but set it anyway for debugging purposes
                    Job newJob = JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_Kill, target);
                    newJob.workGiverDef = MeeseeksDefOf.CM_Meeseeks_Box_WorkGiver_Kill;
                    meeseeksMemory.ForceNewJob(newJob, target);

                    // Put in new lrd
                    meeseeks.GetLord()?.Notify_PawnLost(meeseeks, PawnLostCondition.ChangedFaction);
                    lord.AddPawn(meeseeks);

                    accumulatedMeeseeks.AddRange(meeseeks.GetComp<CompMeeseeksMemory>().CreatedMeeseeks);
                }

                foundMeeseeks = new List<Pawn>(accumulatedMeeseeks);
                accumulatedMeeseeks.Clear();
            }

            CompMeeseeksMemory grandCreatorMeeseeksMemory = grandCreatorMeeseeks.GetComp<CompMeeseeksMemory>();
            MeeseeksUtility.PlayCriticalBreakSound(grandCreatorMeeseeks, grandCreatorMeeseeksMemory.Voice);

            // Since we changed the Meeseeks' faction, we need to make our own letter
            string letterText = this.GetBeginLetterText();
            if (!letterText.NullOrEmpty())
            {
                string letterLabel = (def.beginLetterLabel ?? ((string)def.LabelCap)).CapitalizeFirst();
                if (!reason.NullOrEmpty())
                {
                    letterText = letterText + "\n\n" + reason;
                }
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, def.beginLetterDef, grandCreatorMeeseeks);
            }
        }

        public override string GetBeginLetterText()
        {
            if (target == null)
            {
                Log.Error("No target. This should have been checked in this mental state's worker.");
                return "";
            }

            return def.beginLetter.Formatted(pawn.NameShortColored, target.NameShortColored, pawn.Named("PAWN"), target.Named("TARGET")).AdjustedFor(pawn).Resolve().CapitalizeFirst();
        }

        public override void MentalStateTick()
        {
            // This doesn't even get called as long as Meeseeks is put in another faction
            RecoverFromState();
        }
    }
}