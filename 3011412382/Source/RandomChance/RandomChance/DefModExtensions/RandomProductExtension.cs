using System.Collections.Generic;
using Verse;

namespace RandomChance
{
    public class RandomProductExtension : DefModExtension
    {
        public float? randomProductChance = 0f;
        public List<RandomProductData> randomProducts = new ();

        public float GetRandomProductWeight(ThingDef productDef)
        {
            foreach (RandomProductData productData in randomProducts)
            {
                if (productData.randomProduct.defName == productDef.defName)
                {
                    return productData.randomProductWeight;
                }
            }
            return 0f; // default weight if not found
        }

        public FloatRange GetRandomProductSpawnRange(ThingDef productDef)
        {
            foreach (RandomProductData productData in randomProducts)
            {
                if (productData.randomProduct.defName == productDef.defName)
                {
                    return productData.randomProductAmountRange;
                }
            }
            return FloatRange.Zero; // default spawn range if not found
        }
    }
}
