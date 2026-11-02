using RimWorld;
using UnityEngine;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// The one-shot effect fired when a construction frame finishes: soft settling dust over the
    /// footprint. (The old bright flash + sparkle ring were removed — dust only now.)
    /// </summary>
    [StaticConstructorOnStartup]
    public static class CompletionBurst
    {
        private static FleckDef dust;
        private static bool resolved;

        private static void Resolve()
        {
            if (resolved) return;
            resolved = true;
            dust = DefDatabase<FleckDef>.GetNamedSilentFail("DustPuff");
        }

        public static void Spawn(Map map, CellRect rect)
        {
            Resolve();
            Vector3 center = rect.CenterVector3;
            center.y = 0f;
            if (!center.ToIntVec3().ShouldSpawnMotesAt(map)) return;

            const float P = 0.7f;   // global particle-size factor (sprites -30%)

            // A couple of soft dust puffs settling over the footprint.
            if (dust != null)
            {
                int puffs = Mathf.Clamp(rect.Area, 1, 6);
                for (int i = 0; i < puffs; i++)
                {
                    IntVec3 c = rect.RandomCell;
                    if (!c.ShouldSpawnMotesAt(map)) continue;
                    FleckCreationData d = FleckMaker.GetDataStatic(c.ToVector3Shifted(), map, dust, Rand.Range(1.0f, 1.6f) * P);
                    d.velocityAngle = Rand.Range(0, 360);
                    d.velocitySpeed = Rand.Range(0.3f, 0.9f);
                    d.rotationRate = Rand.Range(-40f, 40f);
                    map.flecks.CreateFleck(d);
                }
            }
        }
    }
}
