using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BDsWeaponJamming
{
    public class Setting : ModSettings
    {
        public float AwfulMulti = 0.1f;
        public float PoorMulti = 0.05f;
        public float NormalMulti = 0.01f;
        public float GoodMulti = 0.005f;
        public float ExcellentMulti = 0;
        public float MasterworkMulti = 0;

        public float NeolithicMulti = 5;
        public float MedievalMulti = 2;
        public float IndustrialMulti = 1;
        public float SpacerMulti = 0;
        public float UltraMulti = 0;

        public bool jamReload = true;
        public bool JamForNPC = true;
        public bool JamDestructionHurtHand = true;
        public bool HitpointMatters = true;
        public float LightlyDamagedHP = 0.85f;
        public float BadlyDamagedHP = 0.5f;
        public int WeaponDamage = 5;
        public int ExternalPowerThreshold = 25;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AwfulMulti, "AwfulMulti", defaultValue: 0.1f);
            Scribe_Values.Look(ref PoorMulti, "PoorMulti", defaultValue: 0.05f);
            Scribe_Values.Look(ref NormalMulti, "NormalMulti", defaultValue: 0.01f);
            Scribe_Values.Look(ref GoodMulti, "GoodMulti", defaultValue: 0.005f);
            Scribe_Values.Look(ref ExcellentMulti, "ExcellentMulti", defaultValue: 0f);
            Scribe_Values.Look(ref MasterworkMulti, "MasterworkMulti", defaultValue: 0f);

            Scribe_Values.Look(ref NeolithicMulti, "NeolithicMulti", defaultValue: 5f);
            Scribe_Values.Look(ref MedievalMulti, "MedievalMulti", defaultValue: 2f);
            Scribe_Values.Look(ref IndustrialMulti, "IndustrialMulti", defaultValue: 1f);
            Scribe_Values.Look(ref SpacerMulti, "SpacerMulti", defaultValue: 0f);
            Scribe_Values.Look(ref UltraMulti, "UltraMulti", defaultValue: 0f);

            Scribe_Values.Look(ref jamReload, "jamReload", defaultValue: true);
            Scribe_Values.Look(ref JamForNPC, "JamForNPC", defaultValue: true);
            Scribe_Values.Look(ref JamDestructionHurtHand, "JamDestructionHurtHand", defaultValue: true);
            Scribe_Values.Look(ref HitpointMatters, "HitpointMatters", defaultValue: true);
            Scribe_Values.Look(ref LightlyDamagedHP, "LightlyDamagedHP", defaultValue: 0.85f);
            Scribe_Values.Look(ref BadlyDamagedHP, "BadlyDamagedHP", defaultValue: 0.5f);
            Scribe_Values.Look(ref WeaponDamage, "WeaponDamage", defaultValue: 5);
            Scribe_Values.Look(ref ExternalPowerThreshold, "ExternalPowerThreshold", defaultValue: 25);
        }
    }

    public class BDJamMod : Mod
    {
        public const double powerMulti = 1.5;

        public static Setting settings;

        public float AwfulMulti => settings.AwfulMulti;
        public float PoorMulti => settings.PoorMulti;
        public float NormalMulti => settings.NormalMulti;
        public float GoodMulti => settings.GoodMulti;
        public float ExcellentMulti => settings.ExcellentMulti;
        public float MasterworkMulti => settings.MasterworkMulti;

        public float NeolithicMulti => settings.NeolithicMulti;
        public float MedievalMulti => settings.MedievalMulti;
        public float IndustrialMulti => settings.IndustrialMulti;
        public float SpacerMulti => settings.SpacerMulti;
        public float UltraMulti => settings.UltraMulti;

        public static bool JamForNPC => settings.JamForNPC;
        public static bool HitpointMatters => settings.HitpointMatters;

        public static bool JamDestructionHurtHand => settings.JamDestructionHurtHand;
        public float LightlyDamagedHP => settings.LightlyDamagedHP;
        public float BadlyDamagedHP => settings.BadlyDamagedHP;
        public int ExternalPowerThreshold => settings.ExternalPowerThreshold;
        public static bool jamReload => settings.jamReload;
        public int WeaponDamage => settings.WeaponDamage;
        public BDJamMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<Setting>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 17f) / 2f;
            listing_Standard.Begin(inRect);
            Text.Font = GameFont.Small;
            listing_Standard.GapLine();
            listing_Standard.Label("BDJ_AwfulMulti".Translate() + Math.Pow(AwfulMulti, powerMulti).ToString("P1"));
            settings.AwfulMulti = listing_Standard.Slider(AwfulMulti, 0, 1);
            listing_Standard.Label("BDJ_PoorMulti".Translate() + Math.Pow(PoorMulti, powerMulti).ToString("P1"));
            settings.PoorMulti = listing_Standard.Slider(PoorMulti, 0, 1);
            listing_Standard.Label("BDJ_NormalMulti".Translate() + Math.Pow(NormalMulti, powerMulti).ToString("P1"));
            settings.NormalMulti = listing_Standard.Slider(NormalMulti, 0, 1);
            listing_Standard.Label("BDJ_GoodMulti".Translate() + Math.Pow(GoodMulti, powerMulti).ToString("P1"));
            settings.GoodMulti = listing_Standard.Slider(GoodMulti, 0, 1);
            listing_Standard.Label("BDJ_ExcellentMulti".Translate() + Math.Pow(ExcellentMulti, powerMulti).ToString("P1"));
            settings.ExcellentMulti = listing_Standard.Slider(ExcellentMulti, 0, 1);
            listing_Standard.Label("BDJ_MasterworkMulti".Translate() + Math.Pow(MasterworkMulti, powerMulti).ToString("P1"));
            settings.MasterworkMulti = listing_Standard.Slider(MasterworkMulti, 0, 1);
            listing_Standard.GapLine();
            listing_Standard.Label("BDJ_NeolithicMulti".Translate() + Math.Pow(NeolithicMulti, powerMulti).ToString("P1"));
            settings.NeolithicMulti = listing_Standard.Slider(NeolithicMulti, 0, 4.6415888f);
            listing_Standard.Label("BDJ_MedievalMulti".Translate() + Math.Pow(MedievalMulti, powerMulti).ToString("P1"));
            settings.MedievalMulti = listing_Standard.Slider(MedievalMulti, 0, 4.6415888f);
            listing_Standard.Label("BDJ_IndustrialMulti".Translate() + Math.Pow(IndustrialMulti, powerMulti).ToString("P1"));
            settings.IndustrialMulti = listing_Standard.Slider(IndustrialMulti, 0, 4.6415888f);
            listing_Standard.Label("BDJ_SpacerMulti".Translate() + Math.Pow(SpacerMulti, powerMulti).ToString("P1"));
            settings.SpacerMulti = listing_Standard.Slider(SpacerMulti, 0, 4.6415888f);
            listing_Standard.Label("BDJ_UltraMulti".Translate() + Math.Pow(UltraMulti, powerMulti).ToString("P1"));
            settings.UltraMulti = listing_Standard.Slider(UltraMulti, 0, 4.6415888f);
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("BDJ_jamReload".Translate(), ref settings.jamReload);
            listing_Standard.CheckboxLabeled("BDJ_JamForNPC".Translate(), ref settings.JamForNPC);
            listing_Standard.CheckboxLabeled("BDJ_JamDestructionHurtHand".Translate(), ref settings.JamDestructionHurtHand);

            listing_Standard.CheckboxLabeled("BDJ_HitpointMatters".Translate(), ref settings.HitpointMatters);

            listing_Standard.Label("BDJ_LightDamageHP".Translate() + LightlyDamagedHP.ToString("P1"));
            settings.LightlyDamagedHP = listing_Standard.Slider(LightlyDamagedHP, 0, 1);

            listing_Standard.Label("BDJ_BadDamageHP".Translate() + BadlyDamagedHP.ToString("P1"));
            settings.BadlyDamagedHP = listing_Standard.Slider(BadlyDamagedHP, 0, 1);

            listing_Standard.Label("BDJ_WeaponDamage".Translate() + WeaponDamage.ToString());
            settings.WeaponDamage = (int)listing_Standard.Slider(WeaponDamage, 0, 100);
            listing_Standard.Label("BDJ_ExternalPowerThreshold".Translate() + ExternalPowerThreshold.ToString());
            settings.ExternalPowerThreshold = (int)listing_Standard.Slider(ExternalPowerThreshold, 0, 150);
            if (listing_Standard.ButtonText("BDJ_SettingReset".Translate()))
            {
                settings.AwfulMulti = 0.21544f;
                settings.PoorMulti = 0.13572f;
                settings.NormalMulti = 0.04641f;
                settings.GoodMulti = 0.02924f;
                settings.ExcellentMulti = 0;
                settings.MasterworkMulti = 0;

                settings.NeolithicMulti = 2.92401f;
                settings.MedievalMulti = 1.5874f;
                settings.IndustrialMulti = 1;
                settings.SpacerMulti = 0;
                settings.UltraMulti = 0;

                settings.LightlyDamagedHP = 0.85f;
                settings.BadlyDamagedHP = 0.5f;
            }
            listing_Standard.End();
        }

        public override string SettingsCategory()
        {
            return "BDJ_Setting".Translate();
        }

        public static float GetJamChance(ThingWithComps weapon, CompQuality compQuality, DefModExtension_Jamming modExtension)
        {
            float jamChance = 0;
            if (compQuality != null)
            {
                switch (compQuality.Quality)
                {
                    case QualityCategory.Awful:
                        jamChance = (float)Math.Pow(settings.AwfulMulti, powerMulti);
                        break;

                    case QualityCategory.Poor:
                        jamChance = (float)Math.Pow(settings.PoorMulti, powerMulti);
                        break;

                    case QualityCategory.Normal:
                        jamChance = (float)Math.Pow(settings.NormalMulti, powerMulti);
                        break;

                    case QualityCategory.Good:
                        jamChance = (float)Math.Pow(settings.GoodMulti, powerMulti);
                        break;

                    case QualityCategory.Excellent:
                        jamChance = (float)Math.Pow(settings.ExcellentMulti, powerMulti);
                        break;

                    case QualityCategory.Masterwork:
                        jamChance = (float)Math.Pow(settings.MasterworkMulti, powerMulti);
                        break;
                }
            }
            switch (weapon.def.techLevel)
            {
                default:
                    jamChance *= 0;
                    break;

                case TechLevel.Neolithic:
                    jamChance *= (float)Math.Pow(settings.NeolithicMulti, powerMulti);
                    break;

                case TechLevel.Medieval:
                    jamChance *= (float)Math.Pow(settings.MedievalMulti, powerMulti);
                    break;

                case TechLevel.Industrial:
                    jamChance *= (float)Math.Pow(settings.IndustrialMulti, powerMulti);
                    break;

                case TechLevel.Spacer:
                    jamChance *= (float)Math.Pow(settings.SpacerMulti, powerMulti);
                    break;

                case TechLevel.Ultra:
                    jamChance *= (float)Math.Pow(settings.UltraMulti, powerMulti);
                    break;
            }
            if (settings.HitpointMatters && weapon.def.useHitPoints)
            {
                float hitPointPercentage = (float)weapon.HitPoints / (float)weapon.MaxHitPoints;
                float hitPointPenalty = 0;

                if (hitPointPercentage < settings.LightlyDamagedHP)
                {
                    hitPointPenalty += (float)(settings.LightlyDamagedHP - hitPointPercentage);
                    if (hitPointPercentage < settings.BadlyDamagedHP)
                    {
                        hitPointPenalty += (float)(settings.BadlyDamagedHP - hitPointPercentage) * 5;
                    }
                    jamChance += (hitPointPenalty * 0.1f);
                }
            }
            if (modExtension != null)
            {
                jamChance *= modExtension.jamChanceMulti;
                jamChance += modExtension.jamChancePostfix;
            }
            return jamChance;
        }

        public static float GetJamChance(Thing weapon, CompQuality compQuality, DefModExtension_Jamming modExtension)
        {
            ThingWithComps thing = weapon as ThingWithComps;
            return GetJamChance(thing, compQuality, modExtension);
        }

        private static IEnumerable<BodyPartRecord> GetManipulationLimb(HediffSet bodyModel)
        {
            return from x in bodyModel.GetNotMissingParts()
                   where (x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))) && (x.def.tags.Contains<BodyPartTagDef>(BodyPartTagDefOf.ManipulationLimbDigit) || x.def.tags.Contains<BodyPartTagDef>(BodyPartTagDefOf.ManipulationLimbSegment) || x.def.tags.Contains<BodyPartTagDef>(BodyPartTagDefOf.ManipulationLimbCore))
                   select x;
        }

        public static bool DoWeaponDamage(ThingWithComps weapon, Pawn casterPawn)
        {
            if (settings.WeaponDamage > 0 && weapon.def.useHitPoints)
            {
                weapon.HitPoints -= settings.WeaponDamage;
                if (weapon.HitPoints <= 0)
                {
                    weapon.Destroy();
                    if (casterPawn != null && casterPawn.Spawned)
                    {
                        if (casterPawn.stances.curStance is Stance_Warmup)
                        {
                            casterPawn.stances.CancelBusyStanceSoft();
                        }
                        if (casterPawn.CurJob != null)
                        {
                            casterPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        }
                        if (settings.JamDestructionHurtHand)
                        {
                            HediffSet hediffSet = casterPawn.health.hediffSet;
                            IEnumerable<BodyPartRecord> source = GetManipulationLimb(hediffSet);
                            if (source.Any())
                            {
                                BodyPartRecord bodyPartRecord = source.RandomElement();
                                DamageInfo splinterDamage = new DamageInfo(DamageDefOf.Cut, 5, 0.5f, -1, weapon, null, weapon.def);
                                splinterDamage.SetHitPart(bodyPartRecord);
                                casterPawn.TakeDamage(splinterDamage);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
