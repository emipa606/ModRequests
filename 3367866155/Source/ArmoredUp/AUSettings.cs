using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ArmoredUp;

public class AUSettings : ModSettings
    {
        public float ExtraLayerPercentage = 0.5f;
        public float ArmorMinDamage = 0.05f;
        public float ArmorPennedDamageMult = 2f;
        public float PrimitiveArmorGunResistance = 0.33f;
        public float MinDamagedArmorEffectiveness = 0.1f;
        public bool HeatBluntTraumaActive = true;
        public float StoppingPowerBluntPenBonus = 0.5f;
        public float StoppingPowerBluntDamageBonus = 0f;
        public float MaxStoppingPower = 4f;
        public float BluntTraumaMultiplier = 0.5f;
        //public float BuffMetalBluntMultiplier = 2f;
        public float ArmorPenMultiplier = 2f;
        public float DamageMultiplier = 1.5f;
        public float MeleeAPMult = 1f;
        public float MeleeDamageMultiplier = 1f;
        public float RangedAPMult = 1.5f;
        public float RangedDamageMultiplier = 1f;
        public float MeleeArtificialStoppingPower = 2f;
        public float BasicFireAP = 0.5f;
        public float HardArmorRatio = 0.36f;
        public bool SoftArmorPenaltyActive = false;
        public float HardArmorBonusMult = 2f;
        public float BluntTraumaPenMultiplier = 1f;
        public float ArmorDamageMult = 0.5f;
        public bool ArmorDestructionProtection = true;
        public float NaturalArmorHPMultiplier = 3f;
        public float NaturalArmorMaxHealthscale = 5f;
        public int PseudoLayersForHelmets = 2;
        public float MinStoppingPower = 1f;
        public float StoppingPowerBaseScaling = 1f;
        public float VehicleArmorForPenetration = 1.5f;
        public float VehicleUnarmoredX = 0.45f;
        public float VehicleUnarmoredY = 0.3f;
        public float VehicleLightX = 0.72f;
        public float VehicleLightY = 0.85f;
        public float VehicleMediumX = 0.80f;
        public float VehicleMediumY = 1.25f;
        public float VehicleHeavyX = 1f;
        public float VehicleHeavyY = 2.5f;
        public float VehicleExampleCurvePoint = 0.5f;
        public float VehiclePseudoHPMult = 5f;
        
        public List<CurvePoint> points;
        
        public SimpleCurve VehicleArmorCurve;
        
        private Vector2 _scrollPosition;

        public AUSettings()
        {
            points = new List<CurvePoint>()
            {
                new CurvePoint(0, 0),
                new CurvePoint(VehicleUnarmoredX, VehicleUnarmoredY),
                new CurvePoint(VehicleLightX, VehicleLightY),
                new CurvePoint(VehicleMediumX, VehicleMediumY),
                new CurvePoint(VehicleHeavyX, VehicleHeavyY),
                new CurvePoint(1000, 1000)
            };
            VehicleArmorCurve = new SimpleCurve(points);
        }

        private void Reset()
        {
            AUSettings obj = new AUSettings();
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                fieldInfo.SetValue(this, fieldInfo.GetValue(obj));
            }
        }
        
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, 1600f);
            Widgets.BeginScrollView(inRect, ref _scrollPosition, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);
            //listingStandard.CheckboxLabeled("exampleBoolExplanation", ref ArmoredUp.ArmoredUp.settings.exampleBool, "exampleBoolToolTip");
            listingStandard.Label("Armor Pen Multipliers. Armor penetration is reduced by armor layers, potentially to 0%");
            ArmorPenMultiplier = listingStandard.SliderLabeled($"Armor Pen Multiplier: {ArmorPenMultiplier:N2}",ArmorPenMultiplier,0f, 5f, tooltip: "General AP multiplier to both projectiles and melee attacks.");
            RangedAPMult = listingStandard.SliderLabeled($"Ranged AP Multiplier: {RangedAPMult:N2}",RangedAPMult, 0f, 5f, tooltip: "AP multiplier applied to projectile attacks only.");
            MeleeAPMult = listingStandard.SliderLabeled($"Melee/Other AP Multiplier: {MeleeAPMult:N2}",MeleeAPMult, 0f, 5f, tooltip: "AP multiplier applied to melee attacks only.");
            BasicFireAP = listingStandard.SliderLabeled($"Fire/Burning AP: {BasicFireAP:P2}",BasicFireAP, 0f, 1f, tooltip: "Fire/being on fire normally has no AP. This is the base number applied to ANY heat damage with 0 AP.");
            listingStandard.GapLine();
            listingStandard.Label("Damage Multipliers");
            DamageMultiplier = listingStandard.SliderLabeled($"Damage Multiplier: {DamageMultiplier:N2}",DamageMultiplier, 0f, 5f, tooltip: "Damage multiplier applied to both projectiles and melee attacks.");
            RangedDamageMultiplier = listingStandard.SliderLabeled($"Ranged Damage Multiplier: {RangedDamageMultiplier:N2}",RangedDamageMultiplier, 0f, 5f, tooltip: "Damage multiplier applied to projectile attacks only.");
            MeleeDamageMultiplier = listingStandard.SliderLabeled($"Melee/Other Damage Multiplier: {MeleeDamageMultiplier:N2}",MeleeDamageMultiplier, 0f, 5f, tooltip: "Damage multiplier applied to melee attacks only.");
            listingStandard.GapLine();
            listingStandard.Label("Armor resilience and effectiveness  Effective Armor is based on armor durability; eg, Armor at 90% durability has 90% of its normal ratings. Natural armor 'durability' is based on target health.");
            MinDamagedArmorEffectiveness = listingStandard.SliderLabeled($"Minimum Armor Effectiveness: {MinDamagedArmorEffectiveness:P2}",MinDamagedArmorEffectiveness, 0f, 1f, tooltip: "The minimum % armor rating armor can have from durability loss; eg at 0.3, the armor will never go below 30% effectiveness.");
            ExtraLayerPercentage = listingStandard.SliderLabeled($"Multi-layer Armor Buff: {ExtraLayerPercentage:P2}",ExtraLayerPercentage, 0f, 1f,
                tooltip: "Additional armor checks can be given to multi-layered armor, with each successive layer multiplied by this number. Significantly strengthens layered armor, but also makes it take damage much faster. Set to 0 to disable.");
            PseudoLayersForHelmets = (int)listingStandard.SliderLabeled(
                $"Layer Bonus for Non-Layered Helmets: {PseudoLayersForHelmets:D}", PseudoLayersForHelmets, 1, 3,
                tooltip:
                "Most helmets by default only occupy the Headgear layer, making them weaker than the body armor even when not intended. Setting this to 2 will negate that in most circumstances.");
            ArmorMinDamage = listingStandard.SliderLabeled($"Minimum Durability Damage: {ArmorMinDamage:P2}",ArmorMinDamage, 0f, 1f, tooltip: "Armor that is stronger than the AP takes less damage from blocking, with a ratio of AP/AR, to a minimum of this. Based on effective armor.");
            ArmorPennedDamageMult = listingStandard.SliderLabeled($"Penetrated Armor Durability Loss: {ArmorPennedDamageMult:N2}",ArmorPennedDamageMult, 1f, 5f, tooltip: "Armor that doesn't fully block and attack can take increased damage. Set to 1 to disable.");
            ArmorDamageMult = listingStandard.SliderLabeled($"Armor Durability Loss Multiplier: {ArmorDamageMult:P2}",ArmorDamageMult, 0.01f, 5f, tooltip: "Modifier for armor durability loss; applied AFTER the minimum durability damage setting.");
            listingStandard.CheckboxLabeled("Armor Destruction Protection", ref ArmorDestructionProtection, "Prevents damaging hits from fully destroying armor.");
            PrimitiveArmorGunResistance = listingStandard.SliderLabeled($"Primitive Armor and Clothing Effectiveness: {PrimitiveArmorGunResistance:P2}",PrimitiveArmorGunResistance, 0f, 1f, tooltip: "Any protective layer that is medieval tech or earlier (including many clothes), will have reduced effect against industrial and higher ranged attacks. Set to 1 to disable.");
            listingStandard.GapLine();
            listingStandard.Label(
                "This mod guesses at which armors are 'hard' using a ratio of blunt:sharp resistance, defaulting to 0.36 - the same as a flak vest - and will modify the blunt resistance based on this. The settings below are based on this.");
            HardArmorBonusMult = listingStandard.SliderLabeled($"Hard Armor Bonus Blunt Resistance: {HardArmorBonusMult:N2}",HardArmorBonusMult, 1f, 5f, tooltip: "The multiplier for the difference in armor ratio. At the default values, this means an armor with 100% sharp and 40% blunt will be buffed to 44% blunt. Will not buff blunt resistance higher than sharp.");
            HardArmorRatio = listingStandard.SliderLabeled($"Hard Armor Threshold: {HardArmorRatio:P0}",HardArmorRatio,0f, 1f, tooltip: "Threshold to determine hard armor buffs/debuffs, defaulting to 36%, same as a flak vest.");
            listingStandard.CheckboxLabeled("Soft Armor Penalty On?", ref SoftArmorPenaltyActive, "By default, armors below the ratio are unaffected. Turn this on to instead debuff their armor rating. Combined with raising the ratio, will make flak vests and clothing let through significantly more blunt damage.");
            NaturalArmorHPMultiplier = listingStandard.SliderLabeled($"Natural Armor HP Mult: {NaturalArmorHPMultiplier:P0}",NaturalArmorHPMultiplier,1f, 10f, tooltip: "Natural armor (and warcasket armor) is degraded temporarily by damage, inflicting the 'Battered' Hediff. The armor's fake durability score for scaling the battered effect is based on the greater of (150 * Healthscale) or 50, then multiplied by this number.");
            NaturalArmorMaxHealthscale = listingStandard.SliderLabeled($"Max Health Scale for Natural Armor: {NaturalArmorMaxHealthscale:P}", NaturalArmorMaxHealthscale, 1f, 10f, tooltip: "Max health scale for natural armor false durability, to prevent runaway durability.");
            listingStandard.GapLine();
            listingStandard.Label("Blunt trauma  Blunt trauma is the blunt hits generated by blocked sharp (and potentially other) hits. It is reduced by the layer it is generated at, and successive layers, but not previous layers; is summed at the end of a hit; and is proportional to how much sharp was blocked. The armor penetration of a blunt trauma is based on the original hit.");
            BluntTraumaMultiplier = listingStandard.SliderLabeled($"Blunt Trauma Damage Multiplier: {BluntTraumaMultiplier:N2}",BluntTraumaMultiplier, 0f, 2f, tooltip: "Sets how much of BLOCKED sharp damage is converted to blunt. Increase to make blunt trauma more damaging.");
            BluntTraumaPenMultiplier = listingStandard.SliderLabeled($"Blunt Trauma Damage AP Multiplier: {BluntTraumaPenMultiplier:N2}",BluntTraumaPenMultiplier, 0f, 2f, tooltip: "Sets how much of BLOCKED sharp penetration is converted to blunt pen. Increase to make blunt trauma more prevalent.");
            StoppingPowerBluntPenBonus = listingStandard.SliderLabeled($"Stopping Power Blunt Trauma Pen Bonus: {StoppingPowerBluntPenBonus:N2}",StoppingPowerBluntPenBonus, 0f, 3f, tooltip: "Blunt trauma armor penetration is multiplied by the stopping power of the weapon; this is a multiplier on stopping power GREATER THAN the below setting; eg at 0.5, and a Base Scaling of 1, a stopping power of 2 gives a ((2-1)*0.5) = 50% bonus.");
            StoppingPowerBluntDamageBonus = listingStandard.SliderLabeled($"Stopping Power Blunt Trauma Damage Bonus: {StoppingPowerBluntDamageBonus:N2}",StoppingPowerBluntDamageBonus, 0f, 2f, tooltip: "As above, but for damage.");
            MinStoppingPower = listingStandard.SliderLabeled($"Min Stopping Power for Blunt Trauma: {MinStoppingPower:N2}",MinStoppingPower, 0f, 5f, tooltip: "Cutoff for stopping power applying to blunt trauma; below this number, it will not be applied.");
            MaxStoppingPower = listingStandard.SliderLabeled($"Max Stopping Power for Blunt Trauma: {MaxStoppingPower:N2}",MaxStoppingPower, 1f, 10f, tooltip: "Maximum stopping power that can be used for blunt trauma bonus calculations. Any amount greater than this is reduced to this.");
            StoppingPowerBaseScaling = listingStandard.SliderLabeled($"Base Scaling Target for Stopping Power: {StoppingPowerBaseScaling:N2}",StoppingPowerBaseScaling, 0f, 10f, tooltip: "The 'base number' for stopping power scaling. Only the amount of SP greater than this will be scaled. Set lower than the minimum to have a sudden jump in blunt trauma effect.");
            MeleeArtificialStoppingPower = listingStandard.SliderLabeled($"Melee Stopping Power: {MeleeArtificialStoppingPower:N2}",MeleeArtificialStoppingPower, 0.5f, 10f, tooltip: "Melee attacks do not have a stopping power; this is used instead.");
            listingStandard.CheckboxLabeled("Heat Based Blunt Trauma?",ref ArmoredUp.settings.HeatBluntTraumaActive,"Determines if heat damage can cause blunt trauma. Due to how this works internally, will not affect heat damage using the 'flame' damage worker, but will affect others like burn.");
            if (VehiclesCompatibility.VehiclesActive)
            {
                listingStandard.GapLine();
                listingStandard.Label(
                    "Vehicle armor is simplified from regular armor; there is no blunt trauma or stopping power effects. To keep balance, rather than a flat rating multiplier, vehicle armor is curved as below. Vehicle armor does inherit settings for minimum armor damage, minimum effective armor, and armor damage multipliers.");
                VehiclePseudoHPMult = listingStandard.SliderLabeled($"Vehicle HP Pseudo-Scaling Factor: {VehiclePseudoHPMult:N2}", VehiclePseudoHPMult, 1f, 10f, tooltip: "Vehicle HP is much lower than armor, so even with high AR values, it's typically easy to demolish a vehicle even with vanilla weapons. Component damage is divided by this factor to simulate an HP multiplier.");
                listingStandard.Label(
                    $"The below settings define a curve, with text labels for ease of use; for the unfamiliar, the input value (Vehicle Component Armor) is matched to the closest set of ratings and then scaled by how far it is between them, with an extra 0,0 point at the bottom, and a 100000%,100000% point at the top.");
                VehicleExampleCurvePoint = listingStandard.SliderLabeled(
                    $"Play with this example slider to see the results. Original Rating: {VehicleExampleCurvePoint:P0}, Adjusted: {VehicleArmorCurve.Evaluate(VehicleExampleCurvePoint):P0}",
                    VehicleExampleCurvePoint, 0f, VehicleHeavyX * 2f);
                VehicleUnarmoredX = listingStandard.SliderLabeled($"Unarmored Vehicle Rating: {VehicleUnarmoredX:P0}",
                    VehicleUnarmoredX, min: 0f, 2f);
                VehicleUnarmoredY = listingStandard.SliderLabeled($"Unarmored Vehicle Scaled Value: {VehicleUnarmoredY:P0}",
                    VehicleUnarmoredY, min: 0f, 2f);
                VehicleLightX = listingStandard.SliderLabeled($"Light Vehicle Rating: {VehicleLightX:P0}",
                    VehicleLightX, min: VehicleUnarmoredX, VehicleMediumX);
                VehicleLightY = listingStandard.SliderLabeled($"Light Vehicle Scaled Value: {VehicleLightY:P0}",
                    VehicleLightY, min: VehicleUnarmoredY, VehicleMediumY);
                VehicleMediumX = listingStandard.SliderLabeled($"Medium Vehicle Rating: {VehicleMediumX:P0}",
                    VehicleMediumX, min: VehicleLightX, VehicleHeavyX);
                VehicleMediumY = listingStandard.SliderLabeled($"Medium Vehicle Scaled Value: {VehicleMediumY:P0}",
                    VehicleMediumY, min: VehicleLightY, VehicleHeavyY);
                VehicleHeavyX = listingStandard.SliderLabeled($"Heavy Vehicle Rating: {VehicleHeavyX:P0}",
                    VehicleHeavyX, min: VehicleMediumX, VehicleMediumX * 5f);
                VehicleHeavyY = listingStandard.SliderLabeled($"Heavy Vehicle Scaled Value: {VehicleHeavyY:P0}",
                    VehicleHeavyY, min: VehicleMediumY, VehicleMediumY * 5f);
                points.Clear();
                points.Add(new CurvePoint(0, 0));
                points.Add(new CurvePoint(VehicleUnarmoredX, VehicleUnarmoredY));
                points.Add(new CurvePoint(VehicleLightX, VehicleLightY));
                points.Add(new CurvePoint(VehicleMediumX, VehicleMediumY));
                points.Add(new CurvePoint(VehicleHeavyX, VehicleHeavyY));
                points.Add(new CurvePoint(1000, 1000));
                VehicleArmorCurve.SetPoints(points);
            }
            listingStandard.End();
            Widgets.EndScrollView();
            Rect rect2 = new Rect(inRect.width - 150f, inRect.height + 40f, 150f, 40f);
            if (Widgets.ButtonText(rect2, "Reset to Defaults", drawBackground: true, doMouseoverSound: false))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                Reset();
            }
        }
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ExtraLayerPercentage, "ExtraLayerPercentage", ExtraLayerPercentage, true);
            Scribe_Values.Look(ref ArmorMinDamage, "ArmorMinDamage", ArmorMinDamage, true);
            Scribe_Values.Look(ref ArmorPennedDamageMult, "ArmorPennedDamageMult", ArmorPennedDamageMult, true);
            Scribe_Values.Look(ref PrimitiveArmorGunResistance, "PrimitiveArmorGunResistance", PrimitiveArmorGunResistance, true);
            Scribe_Values.Look(ref MinDamagedArmorEffectiveness,"MinDamagedArmorEffectiveness", MinDamagedArmorEffectiveness, true);
            Scribe_Values.Look(ref HeatBluntTraumaActive,"HeatBluntTraumaActive", HeatBluntTraumaActive, true);
            Scribe_Values.Look(ref StoppingPowerBluntPenBonus,"StoppingPowerBluntPenBonus", StoppingPowerBluntPenBonus, true);
            Scribe_Values.Look(ref StoppingPowerBluntDamageBonus,"StoppingPowerBluntDamageBonus", StoppingPowerBluntDamageBonus, true);
            Scribe_Values.Look(ref MaxStoppingPower,"MaxStoppingPower", MaxStoppingPower, true);
            Scribe_Values.Look(ref BluntTraumaMultiplier,"BluntTraumaMultiplier", BluntTraumaMultiplier, true);
            Scribe_Values.Look(ref BluntTraumaPenMultiplier,"BluntTraumaPenMultiplier", BluntTraumaPenMultiplier, true);
            //Scribe_Values.Look(ref BuffMetalBluntMultiplier,"BuffMetalBluntMultiplier");
            Scribe_Values.Look(ref ArmorPenMultiplier,"ArmorPenMultiplier", ArmorPenMultiplier, true);
            Scribe_Values.Look(ref DamageMultiplier,"DamageMultiplier", DamageMultiplier, true);
            Scribe_Values.Look(ref MeleeAPMult,"MeleeAPMult", MeleeAPMult, true);
            Scribe_Values.Look(ref MeleeDamageMultiplier,"MeleeDamageMultiplier", MeleeDamageMultiplier, true);
            Scribe_Values.Look(ref RangedAPMult,"RangedAPMult", RangedAPMult, true);
            Scribe_Values.Look(ref RangedDamageMultiplier,"RangedDamageMultiplier", RangedDamageMultiplier, true);
            Scribe_Values.Look(ref MeleeArtificialStoppingPower,"MeleeArtificialStoppingPower", MeleeArtificialStoppingPower, true);
            Scribe_Values.Look(ref BasicFireAP,"BasicFireAP", BasicFireAP, true);
            Scribe_Values.Look(ref HardArmorRatio,"HardArmorRatio", HardArmorRatio, true);
            Scribe_Values.Look(ref SoftArmorPenaltyActive,"SoftArmorPenaltyActive", SoftArmorPenaltyActive, true);
            Scribe_Values.Look(ref HardArmorBonusMult,"HardArmorBonusMult", HardArmorBonusMult, true);
            Scribe_Values.Look(ref ArmorDamageMult, "ArmorDamageMult", ArmorDamageMult, true);
            Scribe_Values.Look(ref ArmorDestructionProtection, "ArmorDestructionProtection", ArmorDestructionProtection, true);
            Scribe_Values.Look(ref BluntTraumaPenMultiplier,  "BluntTraumaPenMultiplier", BluntTraumaPenMultiplier, true);
            Scribe_Values.Look(ref NaturalArmorHPMultiplier,"NaturalArmorHPMultiplier", NaturalArmorHPMultiplier, true);
            Scribe_Values.Look(ref PseudoLayersForHelmets, "PseudoLayersForHelmets", PseudoLayersForHelmets, true);
            Scribe_Values.Look(ref StoppingPowerBaseScaling, "StoppingPowerBaseScaling", StoppingPowerBaseScaling, true);
            Scribe_Values.Look(ref MinStoppingPower, "MinStoppingPower", MinStoppingPower, true);
            Scribe_Values.Look(ref NaturalArmorMaxHealthscale, "NaturalArmorMaxHealthscale", NaturalArmorMaxHealthscale, true);
            Scribe_Values.Look(ref VehicleArmorForPenetration, "VehicleArmorForPenetration", VehicleArmorForPenetration, true);
            Scribe_Values.Look(ref VehicleUnarmoredX,  "VehicleUnarmoredX", VehicleUnarmoredX, true);
            Scribe_Values.Look(ref VehicleUnarmoredY,  "VehicleUnarmoredY", VehicleUnarmoredY, true);
            Scribe_Values.Look(ref VehicleLightX,  "VehicleLightX", VehicleLightX, true);
            Scribe_Values.Look(ref VehicleLightY, "VehicleLightY", VehicleLightY, true);
            Scribe_Values.Look(ref VehicleMediumX, "VehicleMediumX", VehicleMediumX, true);
            Scribe_Values.Look(ref VehicleMediumY, "VehicleMediumY", VehicleMediumY, true);
            Scribe_Values.Look(ref VehicleHeavyX, "VehicleHeavyX", VehicleHeavyX, true);
            Scribe_Values.Look(ref VehicleHeavyY, "VehicleHeavyY", VehicleHeavyY, true);
            Scribe_Values.Look(ref VehiclePseudoHPMult, "VehiclePseudoHPMult", VehiclePseudoHPMult, true);
            
            base.ExposeData();
        }
    }