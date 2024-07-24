using System;
using Verse;
using UnityEngine;
using System.Xml;
using System.Collections;

namespace VanillaStuffableArmor
{
    public class VanillaStuffableArmorSettings : ModSettings
    {
        //Base Game
        public bool stonyStuff = false;
        public bool woodyStuff = false;
        public float additionalStuffMultiplyer = 0.15f;

        //Glass+Lights
        public bool glass = false;
        public bool glassy = false;
        public bool rglass = false;

        //Plastic
        public bool plasticStuff = false;
        public bool plasticPlanks = false;

        //A Mod About Meat
        public bool meaty = false;

        //Blood and Bones
        public bool bones = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref stonyStuff, "stonyStuff");
            Scribe_Values.Look(ref woodyStuff, "woodyStuff");
            Scribe_Values.Look(ref additionalStuffMultiplyer, "additionalStuffMultiplyer");
            Scribe_Values.Look(ref glass, "glass");
            Scribe_Values.Look(ref glassy, "glassy");
            Scribe_Values.Look(ref rglass, "rglass");
            Scribe_Values.Look(ref plasticStuff, "plasticStuff");
            Scribe_Values.Look(ref plasticPlanks, "plasticPlanks");
            Scribe_Values.Look(ref meaty, "meaty");
            Scribe_Values.Look(ref bones, "bones");
            base.ExposeData();
        }
    }

    public class VanillaStuffableArmorMod : Mod
    {
        public static VanillaStuffableArmorSettings settings;

        public VanillaStuffableArmorMod(ModContentPack content) : base(content)
        {
            VanillaStuffableArmorMod.settings = this.GetSettings<VanillaStuffableArmorSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Gap();
            listingStandard.Label("-----Base Game Settings-----");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Stone Brick Type Materials?", ref settings.stonyStuff, "Armors and Clothing use Stone?");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Wood Type Materials?", ref settings.woodyStuff, "Armors and Clothing use Wood?");
            listingStandard.Label("Additive Material Multiplier (0-Uses base apparel stats, Recommended between 10-20 to get material bonuses): " + Math.Round(settings.additionalStuffMultiplyer * 100));
            settings.additionalStuffMultiplyer = listingStandard.Slider(settings.additionalStuffMultiplyer, -1f, 1f);
            listingStandard.Label("*Modifier only adds to apparel that does not have it.");
            listingStandard.Gap();
            listingStandard.Gap();
            listingStandard.Label("-----Glass+Lights Mod Settings(if installed)-----");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Regular Glass Type Materials?", ref settings.glass, "Armors and Clothing use Regular Glass?");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Circular Glass Type Materials?", ref settings.glassy, "Armors and Clothing use Circular Glass?");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Reinforced Glass Type Materials?", ref settings.rglass, "Armors and Clothing use Reinforced Glass?");
            listingStandard.Gap();
            listingStandard.Gap();
            listingStandard.Label("-----Expanded Materials - Plastics Mod Settings(if installed)-----");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Regular Plastic Pellet Type Materials?", ref settings.plasticStuff, "Armors and Clothing use Plastic Pellets?");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Regular Plastic Planks Type Materials?", ref settings.plasticPlanks, "Armors and Clothing use Plastic Planks?");
            listingStandard.Gap();
            listingStandard.Gap();
            listingStandard.Label("-----A Mod About Meat Settings(if installed)-----");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Meaty Type Materials?", ref settings.meaty, "Armors and Clothing use Meaty?");
            listingStandard.Gap();
            listingStandard.Gap();
            listingStandard.Label("-----Blood and Bones(if installed)-----");
            listingStandard.CheckboxLabeled("Armors and Clothing spawn/use Bone Type Materials?", ref settings.bones, "Armors and Clothing use Bones?");
            listingStandard.Gap();
            listingStandard.Gap();
            listingStandard.Label("***Please Restart Game For Changes To Take Effect***");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
            VanillaStuffableArmorMod.settings.Write();
        }

        public override string SettingsCategory()
        {
            return "Stuffable Armor and Clothes";
        }

        public override void WriteSettings()
        {
            
            base.WriteSettings();
        }
    }

    class PatchOperationChangeApparelBase : PatchOperationPathed
    {
        protected string key;
        private XmlContainer value;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode valNode = value.node;
            bool result = false;
            IEnumerator enumerator = xml.SelectNodes(xpath).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    result = true;
                    XmlNode parentNode = obj as XmlNode;
                    XmlNode xmlNode = parentNode.SelectSingleNode(key);
                    if (xmlNode == null)
                    {
                        // Add - Add node if not existing
                        xmlNode = parentNode.OwnerDocument.CreateElement(key);
                        parentNode.AppendChild(xmlNode);
                    }
                    else
                    {
                        // Replace - Clear existing children
                        xmlNode.RemoveAll();
                    }
                    foreach (XmlNode i in valNode)
                    {
                        switch (key)
                        {
                            case "stuffCategories":
                                switch (i.InnerXml)
                                {
                                    case "Woody":
                                        if (VanillaStuffableArmorMod.settings.woodyStuff) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    case "Stony":
                                        if (VanillaStuffableArmorMod.settings.stonyStuff) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    case "Metallic":
                                        xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true));
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "StuffEffectMultiplierArmor":
                                i.InnerText = VanillaStuffableArmorMod.settings.additionalStuffMultiplyer.ToString();
                                xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true));
                                break;
                            default:
                                break;

                        }
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return result;
        }
    }

    class PatchOperationApplyGlass : PatchOperationPathed
    {
        protected string key;
        private XmlContainer value;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode valNode = value.node;
            bool result = false;
            IEnumerator enumerator = xml.SelectNodes(xpath).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    result = true;
                    XmlNode parentNode = obj as XmlNode;
                    XmlNode xmlNode = parentNode.SelectSingleNode(key);
                    if (xmlNode == null)
                    {
                        // Add - Add node if not existing
                        xmlNode = parentNode.OwnerDocument.CreateElement(key);
                        parentNode.AppendChild(xmlNode);
                    }
                    foreach (XmlNode i in valNode)
                    {
                        switch (key)
                        {
                            case "stuffCategories":
                                switch (i.InnerXml)
                                {
                                    case "Glass":
                                        if (VanillaStuffableArmorMod.settings.glass) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    case "Glassy":
                                        if (VanillaStuffableArmorMod.settings.glassy) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    case "RGlass":
                                        if (VanillaStuffableArmorMod.settings.rglass) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;

                        }
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return result;
        }
    }

    class PatchOperationApplyPlastics : PatchOperationPathed
    {
        protected string key;
        private XmlContainer value;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode valNode = value.node;
            bool result = false;
            IEnumerator enumerator = xml.SelectNodes(xpath).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    result = true;
                    XmlNode parentNode = obj as XmlNode;
                    XmlNode xmlNode = parentNode.SelectSingleNode(key);
                    if (xmlNode == null)
                    {
                        // Add - Add node if not existing
                        xmlNode = parentNode.OwnerDocument.CreateElement(key);
                        parentNode.AppendChild(xmlNode);
                    }
                    foreach (XmlNode i in valNode)
                    {
                        switch (key)
                        {
                            case "stuffCategories":
                                switch (i.InnerXml)
                                {
                                    case "VMEu_PlasticStuffCategory":
                                        if (VanillaStuffableArmorMod.settings.plasticStuff) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    case "VMEu_PlasticPlanksStuffCategory":
                                        if (VanillaStuffableArmorMod.settings.plasticPlanks) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;

                        }
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return result;
        }
    }
    class PatchOperationApplyMeaty : PatchOperationPathed
    {
        protected string key;
        private XmlContainer value;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode valNode = value.node;
            bool result = false;
            IEnumerator enumerator = xml.SelectNodes(xpath).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    result = true;
                    XmlNode parentNode = obj as XmlNode;
                    XmlNode xmlNode = parentNode.SelectSingleNode(key);
                    if (xmlNode == null)
                    {
                        // Add - Add node if not existing
                        xmlNode = parentNode.OwnerDocument.CreateElement(key);
                        parentNode.AppendChild(xmlNode);
                    }
                    foreach (XmlNode i in valNode)
                    {
                        switch (key)
                        {
                            case "stuffCategories":
                                switch (i.InnerXml)
                                {
                                    case "Meaty":
                                        if (VanillaStuffableArmorMod.settings.meaty) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;

                        }
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return result;
        }
    }
    class PatchOperationApplyBones : PatchOperationPathed
    {
        protected string key;
        private XmlContainer value;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode valNode = value.node;
            bool result = false;
            IEnumerator enumerator = xml.SelectNodes(xpath).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    result = true;
                    XmlNode parentNode = obj as XmlNode;
                    XmlNode xmlNode = parentNode.SelectSingleNode(key);
                    if (xmlNode == null)
                    {
                        // Add - Add node if not existing
                        xmlNode = parentNode.OwnerDocument.CreateElement(key);
                        parentNode.AppendChild(xmlNode);
                    }
                    foreach (XmlNode i in valNode)
                    {
                        switch (key)
                        {
                            case "stuffCategories":
                                switch (i.InnerXml)
                                {
                                    case "Bone":
                                        if (VanillaStuffableArmorMod.settings.bones) { xmlNode.AppendChild(parentNode.OwnerDocument.ImportNode(i, true)); }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;

                        }
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return result;
        }
    }
}
