namespace Glitterpaths;

[HarmonyPatch(typeof(PathGrid), "CalculatedCostAt")]
public static class PathGrid_CalculatedCostAt
{
    public static IEnumerable<CodeInstruction> Transpiler(
      IEnumerable<CodeInstruction> instrs)
    {
        instrs = instrs.MethodReplacer(typeof(SnowUtility).GetMethod(nameof(SnowUtility.MovementTicksAddOn)), typeof(PathGrid_CalculatedCostAt).GetMethod(nameof(MovementTicksAddOnIgnoreZero)));
        return instrs;
    }

    public static int MovementTicksAddOnIgnoreZero(SnowCategory category)
    {
        switch (category)
        {
            case SnowCategory.None:
                return -100;
            case SnowCategory.Dusting:
                return 0;
            case SnowCategory.Thin:
                return 4;
            case SnowCategory.Medium:
                return 8;
            case SnowCategory.Thick:
                return 12;
            default:
                return 0;
        }
    }
}