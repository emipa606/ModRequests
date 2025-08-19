using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArmoredUp
{
    [DefOf]
    public static class ArmoredUpDefOf
    {
        public static HediffDef BatteredArmor;
        static ArmoredUpDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ArmoredUpDefOf));
        }
        
    }
    
    [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyDamageToPart")]
    public static class ArmoredUpArmorPatching
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo oldGetPostArmorDamage = AccessTools.Method(typeof(ArmorUtility), "GetPostArmorDamage");
            MethodInfo newGetPostArmorDamage = AccessTools.Method(typeof(ArmoredUpArmorPatching), "AUGetPostArmorDamage");
            MethodInfo damageInfoGetAmount = AccessTools.Method(typeof(DamageInfo), "get_Amount");
            bool found = false;
            bool found2 = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(oldGetPostArmorDamage))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, newGetPostArmorDamage);
                    found = true;
                }
                else if (found && !found2 && instruction.Calls(damageInfoGetAmount))
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0.5f);
                    yield return new CodeInstruction(OpCodes.Mul);
                    found2 = true;
                }
                else
                    yield return instruction;
                
            }
            if (!found)
                Log.Error("Armored Up could not apply patch to GetPostArmorDamage");
            
        }

        private static void AUApplyArmor(ref float damAmount, ref float armorPenetration, float armorRating,
            float armorRatingBlunt,
            Thing armorThing, ref DamageDef damageDef, Pawn pawn, out bool metalArmor, DamageInfo dinfo,
            ref float bluntTraumaDamage, ref float bluntTraumaAP)
	    {
            bool isWornArmor = armorThing != null;
            ArmorUtils.SetMetalArmor(armorThing, pawn, out metalArmor, isWornArmor);
            
            if (armorRating < 0.001f && armorRatingBlunt < 0.001f)
            {
                return;
            }
            
            float bluntDamage = 0f;
            float bluntPen = 0.001f;
            StatDef armorRatingStat = damageDef.armorCategory.armorRatingStat;
            float armorBluntRatingEffective = armorRatingBlunt;
            float effectiveArmorRating = armorRating;
            
            //Industrial and above ranged weapons reduce the armor rating of medieval and below armor. Might include things that aren't guns but oh well.
            if (ArmorUtils.IsPrimitiveVsGuns(armorThing, damageDef, dinfo, isWornArmor))
            {
                effectiveArmorRating *= ArmoredUp.settings.PrimitiveArmorGunResistance;
                armorBluntRatingEffective *= ArmoredUp.settings.PrimitiveArmorGunResistance;
            }
            
            float effectiveArmorPen = Mathf.Max(armorPenetration, 0.001f);
            
            float blockPercent = Mathf.Min(1f, effectiveArmorRating / effectiveArmorPen);
            bool isFullBlock = blockPercent >= 1f;

            // Damage worn armor; adjust for armor destruction
            if (isWornArmor && armorThing.def.useHitPoints && armorRating > 0.001f)
            {
                float armorDamage = damAmount + bluntTraumaDamage;
                //Armor damage reduced if AR > AP, increased if the armor is penned
                if (isFullBlock)
                {
                    armorDamage *= Mathf.Max(ArmoredUp.settings.ArmorMinDamage, effectiveArmorPen / effectiveArmorRating);
                }
                else
                {
                    armorDamage *= Mathf.Max(ArmoredUp.settings.ArmorMinDamage,blockPercent) * ArmoredUp.settings.ArmorPennedDamageMult;
                }
                
                armorDamage *= ArmoredUp.settings.ArmorDamageMult;
                // If the armor doesn't have enough HP, it blocks only as much as it can.
                // Corrects for the case of block% going below 100, and thus even less blockage.
                if (armorDamage >= armorThing.HitPoints)
                {
                    blockPercent *= armorThing.HitPoints / armorDamage;
                    if (isFullBlock)
                        blockPercent /= ArmoredUp.settings.ArmorPennedDamageMult;
                    
                    if (ArmoredUp.settings.ArmorDestructionProtection)
                        armorDamage = Mathf.Max(0f, armorThing.HitPoints - 1f);
                    //else if (BigAndSmallCompatibility.BigAndSmallActive)
                    else
                        armorDamage = armorThing.HitPoints + 1;
                }
                
                armorThing.TakeDamage(new DamageInfo(damageDef, GenMath.RoundRandom(armorDamage)));
            }
            else if (armorRating > 0.001f)
            {
                Hediff battered = pawn.health.hediffSet.GetFirstHediffOfDef(ArmoredUpDefOf.BatteredArmor);
                float batteredSeverity = battered?.Severity ?? 0f;
                float armorDamage = damAmount + bluntTraumaDamage;
                float threshold = Mathf.Max(50f, 150f * Mathf.Min(pawn.HealthScale, ArmoredUp.settings.NaturalArmorMaxHealthscale));
                float pseudoArmorHP = threshold * ArmoredUp.settings.NaturalArmorHPMultiplier;
                //Log.Message($"Armor Damage: {armorDamage}, damAmt: {damAmount}, BT Dam: {bluntTraumaDamage}, PseudoHP: {pseudoArmorHP}, Threshold: {threshold}, ArmorPen: {effectiveArmorPen}, ArmorRating: {effectiveArmorRating}");
                
                if (isFullBlock)
                {
                    armorDamage *= Mathf.Max(ArmoredUp.settings.ArmorMinDamage, effectiveArmorPen / effectiveArmorRating);
                }
                else
                {
                    armorDamage *= blockPercent * ArmoredUp.settings.ArmorPennedDamageMult;
                }

                armorDamage *= ArmoredUp.settings.ArmorDamageMult;
                float pseudoArmorCurrentHP = pseudoArmorHP * (1f - batteredSeverity);
                if (armorDamage >= pseudoArmorCurrentHP)
                {
                    blockPercent *= pseudoArmorCurrentHP / armorDamage;
                    if (isFullBlock && blockPercent < 1f)
                    {
                        blockPercent /= ArmoredUp.settings.ArmorPennedDamageMult;
                    }
                    armorDamage = pseudoArmorCurrentHP + 1;
                }
                //Convert from damage to a severity increase, by converting to % of health
                armorDamage /= pseudoArmorHP;


                if (battered == null)
                {
                    battered = HediffMaker.MakeHediff(ArmoredUpDefOf.BatteredArmor, pawn);
                    battered.Severity = Mathf.Clamp01(armorDamage);
                    pawn.health.AddHediff(battered);
                }
                else
                {
                    battered.Severity = Mathf.Clamp01(batteredSeverity + armorDamage);
                }
            }

            // Get the blunt trauma from blocked sharp (and heat, if enabled)
            if (ArmorUtils.IsBluntTraumaSouce(damageDef, armorRatingStat))
            {
                bluntDamage = damAmount * blockPercent * ArmoredUp.settings.BluntTraumaMultiplier;
                bluntPen = effectiveArmorPen * blockPercent * ArmoredUp.settings.BluntTraumaPenMultiplier;
                float stoppingPower = ArmorUtils.GetStoppingPower(damageDef, dinfo);
                if (stoppingPower <= 0f)
                    stoppingPower = 0.5f;
                if (stoppingPower > ArmoredUp.settings.MinStoppingPower)
                {
                    bluntPen *= stoppingPower > ArmoredUp.settings.StoppingPowerBaseScaling ? 
                        (stoppingPower - ArmoredUp.settings.StoppingPowerBaseScaling) * ArmoredUp.settings.StoppingPowerBluntPenBonus + ArmoredUp.settings.StoppingPowerBaseScaling 
                        : stoppingPower;
                    bluntDamage *= stoppingPower > ArmoredUp.settings.StoppingPowerBaseScaling ? 
                        (stoppingPower - ArmoredUp.settings.StoppingPowerBaseScaling) * ArmoredUp.settings.StoppingPowerBluntDamageBonus + ArmoredUp.settings.StoppingPowerBaseScaling 
                        : stoppingPower;
                }
                
            }

            // If we have existing blunt trauma hitting this armor layer, weighted average it in with new blunt trauma.
            if (bluntTraumaDamage > 0.001f)
            {
                if (bluntDamage > 0.001f)
                {
                    float traumaAPWeight = bluntTraumaDamage / (bluntTraumaDamage + bluntDamage);
                    bluntPen = Mathf.Max(0.001f, traumaAPWeight * bluntTraumaAP + bluntPen * (1f - traumaAPWeight));
                }
                else
                    bluntPen = bluntTraumaAP;
                bluntDamage += bluntTraumaDamage;
            }

            //Reduce blunt trauma damage if it exists
            float bluntBlockedPercent = Mathf.Clamp01(armorBluntRatingEffective / bluntPen);
            bluntDamage *= 1f - bluntBlockedPercent;
            bluntPen *= 1f - bluntBlockedPercent;
            
            damAmount *= 1f - blockPercent;
            armorPenetration *= 1f - blockPercent; 
            bluntTraumaDamage = bluntDamage;
            bluntTraumaAP = bluntPen;

        }

        public static float AUGetPostArmorDamage(Pawn pawn, float amount, float armorPenetrationOriginal, BodyPartRecord part, ref DamageDef damageDef, out bool deflectedByMetalArmor, out bool diminishedByMetalArmor, DamageInfo dinfo)
        {
            deflectedByMetalArmor = false;
            diminishedByMetalArmor = false;
            float bluntTraumaDamage = 0f;
            float bluntTraumaAP = 0f;
            float armorPenetration = armorPenetrationOriginal;
            bool metalArmor = false;

            if (float.IsInfinity(armorPenetrationOriginal) || armorPenetrationOriginal > float.MaxValue * 0.001f || damageDef.armorCategory == null)
                return amount;
            
            StatDef armorRatingStat = damageDef.armorCategory.armorRatingStat;
            if (armorPenetrationOriginal <0.001f && armorRatingStat == StatDefOf.ArmorRating_Heat)
                armorPenetration = ArmoredUp.settings.BasicFireAP;
            
            if (pawn.apparel != null)
            {
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int num = wornApparel.Count - 1; num >= 0; num--)
                {
                    Apparel apparel = wornApparel[num];
                    if (!apparel.def.apparel.CoversBodyPart(part))
                        continue;
                    
                    float preArmor = amount;
                    AUApplyArmor(ref amount, ref armorPenetration, apparel.GetStatValue(armorRatingStat), apparel.GetStatValue(StatDefOf.ArmorRating_Blunt), apparel, ref damageDef, pawn, out metalArmor, dinfo, ref bluntTraumaDamage, ref bluntTraumaAP);

                    if (amount < 0.001f)
                    {
                        deflectedByMetalArmor = metalArmor;
                        if (bluntTraumaDamage < 0.001f)
                            return 0f;
                        //if there's no damage left, but blunt left, convert it to a normal blunt hit and process normally
                        damageDef = DamageDefOf.Blunt;
                        amount = bluntTraumaDamage;
                        armorPenetration = bluntTraumaAP;
                        bluntTraumaDamage = 0f;
                        bluntTraumaAP = 0f;
                    }
                    if (amount < preArmor && metalArmor)
                    {
                        diminishedByMetalArmor = true;
                    }
                }
            }

            float preNaturalArmor = amount;
            AUApplyArmor(ref amount, ref armorPenetration,  pawn.GetStatValue(armorRatingStat), pawn.GetStatValue(StatDefOf.ArmorRating_Blunt), null, ref damageDef, pawn, out metalArmor, dinfo, ref bluntTraumaDamage, ref bluntTraumaAP);
            if (amount < 0.001f)
            {
                deflectedByMetalArmor = metalArmor;
                if (bluntTraumaDamage < 0.001f)
                    return 0f;
                damageDef = DamageDefOf.Blunt;
                amount = bluntTraumaDamage;
                bluntTraumaDamage = 0f;
            }

            if (amount < preNaturalArmor && metalArmor)
            {
                diminishedByMetalArmor = true;
            }
            
            if (bluntTraumaDamage > 0.001f)
            {
                //do a blunt trauma to 'em
                //Log.Message("Doing a hurt! Amount: " + bluntTraumaDamage + ", weapon: " + (dinfo.Weapon?.ToString() ?? "none") + ", instigator: " + (dinfo.Instigator?.ToString() ?? "none"));
                if (damageDef.isRanged && dinfo.Weapon != null)
                {
                    BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(dinfo.Instigator, pawn, dinfo.IntendedTarget, dinfo.Weapon, dinfo.Weapon.projectileWhenLoaded, null);
                    Find.BattleLog.Add(battleLogEntry_RangedImpact);
                    pawn.TakeDamage(ArmorUtils.BluntTraumatizeTheDInfo(dinfo, bluntTraumaDamage, bluntTraumaAP)).AssociateWithLog(battleLogEntry_RangedImpact);
                }
                else
                    pawn.TakeDamage(ArmorUtils.BluntTraumatizeTheDInfo(dinfo, bluntTraumaDamage, bluntTraumaAP));
            }
            return amount < 0.001f ? 0f : amount;
        }
    }

}
