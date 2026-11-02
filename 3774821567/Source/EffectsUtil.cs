using UnityEngine;
using RimWorld;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// One-shot celebratory bursts fired from event hooks: craft-quality sparkle,
    /// timber/harvest plant bursts, research breakthrough, and heavy-haul effort puffs.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class EffectsUtil
    {
        private static FleckDef sparkle, leaf, dust, lightbulb, woodchip, leafPale;
        private static bool resolved;

        private static void Resolve()
        {
            if (resolved) return;
            resolved = true;
            sparkle = DefDatabase<FleckDef>.GetNamedSilentFail("JE_Sparkle");
            leaf = DefDatabase<FleckDef>.GetNamedSilentFail("JE_Leaf");
            dust = DefDatabase<FleckDef>.GetNamedSilentFail("DustPuffThick");
            lightbulb = DefDatabase<FleckDef>.GetNamedSilentFail("JE_Lightbulb");
            woodchip = DefDatabase<FleckDef>.GetNamedSilentFail("JE_WoodChip");
            leafPale = DefDatabase<FleckDef>.GetNamedSilentFail("JE_LeafPale");
        }

        private static float I => JobEffectsSettings.Intensity;
        private const float P = 0.7f;   // global particle-size factor (sprites -30%)

        private static void Throw(Map map, Vector3 pos, FleckDef def, float scale, float ang, float speed, Color? col = null)
        {
            if (def == null) return;
            FleckCreationData d = FleckMaker.GetDataStatic(pos, map, def, scale * P);
            d.velocityAngle = ang;
            d.velocitySpeed = speed;
            d.rotation = Rand.Range(0f, 360f);          // random orientation at spawn, not just spin
            d.rotationRate = Rand.Range(-180f, 180f);
            if (col.HasValue) d.instanceColor = col;
            map.flecks.CreateFleck(d);
        }

        // ---- Craft completion, tinted by quality ----
        public static void QualityBurst(Thing thing, Pawn worker)
        {
            Resolve();
            Map map = worker.Map;
            Vector3 c = worker.DrawPos; c.y = 0f;
            if (!c.ToIntVec3().ShouldSpawnMotesAt(map)) return;

            QualityCategory q = QualityCategory.Normal;
            CompQuality cq = (thing as ThingWithComps)?.GetComp<CompQuality>();
            if (cq != null) q = cq.Quality;

            Color col;
            float scale;
            int count;
            switch (q)
            {
                // Desaturated, earthy gold/parchment to sit inside RimWorld's palette.
                case QualityCategory.Legendary: col = new Color(1f, 0.80f, 0.52f); scale = 1.55f; count = 14; break;
                case QualityCategory.Masterwork: col = new Color(1f, 0.86f, 0.5f); scale = 1.4f; count = 12; break;
                case QualityCategory.Excellent: col = new Color(0.96f, 0.92f, 0.72f); scale = 1.2f; count = 10; break;
                default: col = new Color(1f, 0.96f, 0.82f); scale = 1.0f; count = 6; break;
            }
            count = Mathf.RoundToInt(count * I);
            for (int i = 0; i < count; i++)
            {
                float ang = (360f / Mathf.Max(1, count)) * i + Rand.Range(-12f, 12f);
                Throw(map, c, sparkle, scale * Rand.Range(0.7f, 1.2f), ang, Rand.Range(1.2f, 2.8f), col);
            }
        }

        // ---- Plant felled / harvested ----
        public static void PlantBurst(Map map, IntVec3 pos, bool isTree)
        {
            Resolve();
            if (!pos.ShouldSpawnMotesAt(map)) return;
            Vector3 c = pos.ToVector3Shifted(); c.y = 0f;

            // Seasonal recolour: use the near-white leaf mote and tint each leaf to the season
            // (fresh green in spring, an amber/rust mix in autumn, pale in winter).
            bool seasonal = JobEffectsSettings.SeasonalParticles && leafPale != null;
            FleckDef lf = seasonal ? leafPale : leaf;

            if (isTree)
            {
                // NOTE: the felled-tree leaf shower is no longer thrown radially from here. It is
                // spawned as a canopy LEAF-FALL (drifting down, foliage-colour matched) by
                // Patch_Plant_PlantCollected -> ToolAnimator.SpawnTreeFallLeaves, which has the
                // pre-destroy canopy geometry + sampled leaf colour. Here we only do the wood
                // chips + settling dust (which also fire for stumps, that shed no leaves).
                int chips = Mathf.RoundToInt(5 * I);
                for (int i = 0; i < chips; i++)
                    Throw(map, c, woodchip, Rand.Range(0.5f, 0.9f), Rand.Range(0, 360), Rand.Range(4f, 9f));
                int puffs = Mathf.RoundToInt(4 * I);
                for (int i = 0; i < puffs; i++)
                    Throw(map, c + new Vector3(Rand.Range(-0.4f, 0.4f), 0, Rand.Range(-0.4f, 0.4f)), dust, Rand.Range(1.1f, 1.8f), Rand.Range(0, 360), Rand.Range(0.3f, 0.9f));
            }
            else
            {
                int leaves = Mathf.RoundToInt(5 * I);
                for (int i = 0; i < leaves; i++)
                    Throw(map, c, lf, Rand.Range(0.45f, 0.75f), Rand.Range(40f, 140f), Rand.Range(1.5f, 3.2f), seasonal ? SeasonalTint.Leaf(map) : (Color?)null);
            }
        }

        // ---- Research breakthrough ----
        public static void ResearchBurst(Pawn researcher)
        {
            Resolve();
            Map map = researcher.Map;
            Vector3 head = researcher.DrawPos + new Vector3(0f, 0f, 0.85f);
            head.y = 0f;
            if (!head.ToIntVec3().ShouldSpawnMotesAt(map)) return;

            if (lightbulb != null)
            {
                FleckCreationData d = FleckMaker.GetDataStatic(head, map, lightbulb, 1.1f * P);
                d.velocityAngle = 0f;
                d.velocitySpeed = 0.35f;
                map.flecks.CreateFleck(d);
            }
            int n = Mathf.RoundToInt(8 * I);
            for (int i = 0; i < n; i++)
                Throw(map, head, sparkle, Rand.Range(0.7f, 1.2f), Rand.Range(0, 360), Rand.Range(1f, 2.6f), new Color(1f, 0.95f, 0.6f));
        }

        // ---- Heavy haul effort ----
        public static void EffortPuff(Pawn carrier)
        {
            Resolve();
            Map map = carrier.Map;
            Vector3 feet = carrier.DrawPos; feet.y = 0f;
            if (!feet.ToIntVec3().ShouldSpawnMotesAt(map)) return;
            int n = Mathf.RoundToInt(3 * I);
            for (int i = 0; i < n; i++)
                Throw(map, feet + new Vector3(Rand.Range(-0.3f, 0.3f), 0, Rand.Range(-0.3f, 0.1f)), dust, Rand.Range(0.7f, 1.1f), Rand.Range(180f, 360f), Rand.Range(0.4f, 1f));
        }
    }
}
