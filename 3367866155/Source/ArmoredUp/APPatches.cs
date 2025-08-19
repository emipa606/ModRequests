using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Verse;

namespace ArmoredUp
{
    [HarmonyPatch]
    public static class ArmoredUp_APPatches_Melee
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(VerbProperties), "AdjustedArmorPenetration", new []
            {
                typeof(Tool),
                typeof(Pawn),
                typeof(Thing),
                typeof(HediffComp_VerbGiver)
            });
            yield return AccessTools.Method(typeof(VerbProperties), "AdjustedArmorPenetration", new []
            {
                typeof(Tool),
                typeof(Pawn),
                typeof(ThingDef),
                typeof(ThingDef),
                typeof(HediffComp_VerbGiver)
            });
        }
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            string errorStr = "Armored Up: Unable to patch Melee AP!";
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchStartForward(new CodeMatch(new CodeInstruction(OpCodes.Mul))).ThrowIfInvalid(errorStr)
                .InsertAndAdvance(APUtil.divByDamM)
                .MatchStartForward(new CodeMatch(new CodeInstruction(OpCodes.Ret))).ThrowIfInvalid(errorStr)
                .Insert(APUtil.mulByAPM);
            return codeMatcher.Instructions();
        }
    }

    

    [HarmonyPatch]
    public static class ArmoredUp_APPatches_RangedExtraDamage
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ExtraDamage), "AdjustedArmorPenetration", new Type[0]);
            yield return AccessTools.Method(typeof(ExtraDamage), "AdjustedArmorPenetration",
                new[] { typeof(Verb), typeof(Pawn) });
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            string errorStr = "Armored Up: Unable to patch ExtraDamage AP!";
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchStartForward(new CodeMatch(new CodeInstruction(OpCodes.Ret))).ThrowIfInvalid(errorStr)
                .InsertAndAdvance(APUtil.divByDamR).InsertAndAdvance(APUtil.mulByAPR).Advance(2)
                .MatchStartForward(new CodeMatch(new CodeInstruction(OpCodes.Ret))).ThrowIfInvalid(errorStr)
                .Insert(APUtil.mulByAPR);
            return codeMatcher.Instructions();
        }
    }

    [HarmonyPatch(typeof(ProjectileProperties), "GetArmorPenetration", typeof(float), typeof(StringBuilder))]
    public static class ArmoredUp_APPatches_RangedTranspiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            string errorStr = "Armored Up: Unable to patch Ranged AP!";
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchStartForward(new CodeMatch(new CodeInstruction(OpCodes.Mul))).ThrowIfInvalid(errorStr)
                .InsertAndAdvance(APUtil.divByDamR).MatchStartForward(CodeMatch.IsLdarg(2)).ThrowIfInvalid(errorStr)
                .Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0)).InsertAndAdvance(APUtil.mulByAPR)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_0));
            return codeMatcher.Instructions();
        }
    }

    public static class APUtil
    {
        public static CodeInstruction[] divByDamR =
        {
            CodeInstruction.LoadField(typeof(ArmoredUp), "settings"),
            CodeInstruction.LoadField(typeof(AUSettings), "RangedDamageMultiplier"),
            new CodeInstruction(OpCodes.Div),
            CodeInstruction.LoadField(typeof(ArmoredUp), "settings"),
            CodeInstruction.LoadField(typeof(AUSettings), "DamageMultiplier"),
            new CodeInstruction(OpCodes.Div)
        };

        public static CodeInstruction[] mulByAPR =
        {
            CodeInstruction.LoadField(typeof(ArmoredUp), "settings"),
            CodeInstruction.LoadField(typeof(AUSettings), "RangedAPMult"),
            new CodeInstruction(OpCodes.Mul),
            CodeInstruction.LoadField(typeof(ArmoredUp), "settings"),
            CodeInstruction.LoadField(typeof(AUSettings), "ArmorPenMultiplier"),
            new CodeInstruction(OpCodes.Mul)
        };
        public static CodeInstruction[] divByDamM =
        {
            CodeInstruction.LoadField(typeof(ArmoredUp), "settings"),
            CodeInstruction.LoadField(typeof(AUSettings), "MeleeDamageMultiplier"),
            new CodeInstruction(OpCodes.Div),
            CodeInstruction.LoadField(typeof(ArmoredUp), "settings"),
            CodeInstruction.LoadField(typeof(AUSettings), "DamageMultiplier"),
            new CodeInstruction(OpCodes.Div)
        };

        public static CodeInstruction[] mulByAPM =
        {
            CodeInstruction.LoadField(typeof(ArmoredUp), "settings"),
            CodeInstruction.LoadField(typeof(AUSettings), "MeleeAPMult"),
            new CodeInstruction(OpCodes.Mul),
            CodeInstruction.LoadField(typeof(ArmoredUp), "settings"),
            CodeInstruction.LoadField(typeof(AUSettings), "ArmorPenMultiplier"),
            new CodeInstruction(OpCodes.Mul)
        };
    }

}