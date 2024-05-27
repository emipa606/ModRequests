using System.Xml;
using Verse;

namespace RandomChance
{
    public class RandomProductData
    {
        public ThingDef randomProduct = null;
        public float randomProductWeight = 0f;
        public FloatRange randomProductAmountRange = new (1, 2);
        
        public RandomProductData()
        {

        }

        public RandomProductData(ThingDef randomProduct, float randomProductWeight, FloatRange randomProductAmountRange)
        {
            this.randomProduct = randomProduct;
            this.randomProductWeight = randomProductWeight;
            this.randomProductAmountRange = randomProductAmountRange;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "randomProduct", xmlRoot.Name);

            if (xmlRoot.Attributes != null && xmlRoot.Attributes["Weight"] != null)
            {
                randomProductWeight = ParseHelper.FromString<float>(xmlRoot.Attributes["Weight"].Value);
            }

            string spawnRangeStr = xmlRoot.InnerText.Trim();
            string[] rangeValues = spawnRangeStr.Split('~');
            if (rangeValues.Length == 2)
            {
                float min = ParseHelper.FromString<float>(rangeValues[0]);
                float max = ParseHelper.FromString<float>(rangeValues[1]);
                randomProductAmountRange = new FloatRange(min, max);
            }
        }
    }
}
