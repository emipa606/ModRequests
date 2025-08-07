using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Stranger_Danger.CustomMethods;

namespace Stranger_Danger
{
    //shows bio tab disabled work
    [HarmonyPatch]
    static class BioDisabledWorkPatch
    {
        static MethodInfo Pawn_CombinedDisabledWorkTags = AccessTools.PropertyGetter(typeof(Pawn), "CombinedDisabledWorkTags");

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CharacterCardUtility), "DoLeftSection")]
        static IEnumerable<CodeInstruction> DrawCharacterCardPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == Pawn_CombinedDisabledWorkTags)
                {
                    codes[i].operand = OpCodes.Call;
                    codes[i].operand = typeof(BioDisabledWorkPatch).GetMethod("DisabledWorkTags_Hidden");
                    break;
                }
            }

            return codes;
        }

        public static WorkTags DisabledWorkTags_Hidden(Pawn pawn)
        {
            bool hideStory = ShouldHide(pawn, HideType.Story);
            bool hideTraits = ShouldHide(pawn, HideType.Traits);

            if (!hideStory && !hideTraits)
                return pawn.CombinedDisabledWorkTags;

            return CombinedDisabledWorkTags_sd(pawn, hideStory, hideTraits);
        }

        static WorkTags CombinedDisabledWorkTags_sd(Pawn pawn, bool hideStory, bool hideTraits)
        {
            //Pawn.CombinedDisabledWorkTags
            WorkTags workTags = (pawn.story != null) ? DisabledWorkTagsBackstoryTraitsAndGenes_sd(pawn, hideStory, hideTraits) : WorkTags.None;

            if (pawn.royalty != null)
                foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
                    if (item.conceited)
                        workTags |= item.def.disabledWorkTags;

            if (ModsConfig.IdeologyActive && pawn.Ideo != null)
            {
                Precept_Role role = pawn.Ideo.GetRole(pawn);
                if (role != null)
                    workTags |= role.def.roleDisabledWorkTags;
            }

            if (pawn.health != null && pawn.health.hediffSet != null)
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    HediffStage curStage = hediff.CurStage;
                    if (curStage != null)
                        workTags |= curStage.disabledWorkTags;
                }

            foreach (QuestPart_WorkDisabled item2 in QuestUtility.GetWorkDisabledQuestPart(pawn))
                workTags |= item2.disabledWorkTags;

            return workTags;
        }

        public static WorkTags DisabledWorkTagsBackstoryTraitsAndGenes_sd(Pawn pawn, bool hideStory, bool hideTraits)
        {
            //Pawn_StoryTracker.DisabledWorkTagsBackstoryTraitsAndGenes
            WorkTags workTags = DisabledWorkTagsBackstoryAndTraits_sd(pawn.story, hideStory, hideTraits);
            if (pawn.genes != null)
            {
                workTags |= pawn.genes.DisabledWorkTags;
            }
            return workTags;
        }

        public static WorkTags DisabledWorkTagsBackstoryAndTraits_sd(Pawn_StoryTracker story, bool hideStory, bool hideTraits)
        {
            //Pawn_StoryTracker.DisabledWorkTagsBackstoryAndTraits
            WorkTags workTags = WorkTags.None;
            if (story != null)
            {
                if (!hideStory)
                {
                    if (story.Childhood != null)
                        workTags |= story.Childhood.workDisables;
                    if (story.Adulthood != null)
                        workTags |= story.Adulthood.workDisables;
                }

                if (!hideTraits)
                    for (int i = 0; i < story.traits.allTraits.Count; i++)
                        if (!story.traits.allTraits[i].Suppressed)
                            workTags |= story.traits.allTraits[i].def.disabledWorkTags;
            }
            return workTags;
        }
    }
}
