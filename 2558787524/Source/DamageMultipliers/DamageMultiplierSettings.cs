using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace DamageMultipliers
{
    class DamageMultiplierSettings : ModBase
    {
        
        public static float damageMultiplier;

        public override string ModIdentifier
        {
            get { return "RimworldDamageMultiplier"; }
        }

        private SettingHandle<float> damageMultiplierSettingHandle;

        public override void DefsLoaded()
        {
            damageMultiplierSettingHandle = Settings.GetHandle<float>("damageMultiplier", "Damage Multiplier", "Configure the multiplier applied to all damage.", 1.0f, Validators.FloatRangeValidator(0, 20));

            updateSettingValues();
        }

        public override void SettingsChanged()
        {
            base.SettingsChanged();
            updateSettingValues();
        }

        private void updateSettingValues()
        {
            damageMultiplier = damageMultiplierSettingHandle.Value;
        }
    }
}

