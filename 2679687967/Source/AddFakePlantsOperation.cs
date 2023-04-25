using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using Verse;
// ReSharper disable InconsistentNaming

namespace Euphoric.DecorativeGrassAndPlants
{
    // ReSharper disable once UnusedType.Global
    public class AddFakePlantsOperation : PatchOperation
    {
        public enum PlantKind
        {
            Plant,
            Tree,
            Bush,
            CavePlant
        }

        public class FakePlantVariant
        {
            public string DefName { get; }
            public string Label { get; }
            public float VisualSize { get; }
            public VariantSize Size { get; }
            public int MaxMeshCount { get; }

            public FakePlantVariant(string defName, string label, float visualSize, VariantSize size, int maxMeshCount)
            {
                DefName = defName;
                Label = label;
                VisualSize = visualSize;
                Size = size;
                MaxMeshCount = maxMeshCount;
            }
        }

        public class FakePlantDef
        {
            public string defName;
            public string label;
            public FloatRange plantVisualSizeRange = FloatRange.One;
            public int maxMeshCount = 1;
            public XmlElement graphicDataNode;
            public PlantKind kind = PlantKind.Plant;
            public float topWindExposure = 0.25f;
            public string altitudeLayer;

            public FakePlantDef Copy()
            {
                return new FakePlantDef()
                {
                    defName = defName,
                    label = label,
                    plantVisualSizeRange = plantVisualSizeRange,
                    maxMeshCount = maxMeshCount,
                    kind = kind,
                    topWindExposure = topWindExposure,
                    graphicDataNode = graphicDataNode.CloneNode(true) as XmlElement,
                    altitudeLayer = altitudeLayer
                };
            }


            public IEnumerable<FakePlantVariant> CreateVariants()
            {
                foreach (var meshCountLabel in CreateMeshCountVariants())
                {
                    foreach (var sizeVariant in CreateSizeVariants())
                    {
                        var variantDefName = defName + "_Fake_" + sizeVariant.Item2 + "_" + meshCountLabel.Item2;
                        var variantLabel = "fake " + label +" " + sizeVariant.Item3 + " " + meshCountLabel.Item3;

                        yield return new FakePlantVariant(variantDefName, variantLabel, sizeVariant.Item1, sizeVariant.Item2, meshCountLabel.Item1);
                    }
                }
            }

            private IEnumerable<(int, string, string)> CreateMeshCountVariants()
            {
                List<(int, string, string)> meshCountLabels = new List<(int, string, string)>()
                {
                    (1, "Single", "single"),
                    (4, "Few", "few"),
                    (9, "Some", "some"),
                    (16, "Lots", "lots"),
                    (25, "Many", "many"),
                };

                if (maxMeshCount == 1)
                {
                    yield return (1, "Single", "");
                }
                else
                {
                    foreach (var meshCountLabel in meshCountLabels)
                    {
                        if (meshCountLabel.Item1 > maxMeshCount)
                            break;

                        yield return meshCountLabel;
                    }
                }
            }

            private IEnumerable<(float, VariantSize, string)> CreateSizeVariants()
            {
                yield return (plantVisualSizeRange.TrueMin, VariantSize.Small, "small");
                if (plantVisualSizeRange.Span > 0.5)
                {
                    yield return (plantVisualSizeRange.Average, VariantSize.Medium, "medium");
                }
                yield return (plantVisualSizeRange.TrueMax, VariantSize.Large, "large");
            }
        }

        public enum VariantSize
        {
            Small,
            Medium,
            Large
        }
        
        protected override bool ApplyWorker(XmlDocument xml)
        {
            //Log.Warning("Applying fake plants patch");

            var plantBase = xml.SelectNodes("/Defs/ThingDef[@Name='PlantBaseNonEdible']")[0];
            var fakePlantDef = new FakePlantDef() { graphicDataNode = xml.CreateElement("graphicData") };
            var fakePlantDefs = ProcessPlantXmlNode(xml, plantBase, fakePlantDef).ToList();

            //var logMessage = "Plant names:" + Environment.NewLine + PlantDefsDebug(fakePlantDefs);
            //Log.Warning(logMessage);

            var defsNode = xml.SelectSingleNode("Defs");

            foreach (var plantDef in fakePlantDefs)
            {
                var fakePlantDesignatorNode = CreateFakePlantDesignator(xml, plantDef);
                defsNode.AppendChild(fakePlantDesignatorNode);

                foreach (var variant in plantDef.CreateVariants())
                {
                    var fakePlantDefNode = CreateFakePlantDefNode(xml, plantDef, variant);
                    defsNode.AppendChild(fakePlantDefNode);
                }
            }

            return true;
        }

        private static XmlNode CreateFakePlantDesignator(XmlDocument xml, FakePlantDef plantDef)
        {
            var fakePlantXml =
                $"<DesignatorDropdownGroupDef>" +
                $"<defName>Decorations_Fake_{plantDef.defName}</defName>" +
                $"</DesignatorDropdownGroupDef>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fakePlantXml);

            var fakePlantDefNode = xml.ImportNode(doc.FirstChild, true);
            
            return fakePlantDefNode;
        }

        private static XmlNode CreateFakePlantDefNode(XmlDocument xml, FakePlantDef plantDef, FakePlantVariant variant)
        {
            var costScale = CalculateVariantCost(plantDef, variant);

            var fakePlantXml =
                $"<ThingDef ParentName=\"FakePlantBase\">" +
                $"<defName>{variant.DefName}</defName>" +
                $"<label>{variant.Label}</label>" +
                plantDef.graphicDataNode.OuterXml +
                $"<costList>" +
                $"    <WoodLog>{Mathf.CeilToInt(10*costScale)}</WoodLog>" +
                $"    <Cloth>{Mathf.CeilToInt(5*costScale)}</Cloth>"+
                $"    <Chemfuel>{Mathf.CeilToInt(3*costScale)}</Chemfuel>"+
                $"</costList>"+
                $"<altitudeLayer>{plantDef.altitudeLayer}</altitudeLayer>" +
                $"<statBases>"+
                $"    <WorkToBuild>{450*costScale}</WorkToBuild>"+
                $"</statBases>"+
                $"<modExtensions><li Class=\"Euphoric.DecorativeGrassAndPlants.FakePlantExtension\">" +
                $"<visualSize>{variant.VisualSize}</visualSize>" +
                $"<maxMeshCount>{variant.MaxMeshCount}</maxMeshCount>" +
                $"<topWindExposure>{plantDef.topWindExposure}</topWindExposure>" +
                $"</li></modExtensions>" +
                $"<designatorDropdown>Decorations_Fake_{plantDef.defName}</designatorDropdown>" +
                $"</ThingDef>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fakePlantXml);

            var fakePlantDefNode = xml.ImportNode(doc.FirstChild, true);
            //Log.Message(fakePlantDefNode.OuterXml);
            return fakePlantDefNode;
        }

        private static float CalculateVariantCost(FakePlantDef plantDef, FakePlantVariant variant)
        {
            float costScale = 0.1f;
            costScale *= Mathf.Sqrt(variant.MaxMeshCount);
            switch (plantDef.kind)
            {
                case PlantKind.Plant:
                    costScale *= 1;
                    break;
                case PlantKind.Tree:
                    costScale *= 5;
                    break;
                case PlantKind.Bush:
                    costScale *= 2f;
                    break;
                case PlantKind.CavePlant:
                    costScale *= 2f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (variant.Size)
            {
                case VariantSize.Small:
                    costScale *= 1;
                    break;
                case VariantSize.Medium:
                    costScale *= 2.5f;
                    break;
                case VariantSize.Large:
                    costScale *= 5;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return costScale;
        }

        private static string PlantDefsDebug(List<FakePlantDef> fakePlantDefs)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var plantGroup in fakePlantDefs.GroupBy(x => x.kind))
            {
                sb.AppendLine(plantGroup.Key.ToString());
                foreach (var plantDef in plantGroup)
                {
                    sb.AppendLine(
                        $"  {plantDef.defName};{plantDef.label};{plantDef.kind};{plantDef.plantVisualSizeRange};{plantDef.plantVisualSizeRange.Span};{plantDef.maxMeshCount};{plantDef.altitudeLayer};");

                    foreach (var variant in plantDef.CreateVariants())
                    {
                        sb.AppendLine($"    {variant.DefName} {variant.MaxMeshCount} {variant.VisualSize}");
                    }
                }
            }

            return sb.ToString();
        }

        private IEnumerable<FakePlantDef> ProcessPlantXmlNode(XmlDocument xml, XmlNode node, FakePlantDef parentDef)
        {
            var thisDef = parentDef.Copy();

            try
            {
                var plantNode = node.SelectSingleNode("plant");
                if (plantNode != null)
                {
                    var visualSizeRangeNode = plantNode.SelectSingleNode("visualSizeRange");
                    if (visualSizeRangeNode != null)
                    {
                        thisDef.plantVisualSizeRange = ParseFloatRangeFromXmlNode(visualSizeRangeNode);
                    }

                    var maxMeshCountNode = plantNode.SelectSingleNode("maxMeshCount");
                    if (maxMeshCountNode != null)
                    {
                        thisDef.maxMeshCount = int.Parse(maxMeshCountNode.InnerText);
                    }
                    
                    var topWindExposureNode = plantNode.SelectSingleNode("topWindExposure");
                    if (topWindExposureNode != null)
                    {
                        thisDef.topWindExposure = float.Parse(topWindExposureNode.InnerText);
                    }
                }

                var altitudeLayerNode = node.SelectSingleNode("altitudeLayer");
                if (altitudeLayerNode != null)
                {
                    thisDef.altitudeLayer = altitudeLayerNode.InnerText;
                }

                if (node.SelectSingleNode("graphicData") is XmlElement graphicDataNode)
                {
                    MergeNodes(thisDef.graphicDataNode, graphicDataNode);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error parsing plant def {node.OuterXml} {e}");
                yield break;
            }

            var name = node.Attributes["Name"]?.Value;
            if (name != null)
            {
                if (name == "TreeBase")
                {
                    thisDef.kind = PlantKind.Tree;
                }
                else if (name == "BushBase")
                {
                    thisDef.kind = PlantKind.Bush;
                }
                else if (name == "CavePlantBase")
                {
                    thisDef.kind = PlantKind.CavePlant;
                }
                
                // is parent, so go through children
                var xpath = $"/Defs/ThingDef[@ParentName='{name}']";
                var plantChildren = xml.SelectNodes(xpath);

                foreach (XmlNode plantChild in plantChildren)
                {
                    var childDefs = ProcessPlantXmlNode(xml, plantChild, thisDef);
                    foreach (var childDef in childDefs)
                    {
                        yield return childDef;
                    }
                }
            }
            else
            {
                // is leaf, return as completed plant def
                thisDef.defName = node.SelectSingleNode("defName").InnerText;
                thisDef.label = node.SelectSingleNode("label").InnerText;

                yield return thisDef;
            }
        }

        private static FloatRange ParseFloatRangeFromXmlNode(XmlNode visualSizeRangeNode)
        {
            FloatRange fr;

            if (visualSizeRangeNode.ChildNodes.Count == 1)
            {
                fr = FloatRange.FromString(visualSizeRangeNode.InnerText);
            }
            else
            {
                fr = new FloatRange();
                fr.min = float.Parse(visualSizeRangeNode.SelectSingleNode("min").InnerText);
                fr.max = float.Parse(visualSizeRangeNode.SelectSingleNode("max").InnerText);
            }

            return fr;
        }

        private void MergeNodes(XmlElement targetNode, XmlElement sourceNode)
        {
            foreach (XmlElement childNode in sourceNode.ChildNodes)
            {
                var clonedChildNode = childNode.CloneNode(true);

                var targetChildren = targetNode.SelectSingleNode(childNode.Name);
                if (targetChildren == null)
                {
                    targetNode.AppendChild(clonedChildNode);
                }
                else
                {
                    targetNode.ReplaceChild(clonedChildNode, targetChildren);
                }
            }
        }
    }
}