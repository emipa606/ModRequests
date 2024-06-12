// Nightvision NightVision SettingClasses.cs
// 
// 06 04 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using System.Collections.Generic;
using Verse;

namespace NightVision
{
    /// <summary>
    ///     For storing night vision settings for items of apparel
    /// </summary>
    public class ApparelVisionSetting : IExposable, ISaveCheck
    {
        public void ExposeData()
        {
            Scribe_Values.Look(ref NullifiesPS, "nullifiesphotosens", CompNullifiesPS);
            Scribe_Values.Look(ref GrantsNV, "grantsnightvis", CompGrantsNV);

            SetApparelFlags();
        }

        //Current Settings
        public bool NullifiesPS;
        public bool GrantsNV;

        //Settings in xml defs
        public bool CompNullifiesPS;
        public bool CompGrantsNV;

        //Flags settings
        private ApparelFlags SettingsFlags;



        //Corresponding ThingDef

        public ThingDef ParentDef;
        public CompProperties_NightVisionApparel CompProps;

        /// <summary>
        ///     Parameterless Constructor: necessary for RW scribe system
        /// </summary>
        [UsedImplicitly]
        public ApparelVisionSetting() { }

        /// <summary>
        ///     New Setting
        /// </summary>
        public ApparelVisionSetting(
                        ThingDef apparel
                    )
        {
            ParentDef = apparel;
            AttachComp();
            NullifiesPS = CompNullifiesPS;
            GrantsNV = CompGrantsNV;
            SetApparelFlags();

        }

        public bool HasEffect(ApparelFlags effectFlag)
        {
            return (SettingsFlags & effectFlag) != ApparelFlags.None;
        }

        private void SetApparelFlags()
        {
            SettingsFlags = (NullifiesPS ? ApparelFlags.NullifiesPS : ApparelFlags.None);
            SettingsFlags |= (GrantsNV ? ApparelFlags.GrantsNV : ApparelFlags.None);
        }

        private void AttachComp()
        {
            if (ParentDef == null)
            {
                Log.Message("NightVision.ApparelVisionSetting.AttachComp: Null Parentdef");

                return;
            }

            if (ParentDef.GetCompProperties<CompProperties_NightVisionApparel>() is
                        CompProperties_NightVisionApparel props)
            {
                CompNullifiesPS = props.NullifiesPhotosensitivity;
                CompGrantsNV = props.GrantsNightVision;
                props.AppVisionSetting = this;
            }
            else
            {
                if (ParentDef.comps.NullOrEmpty())
                {
                    ParentDef.comps = new List<CompProperties>();
                }

                CompProps = new CompProperties_NightVisionApparel { AppVisionSetting = this };
                ParentDef.comps.Add(CompProps);
                CompNullifiesPS = false;
                CompGrantsNV = false;
            }
        }

        /// <summary>
        ///     Dictionary builder attaches the comp settings to preexisting entries
        /// </summary>
        public void InitExistingSetting(
                        ThingDef apparel
                    )
        {
            ParentDef = apparel;
            AttachComp();
        }

        public static ApparelVisionSetting CreateNewApparelVisionSetting(
                        ThingDef apparel
                    )
        {
            var newAppSetting = new ApparelVisionSetting { ParentDef = apparel };
            newAppSetting.AttachComp();

            if (newAppSetting.ParentDef != apparel)
            {
                Log.Message(
                            "NightVision.ApparelVisionSetting.CreateNewApparelVisionSetting: Failed to attach Comp, parentdef != given appareldef"
                           );
            }

            return newAppSetting;
        }

        public bool Equals(
                        ApparelVisionSetting other
                    )
            => GrantsNV == other.GrantsNV && NullifiesPS == other.NullifiesPS;

        /// <summary>
        ///     Check to see if this setting should be removed from the dictionary, i.e. current and def values are all false
        /// </summary>
        public bool IsRedundant() => !(GrantsNV || NullifiesPS) && !(CompGrantsNV || CompNullifiesPS);

        /// <summary>
        ///     Check to see if this setting should be saved, i.e. current and def values are all false,
        ///     or current values are equal to def values
        /// </summary>
        /// <returns></returns>
        public bool ShouldBeSaved() => !(IsRedundant() || GrantsNV == CompGrantsNV && NullifiesPS == CompNullifiesPS);
    }
}