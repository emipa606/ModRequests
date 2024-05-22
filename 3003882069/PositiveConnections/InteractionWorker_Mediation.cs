using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;


namespace DIL_PositiveConnections
{

    public class InteractionWorker_Mediation : InteractionWorker
    {
        private const int NegativeRelationshipThreshold = -10;
        private const int MinSocialSkillRequired = 6;
        private const int MinMediationBonus = 6;
        private const int MaxMediationBonus = 20;
        private const float BaseSelectionWeight = 0.01f;
        //private const float BaseSelectionWeight = 0.9f;

        private PositiveConnectionsModSettings modSettings = Mod_PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {

            if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
            {
                return 0f;
            }

            // If the initiator's social skill is less than the minimum required, this interaction should not occur.
            if (initiator.skills.GetSkill(SkillDefOf.Social).Level < MinSocialSkillRequired||initiator.Faction != recipient.Faction)
            {
                return 0f;
            }

            // Get list of all colony pawns
            IEnumerable<Pawn> colonyPawns = recipient.Map.mapPawns.FreeColonists;

            // Look for potential conflicts that the initiator could mediate.
            foreach (Pawn pawn in colonyPawns)
            {
                if (recipient != pawn && recipient.relations.OpinionOf(pawn) < NegativeRelationshipThreshold)
                {
                    return BaseSelectionWeight * modSettings.BaseInteractionFrequency;
                }
            }

            // If there are no potential conflicts, return a weight of zero.
            return 0f;
        }



        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Initialize output parameters to null
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Locate a suitable conflict for the initiator to mediate
            var conflictPawn = FindConflictPawn(recipient);

            // If no suitable conflict pawn is found, return early.
            if (conflictPawn == null)
            {
                return;
            }

            // Calculate the outcome based on the initiator's social skill
            var mediationBonus = CalculateMediationBonus(initiator);

            // Apply the mediation bonus to the conflict pawn and recipient
            ApplyMediationOutcome(initiator, recipient, conflictPawn, mediationBonus);

            // Log the interaction for debugging purposes
            Log.Message($"[Positive Connections] {initiator.Name.ToStringShort} mediated a conflict between {recipient.Name.ToStringShort} and {conflictPawn.Name.ToStringShort}. Their relationship improved by {mediationBonus}.");
            // Tell the player about it duh
            string mediationMessage = string.Format("{0} mediated a conflict between {1} and {2}. Their relationship improved by {3}.", initiator.LabelShort, recipient.LabelShort, conflictPawn.LabelShort, mediationBonus);

            if(!modSettings.DisableAllMessages)
            {

                Messages.Message(mediationMessage, new LookTargets(new Pawn[] { initiator, recipient, conflictPawn }), MessageTypeDefOf.PositiveEvent);

            }
           


        }



        private Pawn FindConflictPawn(Pawn recipient)
        {
            var colonyPawns = recipient.Map.mapPawns.FreeColonists;

            foreach (var pawn in colonyPawns)
            {
                if (recipient != pawn && recipient.relations.OpinionOf(pawn) < NegativeRelationshipThreshold)
                {
                    return pawn;
                }
            }

            return null;
        }


        private int CalculateMediationBonus(Pawn initiator)
        {
            int socialSkill = initiator.skills.GetSkill(SkillDefOf.Social).Level;
            float randomFactor = Rand.Value;
            int mediationBonus = (int)(MinMediationBonus + (MaxMediationBonus - MinMediationBonus) * randomFactor * socialSkill / 20f);

            return mediationBonus;
        }

        private void ApplyMediationOutcome(Pawn initiator, Pawn recipient, Pawn conflictPawn, int mediationBonus)
        {
            int convertedBonus = (int)Mathf.Round(mediationBonus / (MaxMediationBonus / 2f));
            convertedBonus = Mathf.Clamp(convertedBonus, 0, 2);

            Thought_Memory memoryInitiator = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));
            Thought_Memory memoryRecipient = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));
            Thought_Memory memoryConflictPawn = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SuccessfulMediation"));

            memoryInitiator.SetForcedStage(convertedBonus);
            memoryRecipient.SetForcedStage(convertedBonus);
            memoryConflictPawn.SetForcedStage(convertedBonus);

            initiator.needs.mood.thoughts.memories.TryGainMemory(memoryInitiator);
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryRecipient);
            conflictPawn.needs.mood.thoughts.memories.TryGainMemory(memoryConflictPawn);

            Thought_Memory memoryShared = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOfPositiveConnections.DIL_InMediationWith);
            memoryShared.SetForcedStage(0);
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryShared, conflictPawn);
            conflictPawn.needs.mood.thoughts.memories.TryGainMemory(memoryShared, recipient);
        }
    }






}


