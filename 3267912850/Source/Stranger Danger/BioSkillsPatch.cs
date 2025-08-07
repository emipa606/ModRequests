using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Stranger_Danger.CustomMethods;

namespace Stranger_Danger
{
    [HarmonyPatch]
    static class BioSkillsPatch
    {
        
        
        
        
        
        static MethodInfo SkillRecord_TotallyDisabled = AccessTools.PropertyGetter(typeof(SkillRecord), "TotallyDisabled");
        static MethodInfo SkillRecord_PermanentlyDisabled = AccessTools.PropertyGetter(typeof(SkillRecord), "PermanentlyDisabled");
        static FieldInfo SkillRecord_passion = AccessTools.Field(typeof(SkillRecord), "passion");

        //edits skill mouseover descriptions
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(SkillUI), "GetSkillDescription")]
        static IEnumerable<CodeInstruction> GetSkillDescriptionPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == SkillRecord_passion)
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = typeof(BioSkillsPatch).GetMethod("passion_sd");
                }

                if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == SkillRecord_TotallyDisabled)
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = typeof(BioSkillsPatch).GetMethod("TotallyDisabled_sd");
                }

                if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == SkillRecord_PermanentlyDisabled)
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = typeof(BioSkillsPatch).GetMethod("PermanentlyDisabled_sd");
                }
            }

            return codes;
        }

        public static int GetLevelProxy(SkillRecord record, bool aptitudes = true) => PermanentlyDisabled_sd(record) ? 0 : record.GetLevel(aptitudes);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillRecord), "GetLevelForUI")]
        static bool GetLevelForUIPatch(SkillRecord __instance, ref int __result)
        {
            if (PermanentlyDisabled_sd(__instance))
            {
                __result = 0;
                return false;
            }

            return true;
        }

        //replaces skill mouseover description
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillUI), "GetSkillDescription")]
        static bool WriteBlank(SkillRecord sk, ref string __result)
        {
            var pawn = sk.pawn;
            if (pawn == null || !ShouldHide(pawn, HideType.Skills))
                return true;

            StringBuilder stringBuilder = new StringBuilder();
            TaggedString taggedString = sk.def.LabelCap.AsTipTitle();
            //taggedString += " (" + "DisabledLower".Translate() + ")";
            taggedString += " (" + "hidden" + ")";
            stringBuilder.AppendLineTagged(taggedString);
            stringBuilder.AppendLineTagged(sk.def.description.Colorize(ColoredText.SubtleGrayColor)).AppendLine();
            __result = stringBuilder.ToString().TrimEndNewlines();

            return false;
        }

        //hides bio tab skills and passions
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(SkillUI), "DrawSkill")]
        [HarmonyPatch(new Type[] { typeof(SkillRecord), typeof(Rect), typeof(SkillUI.SkillDrawMode), typeof(string) })]
        static IEnumerable<CodeInstruction> DrawSkillPatch(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == SkillRecord_TotallyDisabled)
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = typeof(BioSkillsPatch).GetMethod("TotallyDisabled_sd");
                }

                if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == SkillRecord_passion)
                {
                    codes[i].opcode = OpCodes.Call; // from callvirt? // its static why virt
                    codes[i].operand = typeof(BioSkillsPatch).GetMethod("passion_sd");
                }
            }

            return codes;
        }

        public static string SkillDescriptionString(SkillRecord skill)
        {
            Pawn pawn = Traverse.Create(skill).Field("pawn").GetValue() as Pawn;
            if (ShouldHide(pawn, HideType.Skills))
                return "hidden";

            return "DisabledLower";
        }

        public static Passion passion_sd(SkillRecord skill)
        {
            if (skill.TotallyDisabled) return Passion.None;
            return ShouldHide(skill.pawn, HideType.Passion) ? Passion.None : skill.passion;
        }

        public static bool TotallyDisabled_sd(SkillRecord skill)
        {
            return TotallyDisabled_sd2(skill, skill.pawn);
        }

        public static bool TotallyDisabled_sd2(SkillRecord skill, Pawn pawn)
        {
            if (pawn == null || ShouldHide(pawn, HideType.Skills))
                return true;

            bool hideStory = ShouldHide(pawn, HideType.Story);
            bool hideTraits = ShouldHide(pawn, HideType.Traits);

            if (!hideStory && !hideTraits)
                return skill.TotallyDisabled;

            //SkillRecord.CalculateTotallyDisabled()
            return skill.def.IsDisabled(BioDisabledWorkPatch.DisabledWorkTagsBackstoryTraitsAndGenes_sd(pawn, hideStory, hideTraits), GetDisabledWorkTypes_sd(pawn, hideStory, hideTraits));
        }

        public static bool PermanentlyDisabled_sd(SkillRecord skill)
        {
            var pawn = skill.pawn;
            if (pawn == null || ShouldHide(pawn, HideType.Skills))
                return true;

            bool hideStory = ShouldHide(pawn, HideType.Story);
            bool hideTraits = ShouldHide(pawn, HideType.Traits);

            if (!hideStory && !hideTraits)
                return skill.TotallyDisabled;

            //SkillRecord.CalculatePermanentlyDisabled()
            return skill.def.IsDisabled(BioDisabledWorkPatch.DisabledWorkTagsBackstoryAndTraits_sd(pawn.story, hideStory, hideTraits), GetDisabledWorkTypes_sd(pawn, hideStory, hideTraits, permanentOnly: true));
        }

        static List<WorkTypeDef> GetDisabledWorkTypes_sd(Pawn pawn, bool hideStory, bool hideTraits, bool permanentOnly = false)
        {
            //Pawn.GetDisabledWorkTypes
            
            List<WorkTypeDef> list = new List<WorkTypeDef>();

            if (pawn.story != null && !pawn.IsSlave)
            {
                if (!hideStory)
                    foreach (BackstoryDef allBackstory in pawn.story.AllBackstories)
                    foreach (WorkTypeDef disabledWorkType in allBackstory.DisabledWorkTypes)
                        if (!list.Contains(disabledWorkType))
                            list.Add(disabledWorkType);

                if (!hideTraits)
                    for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
                        if (!pawn.story.traits.allTraits[i].Suppressed)
                            foreach (WorkTypeDef disabledWorkType2 in pawn.story.traits.allTraits[i].GetDisabledWorkTypes())
                                if (!list.Contains(disabledWorkType2))
                                    list.Add(disabledWorkType2);
            }

            if (ModsConfig.BiotechActive && pawn.IsColonyMech)
            {
                List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                for (int j = 0; j < allDefsListForReading.Count; j++)
                {
                    if (!pawn.RaceProps.mechEnabledWorkTypes.Contains(allDefsListForReading[j]) && !list.Contains(allDefsListForReading[j]))
                    {
                        list.Add(allDefsListForReading[j]);
                    }
                }
            }

            if (!permanentOnly)
            {
                if (pawn.royalty != null && !pawn.IsSlave)
                    foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
                        if (item.conceited)
                            foreach (WorkTypeDef disabledWorkType3 in item.def.DisabledWorkTypes)
                                if (!list.Contains(disabledWorkType3))
                                    list.Add(disabledWorkType3); //*/

                if (ModsConfig.IdeologyActive && pawn.Ideo != null)
                {
                    Precept_Role role = pawn.Ideo.GetRole(pawn);
                    if (role != null)
                        foreach (WorkTypeDef disabledWorkType4 in role.DisabledWorkTypes)
                            if (!list.Contains(disabledWorkType4))
                                list.Add(disabledWorkType4);
                }

                if (ModsConfig.BiotechActive && pawn.genes != null)
                {
                    foreach (Gene item2 in pawn.genes.GenesListForReading)
                    {
                        foreach (WorkTypeDef disabledWorkType5 in item2.DisabledWorkTypes)
                        {
                            if (!list.Contains(disabledWorkType5))
                            {
                                list.Add(disabledWorkType5);
                            }
                        }
                    }
                }

                foreach (QuestPart_WorkDisabled item3 in QuestUtility.GetWorkDisabledQuestPart(pawn))
                foreach (WorkTypeDef disabledWorkType6 in item3.DisabledWorkTypes)
                    if (!list.Contains(disabledWorkType6))
                        list.Add(disabledWorkType6);

                if (pawn.guest != null)
                    foreach (WorkTypeDef disabledWorkType6 in pawn.guest.GetDisabledWorkTypes())
                        if (!list.Contains(disabledWorkType6))
                            list.Add(disabledWorkType6);

                for (int k = 0; k < pawn.RaceProps.lifeStageWorkSettings.Count; k++)
                {
                    LifeStageWorkSettings lifeStageWorkSettings = pawn.RaceProps.lifeStageWorkSettings[k];
                    if (lifeStageWorkSettings.IsDisabled(pawn) && !list.Contains(lifeStageWorkSettings.workType))
                    {
                        list.Add(lifeStageWorkSettings.workType);
                    }
                }
            }

            return list;
        }
    }
}
