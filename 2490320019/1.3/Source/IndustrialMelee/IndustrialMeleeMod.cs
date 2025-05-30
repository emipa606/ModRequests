using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IndustrialMelee
{
    class IndustrialMeleeMod : Mod
    {
        public static IndustrialMeleeSettings settings;
        public IndustrialMeleeMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<IndustrialMeleeSettings>();
            new Harmony("IndustrialMelee.Mod").PatchAll();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Industrial Melee";
        }
    }

    [DefOf]
    public static class IM_DefOf
    {
        public static HediffDef IM_HighBleedrate;
        public static HediffDef IM_10MoreBleedrate;
        public static ThingDef IM_ArrowExplosive;
        public static JobDef IM_Charge;

        public static ThingDef IM_MeleeWeapon_ChainSword;
        public static ThingDef IM_MeleeWeapon_HeaterSaw;
        public static ThingDef IM_MeleeWeapon_DrillSpear;
        public static ThingDef IM_Apparel_IndustrialPowerArmor;
        public static ThingDef IM_MeleeWeapon_RocketLance;
        public static ThingDef IM_MeleeWeapon_ImpactHammer;
        public static ThingDef IM_ImpactBow;
        public static ThingDef IM_MeleeWeapon_GunLance;
        public static ThingDef IM_MeleeWeapon_PowerFist;
        public static ThingDef IM_MeleeWeapon_PowerClaw;
        public static ThingDef IM_MeleeWeapon_ImpactBlade;
        public static ThingDef IM_MeleeWeapon_ImpactAxe;
        public static ThingDef IM_MeleeWeapon_GoliathSledge;
        public static ThingDef IM_Cyrogatana;
    }
}
