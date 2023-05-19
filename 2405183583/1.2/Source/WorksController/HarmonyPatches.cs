using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace WorksController
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        private readonly static HashSet<Type> TypeToExcludes;

        static HarmonyPatches()
        {
            var harmony = new HarmonyLib.Harmony("miyamiya.workscontroller.latest");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            TypeToExcludes = GetExcludeTypeHashSet();
#if DEBUG
            ListupAllSkillRelevantedWorkGiver(harmony); //!!!!!!DEBUG!!!!!!
#endif
            ExecutePatches(harmony);
        }
#if DEBUG
        #region test!!!
        // !!!!!!debug!!!!!!
        private static void ListupAllSkillRelevantedWorkGiver(Harmony harmony)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("implementsShouldSkip\tdefName\tType\tdeclaringType\tpatchTargetClass\tworkGiverName\tworkTypeName\tRelevantSkills");
            Type workGiver = AccessTools.TypeByName("RimWorld.WorkGiver");
            //foreach (WorkGiverDef workGiverDef in DefDatabase<WorkGiverDef>.AllDefs.Where(x => x.workType?.relevantSkills?.Any() ?? false).OrderBy(x => x.giverClass.GetHashCode()).Distinct())
            //foreach (WorkGiverDef workGiverDef in DefDatabase<WorkGiverDef>.AllDefs.Where(x => x.workType?.relevantSkills?.Any() ?? false).OrderBy(x => x.giverClass.GetHashCode()))
            foreach (WorkGiverDef workGiverDef in DefDatabase<WorkGiverDef>.AllDefs.OrderBy(x => x.giverClass.GetHashCode()))
            {
                WC_DataContext.AddNewData(workGiverDef);
                Type giverClass = workGiverDef.giverClass;
                List<SkillDef> relevantSkills = workGiverDef.workType.relevantSkills;
                MethodInfo method = AccessTools.Method(giverClass, "ShouldSkip");
                Type declaringType = method.DeclaringType;
                bool implementsShouldSkip = giverClass == declaringType || declaringType != workGiver;
                Type patchTargetClass = workGiver;
                if (giverClass == declaringType)
                {
                    patchTargetClass = giverClass;
                }
                else if (declaringType != workGiver)
                {
                    patchTargetClass = declaringType;
                }
                sb.AppendLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", implementsShouldSkip, workGiverDef.defName, giverClass.FullName, declaringType.FullName, patchTargetClass, workGiverDef.LabelCap, workGiverDef.workType.LabelCap, relevantSkills != null && relevantSkills.Any() ? string.Join("|", relevantSkills) : "関連スキル無し"));
            }
            Log.Message("======== ListupAllSkillRelevantedWorkGiver  END  ========");

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("DEBUG_WORK_GIVER_LISTUP.txt", false, System.Text.Encoding.Default))
            {
                sw.Write(sb.ToString());
            }
        }
        // !!!!!!debug!!!!!!
        #endregion
#endif

        static HashSet<Type> GetExcludeTypeHashSet()
        {
            List<Type> list = new List<Type>();
            foreach (string excludeTypeString in DefDatabase<ExcludeTypeDef>.AllDefs.SelectMany(x => x.excludeTypes).Distinct())
            {
                Type excludeType = AccessTools.TypeByName(excludeTypeString);
                if (excludeType != null)
                {
                    list.Add(excludeType);
                }
            }
            return list.ToHashSet();
        }

        [StaticConstructorOnStartup]
        [HarmonyPatch(typeof(BackCompatibility))]
        public class BackCompatibility_Patch
        {
            [HarmonyPatch("PreLoadSavegame")]
            [HarmonyPatch(new Type[] { typeof(string) })]
            [HarmonyPostfix]
            public static void PreLoadSavegame_Postfix(string loadingVersion)
            {
                //Log.Message(String.Format("ロード実行中です。WCセーブデータをクリアします。"));
                WC_DataContext.ClearSaveData();
            }
        }

        [StaticConstructorOnStartup]
        [HarmonyPatch(typeof(Game))]
        public class Game_Patch
        {
            [HarmonyPatch("LoadGame")]
            [HarmonyPostfix]
            public static void LoadGame_Postfix()
            {
                //Log.Message(String.Format("ロード完了しました。WCセーブデータを再構築します。savedata-string:\n{0}", WC_DataContext.m_SavedataString));
                WC_DataContext.m_WCOpend = false;
                WC_DataContext.RestoreBySaveData();
            }

            [HarmonyPatch("InitNewGame")]
            [HarmonyPostfix]
            public static void InitNewGame_Postfix()
            {
                //Log.Message(String.Format("新規ゲームが開始しました。WCセーブデータをクリアします。"));
                WC_DataContext.m_WCOpend = false;
                WC_DataContext.ClearSaveData();
            }

        }

        private static void ExecutePatches(Harmony harmony)
        {
            HashSet<Type> patchedClasses = new HashSet<Type>();
            Type workGiver = typeof(WorkGiver);
            harmony.Patch(AccessTools.Method(workGiver, nameof(WorkGiver.ShouldSkip)), null, null, new HarmonyMethod(typeof(WorkGiver_Patches), nameof(WorkGiver_Patches.WorkGiver_ShouldSkip_Transpiler), null) { methodType = MethodType.Normal, priority = Priority.VeryLow }, null);
            patchedClasses.Add(workGiver);
            foreach (WorkGiverDef workGiverDef in DefDatabase<WorkGiverDef>.AllDefs.Where(x => x.workType?.relevantSkills?.Any() ?? false).OrderBy(x => x.giverClass.GetHashCode()))
            {
                Type giverClass = workGiverDef.giverClass;
                if (TypeToExcludes.Contains(giverClass))
                {
                    //Log.Message(String.Format("====@@@ exclude!!!: {0}({1})====", workGiverDef.defName, giverClass.FullName));
                    continue;
                }
                WC_DataContext.AddNewData(workGiverDef);
                List<SkillDef> relevantSkills = workGiverDef.workType.relevantSkills;
                MethodInfo method = AccessTools.Method(giverClass, nameof(WorkGiver.ShouldSkip));
                Type declaringType = method.DeclaringType;
                bool implementsShouldSkip = giverClass == declaringType || declaringType != workGiver;
                Type patchTargetClass = workGiver;
                if (giverClass == declaringType)
                {
                    patchTargetClass = giverClass;
                }
                else if (declaringType != workGiver)
                {
                    patchTargetClass = declaringType;
                }
                //Log.Message(String.Format("====register: {0}({1})====", workGiverDef.defName, patchTargetClass.FullName));
                bool patchable = !patchedClasses.Contains(patchTargetClass);
                if (!patchable)
                {
                    continue;
                }

                harmony.Patch(AccessTools.Method(patchTargetClass, nameof(WorkGiver.ShouldSkip)), null, null, new HarmonyMethod(typeof(WorkGiver_Patches), nameof(WorkGiver_Patches.WorkGiver_ShouldSkip_Transpiler), null) { methodType = MethodType.Normal, priority = Priority.VeryLow }, null);
                patchedClasses.Add(patchTargetClass);
            }
        }

    }

    [StaticConstructorOnStartup]
    public class WorkGiver_Patches
    {
        public static IEnumerable<CodeInstruction> WorkGiver_ShouldSkip_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            Label labelNormalStart = generator.DefineLabel();
            instructions.First().labels.Add(labelNormalStart);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return CodeInstruction.LoadField(original.DeclaringType, nameof(WorkGiver.def));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return CodeInstruction.Call(typeof(WorksControllerUtil), nameof(WorksControllerUtil.WorkGiver_ShouldSkip), new Type[] { typeof(WorkGiverDef), typeof(Pawn), typeof(bool) });
            yield return new CodeInstruction(OpCodes.Brfalse_S, labelNormalStart);
            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
            yield return new CodeInstruction(OpCodes.Ret);

            foreach (CodeInstruction ci in instructions)
            {
                yield return ci;
            }
        }
    }
}
