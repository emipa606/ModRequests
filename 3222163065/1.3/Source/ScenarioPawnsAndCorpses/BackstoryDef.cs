using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ScenarioPawnsAndCorpses
{
    /// <summary>
    /// This class allows new BackgroundDefs to be specified in XML
    /// </summary>
    class BackstoryDef : Def
    {
#pragma warning disable CS0649
        #region XML
        /// <summary>
        /// XML option: whether this background should occupy the adulthood or childhood slot in the bio.
        /// </summary>
        public BackstorySlot slot = BackstorySlot.Adulthood;

        /// <summary>
        /// XML option: title for the holder. Follows the Rimworld naming schemed where title is general or just male if the female option is provided.
        /// </summary>
        public string title;

        /// <summary>
        /// XML option: title for the holder if female.
        /// </summary>
        public string titleFemale;

        /// <summary>
        /// XML option: shorter version of the title for the holder. Follows the Rimworld naming schemed where title is general or just male if the female option is provided.
        /// </summary>
        public string titleShort;

        /// <summary>
        /// XML option: shorter version of the title for the holder if female.
        /// </summary>
        public string titleShortFemale;

        /// <summary>
        /// XML option: the background's description.
        /// </summary>
        public string baseDescription;

        /// <summary>
        /// XML option: skill modifiers to apply.
        /// </summary>
        public List<BackstorySkillListItem> skillGains = new List<BackstorySkillListItem>();

        /// <summary>
        /// XML option: work the pawn with this background cannot do.
        /// </summary>
        public List<WorkTags> workDisables = new List<WorkTags>();

        /// <summary>
        /// XML option: work the pawn with this background must be able to do.
        /// </summary>
        public List<WorkTags> requiredWorkTags = new List<WorkTags>();

        /// <summary>
        /// XML option: which spawn categories can take this background.
        /// </summary>
        public List<string> spawnCategories = new List<string>();

        /// <summary>
        /// XML option: the unisex body type for the pawn.
        /// </summary>
        public string bodyTypeGlobal;

        /// <summary>
        /// XML option: the body type for the pawn if female and if the bodyTypeGlobal was not specified.
        /// </summary>
        public string bodyTypeFemale;

        /// <summary>
        /// XML option: the body type for the pawn if male and if the bodyTypeGlobal was not specified.
        /// </summary>
        public string bodyTypeMale;

        /// <summary>
        /// XML option: traits the pawn must have.
        /// </summary>
        public List<BackstoryTraitListItem> forcedTraits = new List<BackstoryTraitListItem>();

        /// <summary>
        /// XML option: traits the pawn cannot have.
        /// </summary>
        public List<BackstoryTraitListItem> disallowedTraits = new List<BackstoryTraitListItem>();

        /// <summary>
        /// XML option: whether the background can be dealt out.
        /// </summary>
        public bool shuffleable = true;

        /// <summary>
        /// XML option: likelihood of pawn having relationships.
        /// </summary>
        public RelationshipOverrides relationshipOverrides = new RelationshipOverrides();
        #endregion
#pragma warning restore CS0649

        /// <summary>
        /// Create the background, inserting it into the background database
        /// </summary>
        public override void ResolveReferences()
        {
            base.ResolveReferences();

            if (BackstoryDatabase.allBackstories.ContainsKey(this.defName))
            {
                Log.Error($"[{this.GetType().Namespace}] Duplicate entry: {this.defName}");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.title))
            {
                Log.Error($"[{this.GetType().Namespace}] {this.defName} does not contain a title");
                return;
            }

            if (this.spawnCategories.NullOrEmpty())
            {
                Log.Error($"[{this.GetType().Namespace}] {this.defName} does not contain a spawnCategory");
                return;
            }

            Backstory backstory = new Backstory
            {
                slot = this.slot,
                title = this.title,
                titleFemale = this.titleFemale ?? this.title,
                titleShort = this.titleShort,
                titleShortFemale = this.titleShortFemale ?? this.titleShort,
                baseDesc = this.baseDescription,
                // skillGains is a private field; need to use Harmony to inject code
                //skillGains = this.skillGains,
                // workDisables is a mask; need to OR it up
                //workDisables = this.workDisables,
                // requiredWorkTags is a mask; need to OR it up
                //requiredWorkTags = this.requiredWorkTags,
                spawnCategories = this.spawnCategories,
                // bodyTypes are private fields; need to use Harmony to inject code
                //bodyTypeGlobal = this.bodyTypeGlobal,
                //bodyTypeFemale = this.bodyTypeFemale,
                //bodyTypeMale = this.bodyTypeMale,
                forcedTraits = this.forcedTraits == null ? null : this.forcedTraits.ConvertAll(t => new TraitEntry(t.defName, t.degree)),
                disallowedTraits = this.disallowedTraits == null ? null : this.disallowedTraits.ConvertAll(t => new TraitEntry(t.defName, t.degree)),
                shuffleable = this.shuffleable
            };

            // Override backstory skills
            Traverse.Create(backstory).Field(nameof(this.skillGains)).SetValue(this.skillGains.ToDictionary(i => i.defName, i => i.amount));

            // Set work disables
            if (this.workDisables.Count > 0)
            {
                foreach (WorkTags tag in this.workDisables)
                {
                    backstory.workDisables |= tag;
                }
            }
            else
            {
                backstory.workDisables = WorkTags.None;
            }

            // Set work requireds
            if (this.requiredWorkTags.Count > 0)
            {
                foreach (WorkTags tag in this.requiredWorkTags)
                {
                    backstory.requiredWorkTags |= tag;
                }
            }
            else
            {
                backstory.requiredWorkTags = WorkTags.None;
            }

            // Set body types
            Traverse.Create(backstory).Field(nameof(this.bodyTypeGlobal)).SetValue(this.bodyTypeGlobal);
            Traverse.Create(backstory).Field(nameof(this.bodyTypeFemale)).SetValue(this.bodyTypeFemale);
            Traverse.Create(backstory).Field(nameof(this.bodyTypeMale)).SetValue(this.bodyTypeMale);

            // Flush the background
            backstory.ResolveReferences();
            backstory.PostLoad();
            backstory.identifier = this.defName;

            IEnumerable<string> errors = backstory.ConfigErrors(ignoreNoSpawnCategories: false);

            if (errors.Any())
            {
                Log.Error($"[{this.GetType().Namespace}] {this.defName} contains the following errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
            else
            {
                BackstoryDatabase.AddBackstory(backstory);
            }
        }
    }

#pragma warning disable CS0649
    /// <summary>
    /// Class representing skill entries for XML import
    /// </summary>
    public class BackstorySkillListItem
    {
        /// <summary>
        /// DefName for the skill record
        /// </summary>
        public string defName;

        /// <summary>
        /// Amount to modify the skill by
        /// </summary>
        public int amount = 0;
    }

    /// <summary>
    /// Class representing trait entries for XML import
    /// </summary>
    public class BackstoryTraitListItem
    {
        /// <summary>
        /// DefName for the trait
        /// </summary>
        public TraitDef defName;

        /// <summary>
        /// Degree to set for the trait, noting that 0 is the default for traits without degrees
        /// </summary>
        public int degree = 0;
    }
#pragma warning restore CS0649
}