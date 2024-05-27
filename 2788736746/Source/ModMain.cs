using System;
using System.Collections.Generic;
using System.Reflection;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace Trash_Collector_SoS2
{

    public class SoS2WreckDeconstructionAddon : ModBase
    {
        internal static SettingHandle<float> deconstructSpeed;
        internal static SettingHandle<int> activePowerConsumption;

        public override string ModIdentifier
        {
            get => "sos2wreckdeconstaddon";
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();

            deconstructSpeed = Settings.GetHandle("deconstructSpeed", "Deconstruction Speed", "Speed of deconstruction process (each salvage bay deconstruct things with that speed).", 2f, Validators.FloatRangeValidator(1f, 10f));
            activePowerConsumption = Settings.GetHandle("activePowerConsumption", "Active power consumption", "Sets the power consumption of salvage bay, when wreck deconstruct proccess enabled.", 3000, Validators.IntRangeValidator(200, 10000));
        }
    }
}
