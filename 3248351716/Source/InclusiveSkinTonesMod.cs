using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SkinTones
{
    public class InclusiveSkinTonesMod : Mod
    {
        
        public InclusiveSkinTonesMod(ModContentPack content) : base(content)
        {
            //Static but needed to ExposeData()
            GetSettings<SkinTonesSettings>();

            var harmony = new Harmony("h2forge.InclusiveSkinTones");
            harmony.PatchAll();
        }

        //GUI box
        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Red Skin Tone Shader (requires quit to main menu)", ref SkinTonesSettings.ApplySkinShader, "Toggle on/off the red shadow skin shader for light skin. (Effect is subtle and requires only a scene restart for existing pawns. No need for a full restart.)");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        
        //Show up in list of settings.
        public override string SettingsCategory()
        {
            return "InclusiveSkinTones";
        }
    }

    [HarmonyPatch(typeof(ShaderUtility), nameof(ShaderUtility.GetSkinShader))]
    class Patch1
    {

        static void Postfix(Pawn pawn, ref Shader __result)
        {
            if  (!SkinTonesSettings.ApplySkinShader|| PawnSkinColors.IsDarkSkin(pawn.story.SkinColor))
            {
                __result = ShaderDatabase.Cutout;
            }
        }
    }
}