using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace VFECore.Abilities
{
    public class EquipmentAbilityDef : AbilityDef
    {
        public bool hasCooldown = true;
        public float cooldown = 0;
        public bool requirePsycast = false;
        public int requiredPsycastLevel = 0;
        public virtual string GetDescription()
        {
            var coolDesc = GetBasics();
            //    var AoEDesc = GetAoEDesc();
            //string postDesc = PostAbilityVerbDesc();
            var desc = new StringBuilder();
            desc.AppendLine(description);
            if (coolDesc != "") desc.AppendLine(coolDesc);
            //    if (AoEDesc != "") desc.AppendLine(AoEDesc);
            //if (postDesc != "") desc.AppendLine(postDesc);
            var result = desc.ToString();
            return result;
        }
        public string GetBasics()
        {
            var result = "";
            var def = verbProperties;
            if (def != null)
            {
                var s = new StringBuilder();
                s.AppendLine(StringsToTranslate.AU_Cooldown + this.cooldown.ToString("N0") + " " +
                             "SecondsLower".Translate());
                if (this.comps.Any(x => x.GetType() == typeof(CompProperties_EffectWithDest)))
                {
                    CompProperties_EffectWithDest withDest = (CompProperties_EffectWithDest)this.comps.Find(x => x.compClass == typeof(CompProperties_EffectWithDest));
                    if (this.verbProperties.defaultProjectile != null)
                    {
                        if (this.verbProperties.defaultProjectile.projectile.explosionRadius > 0)
                        {
                            s.AppendLine(StringsToTranslate.AU_Type + StringsToTranslate.AU_TargetAoE);
                        }
                        else
                        {
                            s.AppendLine(StringsToTranslate.AU_Type + StringsToTranslate.AU_TargetThing);
                        }
                    }
                    else
                    {
                        s.AppendLine(StringsToTranslate.AU_Type + StringsToTranslate.AU_TargetThing);
                    }
                }
                else
                {
                    s.AppendLine(StringsToTranslate.AU_Type + StringsToTranslate.AU_TargetSelf);
                }

                if (def.defaultProjectile != null)
                    if (def.defaultProjectile.projectile != null)
                        if (def.defaultProjectile.projectile.GetDamageAmount(1f) > 0)
                        {
                            s.AppendLine("Damage".Translate() + ": " +
                                         def.defaultProjectile.projectile.GetDamageAmount(1f));
                            s.AppendLine("Damage".Translate() + " " + StringsToTranslate.AU_Type +
                                         def.defaultProjectile.projectile.damageDef.LabelCap);
                        }

                if (this.comps.Any(x => x.GetType() == typeof(CompProperties_AbilityGiveMentalState)))
                {

                    List<CompProperties_AbilityGiveMentalState> mentalStatesToApply = new List<CompProperties_AbilityGiveMentalState>();
                    this.comps.FindAll(x => x.compClass == typeof(CompProperties_AbilityGiveMentalState)).ForEach(x => mentalStatesToApply.Add((CompProperties_AbilityGiveMentalState)x));
                    if (mentalStatesToApply != null && mentalStatesToApply.Count > 0)
                        if (mentalStatesToApply.Count == 1)
                        {
                            s.AppendLine(StringsToTranslate.AU_MentalStateChance + ": " +
                                         mentalStatesToApply[0].stateDef.LabelCap);
                        }
                        else
                        {
                            s.AppendLine(StringsToTranslate.AU_MentalStateChance);
                            foreach (var mentalState in mentalStatesToApply)
                                s.AppendLine("\t" + mentalState.stateDef.LabelCap);
                        }
                }

                if (this.comps.Any(x => x.GetType() == typeof(CompProperties_AbilityGiveHediff)))
                {
                    List<CompProperties_AbilityGiveHediff> hediffsToApply = new List<CompProperties_AbilityGiveHediff>();
                    this.comps.FindAll(x => x.compClass == typeof(CompProperties_AbilityGiveHediff)).ForEach(x => hediffsToApply.Add((CompProperties_AbilityGiveHediff)x));
                    if (hediffsToApply != null && hediffsToApply.Count > 0)
                        if (hediffsToApply.Count == 1)
                        {
                            s.AppendLine(StringsToTranslate.AU_EffectChance + hediffsToApply[0].hediffDef.LabelCap);
                        }
                        else
                        {
                            s.AppendLine(StringsToTranslate.AU_EffectChance);
                            foreach (var hediff in hediffsToApply)
                            {
                                float duration = 0;
                                if (hediff.hediffDef.comps != null)
                                    if (hediff.hediffDef.HasComp(typeof(HediffComp_Disappears)))
                                    {
                                        var intDuration =
                                        ((HediffCompProperties_Disappears)hediff.hediffDef.CompPropsFor(
                                            typeof(HediffComp_Disappears))).disappearsAfterTicks.max;
                                        duration = intDuration.TicksToSeconds();
                                    }
                                if (duration == 0)
                                    s.AppendLine("\t" + hediff.hediffDef.LabelCap);
                                else
                                    s.AppendLine("\t" + hediff.hediffDef.LabelCap + " " + " " + duration + " " +
                                                 "SecondsToLower".Translate());
                            }
                        }
                    if (def.burstShotCount > 1)
                        s.AppendLine(StringsToTranslate.AU_BurstShotCount + " " + def.burstShotCount);
                }
                result = s.ToString();
            }
            return result;
        }
    }
}