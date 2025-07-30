using Verse;

namespace ImperialFunctionality
{
    public static class ModCompatibility
    {
        public static bool DubsBadHygieneActive = ModsConfig.IsActive("Dubwise.DubsBadHygiene") || ModsConfig.IsActive("Dubwise.DubsBadHygiene.Lite");
    
        public static bool HasPipeCompAndIsEmpty(Thing thing)
        {
            var pipeComp = thing.TryGetComp<DubsBadHygiene.CompPipe>();
            if (pipeComp != null && pipeComp.pipeNet.WaterStorage <= 0)
            {
                return true;
            }
            return false;
        }
    }
}
