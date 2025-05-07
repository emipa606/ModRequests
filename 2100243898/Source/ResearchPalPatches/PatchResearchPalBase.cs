using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;

namespace DTechprinting
{
    [StaticConstructorOnStartup]
    public static class PatchResearchPalBase
    {

        public static Type rn;

        static PatchResearchPalBase()
        {

            try
            {
                ((Action)(() =>
                {
                    MethodInfo target1;
                    var harmony = new Harmony("io.github.dametri.techprinting");
                    if (LoadedModManager.RunningModsListForReading.Any(x => x.Name.ToLower() == "researchpal"))
                    {
                        Log.Message("DTechprinting: Research Pal running, attempting to patch");

                        rn = AccessTools.TypeByName("ResearchPal.ResearchNode");
                    }
                    else
                        return;

                    target1 = AccessTools.Method(rn, "Draw");
                    var invoke1 = AccessTools.Method(typeof(Patch_ResearchNodeDraw_Transpiler), "Transpiler");
                    if (target1 != null && invoke1 != null)
                        harmony.Patch(target1, transpiler: new HarmonyMethod(invoke1));

                }))();
            }
            catch (TypeLoadException ex) { Log.Message(ex.ToString()); }
        }

        public static void DrawLabel(object obj)
        {
            var node = obj as ResearchPal.ResearchNode;
            if (node == null)
                return;
            if (node.Research.techprintCount == 0 || node.Research.TechprintRequirementMet)
            {
                Widgets.Label(node.CostLabelRect, node.Research.CostApparent.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Absolute));
                GUI.DrawTexture(node.CostIconRect, (!node.Completed && !node.Available) ? ResearchPal.Assets.Lock : ResearchPal.Assets.ResearchIcon, ScaleMode.ScaleToFit);
            }
            else
            {
                int printsNeeded = node.Research.techprintCount;
                int printsAcquired = Find.ResearchManager.GetTechprints(node.Research);
                string label = printsAcquired + " / " + printsNeeded;
                Text.Font = GameFont.Tiny;
                Widgets.Label(node.CostLabelRect, label);
                Texture2D shards = ContentFinder<Texture2D>.Get("Things/Item/Special/Techshard/Techshard_c", true);
                GUI.color = Color.white;
                GUI.DrawTexture(node.CostIconRect, shards, ScaleMode.ScaleToFit);
                GUI.color = Color.grey;
            }
        }
    }
}
