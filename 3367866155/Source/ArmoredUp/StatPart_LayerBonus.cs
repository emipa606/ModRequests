using RimWorld;
using UnityEngine;
using Verse;

namespace ArmoredUp
{
    public class StatPart_LayerBonus : StatPart
    {
        public float getLayerBonusFactor(Apparel apparel)
        {
            if (ArmoredUp.settings.ExtraLayerPercentage > 0f)
            {
                int layerCount = apparel.def.apparel.layers.Count;
                float layerBonus = 0f;
                float layerFactor = ArmoredUp.settings.ExtraLayerPercentage;

                if (layerCount == 1 && apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                    layerCount = ArmoredUp.settings.PseudoLayersForHelmets;
                
                for (int i = 1; i < layerCount; i++)
                {
                    layerBonus += layerFactor;
                    layerFactor *= layerFactor;
                }
                return 1f + layerBonus;
            }
            return 1f;
        }
        
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing is Apparel apparel && (apparel.def.apparel.layers.Count > 1 || apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead))
            {
                if (PirateCompatibility.piratesLoaded && apparel.IsCasketApparel())
                    return;
                val *= getLayerBonusFactor(apparel);
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing is Apparel apparel && (apparel.def.apparel.layers.Count > 1 || apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead))
            {
                if (PirateCompatibility.piratesLoaded && apparel.IsCasketApparel())
                    return null;
                string count = apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead ? ArmoredUp.settings.PseudoLayersForHelmets.ToString() + " (artificial)" : apparel.def.apparel.layers.Count.ToString();
                return $"Armor Layer Bonus: x{getLayerBonusFactor(apparel):P} for {count} layers";
            }
            return null;
        }
    }
}