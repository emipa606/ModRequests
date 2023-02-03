using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;
using AnimalRangeAttack;

namespace MonsterMash
{
    [HarmonyPatch(typeof(AnimalRangeAttack.ARA_FightAI_Patch), "Prefix", null)]
    class harmonyPatches_ARA_FightAI_Patch
    {
        [HarmonyBefore("com.github.rimworld.mod.AnimalRangeAttack")]
        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            int startIndex = -1;

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldind_Ref)
                {
                    startIndex = i; //Provides the index of the callvirt before the call I need.
                    for (int j = startIndex; j < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Ldfld && codes[j + 1].opcode == OpCodes.Callvirt)
                        {
                            codes[j].opcode = OpCodes.Callvirt;
                            codes[j].operand = "instance class ['mscorlib'] System.Collections.Generic.List`1 <class ['Assembly-CSharp'] Verse.Verb>]GetHediffVerbs_forAnimal";
                            codes[j + 1].opcode = OpCodes.Nop;
                            return codes.AsEnumerable();
                        }
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }
}
