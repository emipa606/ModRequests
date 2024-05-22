using System;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_GiveComfort : InteractionWorker
    {
        private const int MinMoodForComfort = 45;
        private const int MinSocialSkillRequired = 6;
        private float BaseSelectionWeight = 0.01f;

        private PositiveConnectionsModSettings modSettings = Mod_PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
            {
                return 0f;
            }

            if (initiator.skills.GetSkill(SkillDefOf.Social).Level < MinSocialSkillRequired
            && !initiator.story.traits.HasTrait(TraitDefOf.Kind))
            {
                return 0f;
            }

            // If the recipient's mood is not low enough, this interaction should not occur.
            if (recipient.needs.mood.CurLevelPercentage * 100 >= MinMoodForComfort)
            {
                return 0f;
            }

            // really rare to cross faction lines
            if (recipient.Faction != initiator.Faction)
            {
                BaseSelectionWeight *= 0.05f;
            }

            // If the conditions are met, return the base selection weight.
            return BaseSelectionWeight * modSettings.BaseInteractionFrequency;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Initialize output parameters to null
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Create a memory of the comforting interaction
            Thought_Memory memoryRecipient = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_ComfortReceived"));

            // Add the memory to the recipient
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryRecipient, initiator);

            // Log the interaction for debugging purposes
            Log.Message($"[Positive Connections] {initiator.Name.ToStringShort} comforted {recipient.Name.ToStringShort}.");

            // Notify the player
            string comfortMessage = string.Format("{0} comforted {1}.", initiator.LabelShort, recipient.LabelShort);

            if (!modSettings.DisableAllMessages)
            {
                Messages.Message(comfortMessage, new LookTargets(new Pawn[] { initiator, recipient }), MessageTypeDefOf.PositiveEvent);
            }
        }
    }

}

