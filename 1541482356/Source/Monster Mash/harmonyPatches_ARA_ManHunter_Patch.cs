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
    [HarmonyPatch(typeof(ARA__ManHunter_Patch), "Prefix", null)]
    class harmonyPatches_ARA_ManHunter_Patch
    {
        [HarmonyBefore("com.github.rimworld.mod.AnimalRangeAttack")]
        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Log.Message("Transpiling ARA__ManHunter_Patch");
            int startIndex = -1;

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldind_Ref)
                {
                    startIndex = i; //Provides the index of the callvirt before the call I need.
                    Log.Message("Found start Index:" + startIndex as String);
                    for (int j = startIndex; j < codes.Count; j++)
                    {
                        if (codes[j].opcode == OpCodes.Ldfld && codes[j + 1].opcode == OpCodes.Callvirt)
                        {
                            codes[j].opcode = OpCodes.Callvirt;
                            codes[j].operand = "instance class ['mscorlib'] System.Collections.Generic.List`1 <class ['Assembly-CSharp'] Verse.Verb>]GetHediffVerbs_forAnimal";
                            codes[j + 1].opcode = OpCodes.Nop;
                            Log.Message("Transpiler for ARA__ManHunter_Patch complete");
                            return codes.AsEnumerable();
                        }
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }
}
