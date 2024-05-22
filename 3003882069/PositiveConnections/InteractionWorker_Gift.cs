using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_Gift : InteractionWorker
    {

        PositiveConnectionsModSettings modSettings = Mod_PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();
        private const float BaseSelectionWeight = 0.0050f;
       

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
            {
                return 0f;
            }

            if (initiator.relations.OpinionOf(recipient) < -5 || recipient.relations.OpinionOf(initiator) < -5)
            {
                return 0f;
            }

           
            float baseWeight = initiator.needs.mood.CurLevel * BaseSelectionWeight * modSettings.BaseInteractionFrequency;


            if (recipient.story.traits.HasTrait(TraitDefOf.Beauty))
            {
                baseWeight *= 2;  // double the base weight if the recipient is Beautiful
            }

            if (initiator.Faction == recipient.Faction)
            {
                return baseWeight;
            }
            else
            {
                return baseWeight * 0.2f; // 1/5 as likely
            }
        }



        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            string giftDescription = PositiveConnectionsUtility.GenerateGiftDescription(initiator, recipient);

            if(!modSettings.DisableAllMessages) {

                Messages.Message(giftDescription, recipient, MessageTypeDefOf.PositiveEvent);

            }

            Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_ReceivedGift"));
            memory.moodPowerFactor = 1f;
            recipient.needs.mood.thoughts.memories.TryGainMemory(memory,initiator);

            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Update relationship between initiator and recipient
            Faction factionA = initiator.Faction;
            Faction factionB = recipient.Faction;

            if (factionA != null && factionB != null && factionA != factionB)
            {
                PositiveConnectionsUtility.ChangeFactionRelations(factionA, factionB, 10);
            }
        }
    }
}
