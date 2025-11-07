using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;

namespace BillDoorsLootsAndShelves
{
    public class LootContentDef : Def
    {
        public List<LootContentData> contents;

        public RulePackDef manifestNameMaker;

        public int meritCost = 1;

        public string GetManifestLabel(ThingOwner content)
        {
            if (manifestNameMaker != null)
            {
                string manifest = NameGenerator.GenerateName(manifestNameMaker);
                if (content.Count > 0)
                {
                    manifest = manifest.Replace("{RandomCargo}", content.RandomElement().LabelNoParenthesisCap);
                }

                manifest = manifest.Replace("{ContentLabel}", label);
                return manifest;
            }

            return label.CapitalizeFirst();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (LootContentData content in contents)
            {
                stringBuilder.AppendLine(content.ToString());
            }
            return stringBuilder.ToString();
        }
    }


    public class LootContentData
    {
        public List<ThingDef> ThingDefs = new List<ThingDef>();
        public ThingCategoryDef ThingCategory;

        public QualityRange qualityRange = QualityRange.FromString("Normal~Normal");

        public PawnKindDef PawnKindDef;
        public FactionDef faction;
        public bool tamed;
        public bool randomFromList;

        public IntRange count = new IntRange(1, 1);

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("=================");
            if (ThingDefs.Any())
            {
                foreach (ThingDef thingDef in ThingDefs)
                {
                    stringBuilder.AppendInNewLine("Thing: " + thingDef.label);
                }
            }
            if (PawnKindDef != null)
            {
                stringBuilder.AppendInNewLine("PawnKindDef: " + PawnKindDef.label);
            }
            if (ThingCategory != null)
            {
                stringBuilder.AppendInNewLine("ThingCategory: " + ThingCategory.label);
            }
            stringBuilder.AppendInNewLine("Count: " + count.ToString());
            return stringBuilder.ToString();
        }
    }

    public class LootContentDefWeight
    {
        public LootContentDef lootDef;

        public float weight;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "lootDef", xmlRoot.Name);
            weight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }
}
