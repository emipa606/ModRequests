using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;


namespace TakeOffIndoor
{
    public static class Harmony_FationColors
    {
        public static void PatchFactionColors(Harmony harmony)
        {
            MethodInfo original = AccessTools.Method(ModUtil.FactionColors_HP, "ResolveApparelGraphicsOriginal");
            HarmonyMethod transpiler;
            debug.WriteLine("Got FactionColors Method.");
            if (ModUtil.ExistAppearanceClothes)
            {
                transpiler = Util.HM(typeof(Harmony_WornApparel_Trans), "APTranspiler");
                harmony.Patch(original, null, null, transpiler);

                Type type = Util.LoadTypeInAssembly("FactionColors", "FactionColors.ApparelDetailDrawer");

                original = AccessTools.PropertyGetter(type, "DetailGraphic");
                transpiler = Util.HM(typeof(Harmony_WornApparel_Trans), "BodyTypeTranspiler");
                harmony.Patch(original, null, null, transpiler);

                type = Util.LoadTypeInAssembly("FactionColors", "FactionColors.CompPauldronDrawer");

                original = AccessTools.PropertyGetter(type, "PauldronGraphic");
                transpiler = Util.HM(typeof(Harmony_WornApparel_Trans), "BodyTypeTranspiler");
                harmony.Patch(original, null, null, transpiler);

                type = Util.LoadTypeInAssembly("FactionColors", "FactionColors.Backpack_Accessory");

                original = AccessTools.Method(type, "Tick");
                transpiler = Util.HM(typeof(Harmony_WornApparel_Trans), "BodyTypeTranspiler");
                harmony.Patch(original, null, null, transpiler);

            }
            else
            {
                transpiler = Util.HM(typeof(Harmony_WornApparel_Trans), "RAG_Transpiler");
                debug.WriteLine("Got Transpiler Method.");
                harmony.Patch(original, null, null, transpiler);
            }
            Log.Message("TakeOffCoat:FactionColors Patched.");
            debug.WriteLine("FactionColors Patched.");
        }

    }
}
