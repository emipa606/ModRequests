using RimWorld;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsPredefinedCharacter
{
    public class PredefinedCharacterParmDef : Def
    {
        public BodyTypeDef forcedBodyTypeDef;

        public ThingDef raceDef, weaponStuffDef;

        public PawnKindDef basePawnKindDef;

        public FactionDef fixedFaction;

        public Color? hairColor, skinColor, eyeColor;

        public HairDef forcedHairDef;

        public List<AbilityDef> abilities = new List<AbilityDef>();

        public float fixedAge, fixedChoroAge;

        public Gender gender;

        public HeadTypeDef headType;

        public List<ThingDefCountClass> forcedApparels = new List<ThingDefCountClass>();
        public List<ThingDef> weaponDefs = new List<ThingDef>();
        public List<string> tags = new List<string>();

        public List<BackstoryDef> fixedChildBackStories = new List<BackstoryDef>(), fixedAdultBackStories = new List<BackstoryDef>();

        public List<SkillParm> skillOverrides = new List<SkillParm>();

        public List<TraitRequirement> traits = new List<TraitRequirement>();

        public bool useFactionIdeo, overrideFactionLeader, alwaysRecruitable = true, tracked = false, preventDiscard = true, allowDuplicate = true;

        [MustTranslate]
        public string firstName, nickname, lastname, title;

        [NoTranslate]
        public string eyeTexPath, iconPath;

        public TattooDef faceTattoo, bodyTattoo;

        public List<ScarParm> scars = new List<ScarParm>();

        public List<HediffParm> hediffs = new List<HediffParm>();

        #region Xenotype
        public List<GeneDef> endoGenes = new List<GeneDef>(), xenoGenes = new List<GeneDef>(), removeGenes = new List<GeneDef>();

        public XenotypeDef baseXenotype;
        #endregion

        #region Ideo
        public List<PreceptDef> disallowedPreceptDefs = new List<PreceptDef>();

        public List<MemeDef> disallowedMemes = new List<MemeDef>(), forcedMemes = new List<MemeDef>();

        public List<StyleCategoryDef> styles = new List<StyleCategoryDef>();
        #endregion

        #region Royalty
        public List<RoyalTitleParm> royalTitles = new List<RoyalTitleParm>();

        public List<RoyalPermitParm> royalPermits = new List<RoyalPermitParm>();
        #endregion

        public virtual Texture2D Icon
        {
            get
            {
                if (iconPath == null)
                {
                    return BaseContent.ClearTex;
                }

                return ContentFinder<Texture2D>.Get(iconPath);
            }
        }

        public ThingDef weaponDef
        {
            get
            {
                if (weaponDefs.Any()) return weaponDefs.RandomElement();
                return null;
            }
        }

        public List<ThingDefCountClass> possessions = new List<ThingDefCountClass>();

        public List<CharAppearParm> appearMethods = new List<CharAppearParm>();

        public bool removeSkinColorGene => skinColor != null;
        public bool removeHairColorGene => hairColor != null;

        public List<PawnRenderNodeProperties> renderNodeProperties = new List<PawnRenderNodeProperties>();

        public override IEnumerable<string> ConfigErrors()
        {
            if (basePawnKindDef == null)
            {
                basePawnKindDef = PawnKindDefOf.Colonist;
            }
            if (baseXenotype == null)
            {
                baseXenotype = XenotypeDefOf.Baseliner;
            }
            return base.ConfigErrors();
        }
    }

    public class ScarParm
    {
        public DamageDef damage;

        public BodyPartDef bodyPart;

        public float damageAmount;
    }

    public class HediffParm
    {
        public HediffDef hediff;

        public BodyPartDef bodyPart;

        public string partCustomLabel;
    }

    public class CharAppearParm
    {
        [MustTranslate]
        public string name, description;

        [NoTranslate]
        public string identifier, tag;

        public IncidentDef incidentDef;

        public bool defaultEnabled = false;
    }

    public class SkillParm
    {
        public SkillDef skill;

        public int? amount;

        public Passion? passion;
    }

    public class RoyalTitleParm
    {
        public FactionDef faction;

        public RoyalTitleDef titleDef;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "faction", xmlRoot.Name);
            if (xmlRoot.HasChildNodes)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "titleDef", xmlRoot.FirstChild.Value);
            }
        }
    }

    public class RoyalPermitParm
    {
        public RoyalTitlePermitDef permit;

        public FactionDef faction;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "faction", xmlRoot.Name);
            if (xmlRoot.HasChildNodes)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "permit", xmlRoot.FirstChild.Value);
            }
        }
    }
}
