using HarmonyLib;
using RimWorld;
using StuffableCore;
using StuffableCore.SCUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFEPirates;
using VFEPirates.Buildings;

namespace VEFPirateAddOn
{
    [StaticConstructorOnStartup]
    internal class StartUp
    {
        static StartUp()
        {
            if (ModLister.HasActiveModWithName("Vanilla Factions Expanded - Pirates") && SCMod.settings.ArmorSettings.enabled)
            {
                Log.Message("Found Vanilla Factions Expanded - Pirates, applying patch!");
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(i => i.GetType().Equals(typeof(WarcasketDef))))
                {
                    if (def.apparel.tags == null)
                        def.apparel.tags = new List<string>();
                    def.apparel.tags.Add(SCConstants.stuffableTag);
                    def.apparel.tags.Add(SCConstants.StuffableArmor);
                    CostManager.UpdateCost(def, SCMod.settings.ArmorSettings.DefaultStuffCost);
                }
                SCMod.harmony.Patch(AccessTools.Method(typeof(WarcasketProject), "RequiredIngredients"), postfix: new HarmonyMethod(typeof(StartUp), "RequiredIngredients_Harmony_Patch_Postfix"));
                SCMod.harmony.Patch(AccessTools.Method(typeof(WarcasketProject), "ApplyOn"), prefix: new HarmonyMethod(typeof(StartUp), "ApplyOnWarcasket_Harmony_Patch_Prefix"));
                SCMod.harmony.Patch(AccessTools.Method(typeof(Dialog_WarcasketCustomization), "DoPartsSelect"), prefix: new HarmonyMethod(typeof(StartUp), "Dialog_WarcasketCustomization_Harmony_Patch_Prefix"));
            }
        }

        public static bool Dialog_WarcasketCustomization_Harmony_Patch_Prefix(
            Dialog_WarcasketCustomization __instance,
            Rect inRect,
            string label,
            List<WarcasketDef> options,
            WarcasketDef current,
            Action<WarcasketDef> setCurrent,
            ref Color currentColor,
            bool doResearchText = false)
        {
            inRect = inRect.ContractedBy(3f);
            Text.Font = GameFont.Small;
            Widgets.Label(VFECore.UItils.UIUtility.TakeTopPart(ref inRect, 20f), label);
            inRect.y += 2f;
            __instance.DoSelection(VFECore.UItils.UIUtility.TakeTopPart(ref inRect, 24f), options, current, setCurrent);
            if (doResearchText)
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect topPart = VFECore.UItils.UIUtility.TakeTopPart(ref inRect, 25f);
                topPart.width *= 3f;
                Widgets.Label(topPart, "VFEP.ResearchText".Translate().Colorize(ColoredText.SubtleGrayColor));
                Text.Font = GameFont.Small;
            }
            else
                inRect.y += 25f;
            Rect topPart1 = VFECore.UItils.UIUtility.TakeTopPart(ref inRect, 30f);
            Widgets.InfoCardButton(topPart1.x, topPart1.y, (Def)current);
            topPart1.x += 30f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(topPart1, current.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            inRect.y += 5f;
            Widgets.Label(VFECore.UItils.UIUtility.TakeTopPart(ref inRect, 100f), current.shortDescription);
            // ----------------------------------------------------------------------------------
            IEnumerable<ThingDef> source = SCMod.settings.ArmorSettings.GetDefaultStuffListFor();
            if (source.Count() > 0 && ShowMaterialsButton(ref inRect))
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();
                foreach (ThingDef shownItemForIcon in source)
                {
                    ThingDef localStuff = shownItemForIcon;
                    opts.Add(new FloatMenuOption((string)shownItemForIcon.LabelCap, () =>
                    {
                        WarcasketStuffCache.AddStuff(current.defName, localStuff);
                    }, shownItemForIcon));
                }
                Find.WindowStack.Add(new FloatMenu(opts));
            }
            // ----------------------------------------------------------------------------------
            inRect.y += 5f;
            Widgets.Label(VFECore.UItils.UIUtility.TakeTopPart(ref inRect, 40f), "VFEP.ResourceCost".Translate() + " " + GeneralExtensions.Join(current.costList, (cost => cost.LabelCap), ", "));
            Widgets.Label(VFECore.UItils.UIUtility.TakeTopPart(ref inRect, 40f), "Stuff Cost: " + WarcasketStuffCache.GetStuff(current.defName).label + " x" + current.CostStuffCount);
            inRect.y += 5f;
            if (!Widgets.ColorSelector(VFECore.UItils.UIUtility.TakeTopPart(ref inRect, 150f), ref currentColor, StaticStartup.colors, out float _))
                return false;
            AccessTools.Method(__instance.GetType(), "Notify_SettingsChanged").Invoke(__instance, new object[] { });

            return false;
        }
        private static bool ShowMaterialsButton(ref Rect containerRect, bool withBackButtonOffset = false)
        {
            float x = (float)((double)containerRect.x + (double)containerRect.width - 14.0 - 200.0 - 16.0);
            if (withBackButtonOffset)
                x -= 136f;
            return Widgets.ButtonText(new Rect(x, containerRect.y - 50, 200f, 40f), (string)"ShowMaterials".Translate());
        }

        public static bool ApplyOnWarcasket_Harmony_Patch_Prefix(WarcasketProject __instance, ref Pawn pawn)
        {
            WarcasketDef wa = __instance.armorDef;
            Apparel_Warcasket apparelWarcasket1 = ThingMaker.MakeThing(wa, WarcasketStuffCache.GetStuff(wa.defName)) as Apparel_Warcasket;
            apparelWarcasket1.colorApparel = new Color?(__instance.colorArmor);
            pawn.apparel.Wear(apparelWarcasket1, false, true);

            WarcasketDef wh = __instance.helmetDef;
            Apparel_Warcasket apparelWarcasket2 = ThingMaker.MakeThing(wh, WarcasketStuffCache.GetStuff(wh.defName)) as Apparel_Warcasket;
            apparelWarcasket2.colorApparel = new Color?(__instance.colorHelmet);
            pawn.apparel.Wear(apparelWarcasket2, false, true);

            WarcasketDef wsp = __instance.shoulderPadsDef;
            Apparel_Warcasket apparelWarcasket3 = ThingMaker.MakeThing(__instance.shoulderPadsDef, WarcasketStuffCache.GetStuff(wsp.defName)) as Apparel_Warcasket;
            apparelWarcasket3.colorApparel = new Color?(__instance.colorShoulderPads);
            pawn.apparel.Wear(apparelWarcasket3, false, true);

            return false;
        }

        public static void RequiredIngredients_Harmony_Patch_Postfix(WarcasketProject __instance, ref IEnumerable<IngredientCount> __result)
        {
            List<IngredientCount> ingredientCountList;
            ingredientCountList = __result.ToList();
            int defaultCost = SCMod.settings.ArmorSettings.DefaultStuffCost;
            int oldHelmCost = __instance.helmetDef.CostStuffCount;
            int helmCost = oldHelmCost > 0 ? oldHelmCost : defaultCost;
            int oldShoulderPadsCost = __instance.shoulderPadsDef.CostStuffCount;
            int shoulderPadsCost = oldShoulderPadsCost > 0 ? oldShoulderPadsCost : defaultCost;
            int oldArmorCost = __instance.armorDef.CostStuffCount;
            int armorCost = oldArmorCost > 0 ? oldArmorCost : defaultCost;
            List<ThingDefCountClass> armorList = new List<ThingDefCountClass>() {
                new ThingDefCountClass(WarcasketStuffCache.GetStuffWithDefault(__instance.helmetDef), helmCost),
                new ThingDefCountClass(WarcasketStuffCache.GetStuffWithDefault(__instance.shoulderPadsDef), shoulderPadsCost),
                new ThingDefCountClass(WarcasketStuffCache.GetStuffWithDefault(__instance.armorDef), armorCost),
            };

            Dictionary<ThingDef, int> dictionary = new Dictionary<ThingDef, int>();
            foreach (ThingDefCountClass thingDefCountClass in armorList)
            {
                if (dictionary.ContainsKey(thingDefCountClass.thingDef))
                    dictionary[thingDefCountClass.thingDef] += thingDefCountClass.count;
                else
                    dictionary[thingDefCountClass.thingDef] = thingDefCountClass.count;
            }
            foreach (KeyValuePair<ThingDef, int> keyValuePair in dictionary)
                ingredientCountList.Add(new ThingDefCountClass(keyValuePair.Key, keyValuePair.Value).ToIngredientCount());

            __result = ingredientCountList;
        }
    }
}
