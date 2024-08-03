using Verse;

namespace ArchotechWeaponry.Defs
{
    public class ArchotechDamageExtension : DefModExtension
    {
        public HediffDef nonLethalHediffToApplyOnOrganics;
        public float nonLethalSeverityPerHit = 0.10f;
        public HediffDef lethalHediffToApplyOnOrganics;
        public float lethalSeverityPerHit = 0.10f;
    }
}