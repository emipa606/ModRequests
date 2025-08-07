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
    //hides bio tab traits
    [HarmonyPatch]
    static class BioTraitsPatch
    {
        static FieldInfo TraitSet_allTraits = AccessTools.Field(typeof(TraitSet), "allTraits");

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CharacterCardUtility), "DoLeftSection")]
        static IEnumerable<CodeInstruction> DrawCharacterCardPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == TraitSet_allTraits)
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = typeof(BioTraitsPatch).GetMethod("allTraits_sd");
                    break;
                }
            }

            return codes;
        }

        public static List<Trait> allTraits_sd(TraitSet traits)
        {
            Pawn pawn = Traverse.Create(traits).Field("pawn").GetValue() as Pawn;
            return ShouldHide(pawn, HideType.Traits) ? null : traits.allTraits;
        }
    }
}
