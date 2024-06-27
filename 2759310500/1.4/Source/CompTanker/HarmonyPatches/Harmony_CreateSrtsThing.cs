using HarmonyLib;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace CompTanker.HarmonyPatches
{
    [HarmonyPatch]
    public static class Harmony_CreateSrtsThing
    {
        [UsedImplicitly]
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            var type = AccessTools.TypeByName("SRTS.CompLaunchableSRTS");
            yield return AccessTools.Method(type, "TryLaunch");

            type = AccessTools.TypeByName("SRTS.CompBombFlyer");
            yield return AccessTools.Method(type, "TryLaunchBombRun");
        }

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, MethodBase parentMethod)
        {
            var target = AccessTools.Method(typeof(Harmony_CreateSrtsThing), nameof(SetupThing));
            var targetIndex = parentMethod.Name == "TryLaunch" ? 10 : 7;

            foreach (var ci in codeInstructions)
            {
                yield return ci;

                if (ci.opcode == OpCodes.Stloc_S && ci.operand is LocalBuilder builder && builder.LocalIndex == targetIndex)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, ci.operand);
                    yield return new CodeInstruction(OpCodes.Call, target);
                }
            }
        }

        private static void SetupThing(ThingComp parent, Thing thing)
        {
            var comps = parent.parent.GetComps<CompTanker>()?.ToArray();
            if (comps == null || comps.Length == 0 || thing is not ThingWithComps thingWithComps) return;

            var newComps = thingWithComps.GetComps<CompTanker>().ToArray();

            foreach (var comp in comps)
            {
                var newComp = newComps.FirstOrDefault(x => x.Props.contents == comp.Props.contents);
                if (newComp == null) continue;
                newComp.storedAmount = comp.storedAmount;
                newComp.isDraining = false;
                newComp.isFilling = false;
                newComp.contaminationLevel = comp.contaminationLevel;
            }
        }
    }
}
