using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace DIL_PositiveConnections
{
    public static class PositiveConnectionsUtility
    {
        public static void ChangeFactionRelations(Faction factionA, Faction factionB, int goodwillChange)
        {
            if (goodwillChange == 0)
            {
                goodwillChange = Rand.RangeInclusive(1, 15);
            }
            else
            {
                goodwillChange = Mathf.Clamp(goodwillChange, -100, 100);
            }

            FactionRelation factionRelationA = factionA.RelationWith(factionB);
            FactionRelation factionRelationB = factionB.RelationWith(factionA);

            factionRelationA.baseGoodwill = Mathf.Clamp(factionRelationA.baseGoodwill + goodwillChange, -100, 100);
            factionRelationB.baseGoodwill = factionRelationA.baseGoodwill;

            factionRelationA.CheckKindThresholds(factionA, canSendLetter: false, null, GlobalTargetInfo.Invalid, out _);
            factionRelationB.CheckKindThresholds(factionB, canSendLetter: false, null, GlobalTargetInfo.Invalid, out _);
        }

        public static string GenerateComplimentMessage(Pawn initiator, Pawn recipient)
        {
            string[] compliments = { "social skills", "hard work", "bravery", "intelligence", "grace", "patience" };
            string randomCompliment = compliments.RandomElement();

            return $"{initiator.Name} compliments {recipient.Name} on their {randomCompliment}!";
        }

        private static List<string> giftDescriptions = new List<string>
        {
            "a grass crown",
            "a beautiful feather",
            "a finely crafted comb",
            "a bouquet of wildflowers",
            "an exquisite leaf",
            "a heart-shaped pebble",
            "a small, intricately carved stone",
            "a fresh, juicy fruit",
            "a hand-woven bracelet",
            "an interesting shell",
            "a small trinket made from spare parts",
            "a hand-picked selection of herbs"
        };

        public static string GenerateGiftDescription(Pawn initiator, Pawn recipient)
        {
            int index = Rand.Range(0, giftDescriptions.Count);
            string gift = giftDescriptions[index];
            string message = string.Format("{0} gave {1} {2} as a gift.", initiator.LabelShort, recipient.LabelShort, gift);

            return message;
        }

        public static string GenerateSharedPassionMessage(Pawn pawnA, Pawn pawnB, SkillRecord sharedPassion)
        {
            string passionSubject = sharedPassion.def.skillLabel;
            string actionPhrase;

            switch (passionSubject)
            {
                case "animals":
                    actionPhrase = "discussing animal husbandry";
                    break;
                case "artistic":
                    actionPhrase = "talking about art and creativity";
                    break;
                case "construction":
                    actionPhrase = "discussing architecture";
                    break;
                case "cooking":
                    actionPhrase = "talking about cooking";
                    break;
                case "crafting":
                    actionPhrase = "discussing crafting techniques";
                    break;
                case "growing":
                    actionPhrase = "talking about agricultural technique";
                    break;
                case "mining":
                    actionPhrase = "discussing mining and minerals";
                    break;
                case "shooting":
                    actionPhrase = "discussing the ins and outs of marksmanship";
                    break;
                case "melee":
                    actionPhrase = "talking about melee combat";
                    break;
                case "intellectual":
                    actionPhrase = "having an intellectual conversation";
                    break;
                case "social":
                    actionPhrase = "socializing";
                    break;
                default:
                    actionPhrase = "sharing their passion";
                    break;
            }

            return $"{pawnA.Name} and {pawnB.Name} enjoyed {actionPhrase} together.";
        }


    }
}

