// Nightvision NightVision StatReportFor_NightVision_Combat.cs
// 
// 25 10 2018
// 
// 06 12 2018

using System.Text;
using RimWorld;
using Verse;

namespace NightVision
{
    public static class StatReportFor_NightVision_Combat
    {
        private class CombatStatStrHelper
        {
            private static StringBuilder sb;
            public CombatStatStrHelper()
            {

#if RW10

                sb = new StringBuilder();

#else
                if (sb == null)
                {
                    sb = new StringBuilder();
                }
                else
                {
                    sb.Clear();
                }
#endif
            }

            public void NextLine()
            {
                sb.AppendLine();
            }

            public void AddLine(string line)
            {
                sb.AppendLine(line);
            }

            
            public void AddMainHeader()
            {
                // TODO move hit chance parts to a different function - see violence disabled pawns
                AddLine(Str_Combat.LMDef);
                AddLine(Str_Combat.AnimalAndMechNote);
                NextLine();
                AddLine(Str_Combat.HitChanceTitle.PadLeft(totalWidth: 20, paddingChar: '-').PadRight(totalWidth: 30, paddingChar: '-'));
                AddLine(Str_Combat.HitChanceHeader());

            }

            public void AddSurpriseAttackHeader()
            {
                AddLine(
                    Str_Combat.SurpriseAttackTitle.PadLeft(totalWidth: 20, paddingChar: '-').PadRight(totalWidth: 30, paddingChar: '-')
                );

                AddLine(Str_Combat.SurpriseAtkDesc());
                AddLine(Str_Combat.SurpriseAtkChance());
                NextLine();
                AddLine(Str_Combat.SurpriseAtkCalcHeader());
            }

            public void AddSurpriseAttackRow(float glow, float atkLM, float defLM)
            {
                float chance = CombatHelpers.SurpriseAttackChance(atkLM, defGlowFactor: defLM);
                AddLine(Str_Combat.SurpriseAtkCalcRow(glow, atkLM, defLM, chance));
            }

            public void AddDodgeHeader()
            {
                AddLine( Str_Combat.DodgeTitle.PadLeft(totalWidth: 20, paddingChar: '-').PadRight(totalWidth: 30, paddingChar: '-')
                );

                AddLine( Str_Combat.Dodge());
                NextLine();

                AddLine(Str_Combat.DodgeCalcHeader());

            }

            public void AddDodgeRow(float glow, float atkLM, float defLM, float baseDodge)
            {
                AddLine(Str_Combat.DodgeCalcRow(glow, atkLM, defLM, baseDodge, CombatHelpers.DodgeChanceFunction(baseDodge, atkLM-defLM)));
            }

            public void AddRangedCdRow(float glow, int skill, float glowFactor)
            {
                AddLine(Str_Combat.RangedCooldown(glow, skill, CombatHelpers.RangedCooldownMultiplier(skill, glowFactor)));
            }

            public void AddRangedCdDemo(float glow, int skill, float glowFactor)
            {
                AddLine(Str_Combat.RangedCooldownDemo(glow, CombatHelpers.RangedCooldownMultiplier(skill, glowFactor)));
            }


            public override string ToString()
            {
                return sb.ToString();
            }
        }

        public static bool ShowMeleeEffectsForPawn(Pawn pawn)
        {
            if (!Settings.CombatStore.MeleeHitEffectsEnabled.Value)
            {
                return false;
            }
            if (pawn?.story != null)
            {
                return !pawn.skills.GetSkill(SkillDefOf.Melee).TotallyDisabled;
            }
            else
            {
                return true;
            }
        }

        public static bool ShowRangedEffectsForPawn(Pawn pawn)
        {
            
            if (pawn?.story != null)
            {
                return !pawn.skills.GetSkill(SkillDefOf.Shooting).TotallyDisabled;
            }
            else
            {
                return true;
            }
        }

        public static string CombatPart(Pawn pawn, Comp_NightVision comp)
        {

            var strHelper = new CombatStatStrHelper();
            const float NoLight = 0f;
            const float FullLight = 1f;
            float noLightFactor = comp.FactorFromGlow(glow: NoLight);
            float fullLightFactor = comp.FactorFromGlow(glow: FullLight);


            strHelper.AddMainHeader();

            if (Settings.CombatStore.RangedHitEffectsEnabled.Value)
            {
                if(ShowRangedEffectsForPawn(pawn))
                {
                    strHelper.AddLine(Str_Combat.ShootTargetAtGlow());


                    for (var i = 1; i <= 4; i++)
                    {
                        float hit = ShotReport.HitFactorFromShooter(caster: pawn, distance: i * 5);

                        strHelper.AddLine(
                            Str_Combat.ShotChanceTransform(
                                distance: i * 5,
                                hitChance: hit,
                                nvResult: CombatHelpers.HitChanceGlowTransform(hitChance: hit,
                                    attGlowFactor: noLightFactor),
                                psResult: CombatHelpers.HitChanceGlowTransform(hitChance: hit,
                                    attGlowFactor: fullLightFactor)
                            )
                        );
                    }

                    strHelper.NextLine();
                }
                else
                {
                    // TODO add line reporting why effects are not appearing
                }
            }

            if (Settings.CombatStore.MeleeHitEffectsEnabled.Value)
            {


                if (ShowMeleeEffectsForPawn(pawn))
                {
                    float pawnDodgeVal = pawn.GetStatValue(stat: Defs_Rimworld.MeleeDodgeStat);
                    float meleeHit = pawn.GetStatValue(stat: Defs_Rimworld.MeleeHitStat, applyPostProcess: true);

                    var caps = Settings.Store.MultiplierCaps;

                    strHelper.AddLine(Str_Combat.StrikeTargetAtGlow());


                    strHelper.AddLine(
                        Str_Combat.StrikeChanceTransform(
                            hitChance: meleeHit,
                            nvResult: CombatHelpers.HitChanceGlowTransform(hitChance: meleeHit,
                                attGlowFactor: noLightFactor),
                            psResult: CombatHelpers.HitChanceGlowTransform(hitChance: meleeHit,
                                attGlowFactor: fullLightFactor)
                        )
                    );

                    strHelper.NextLine();

                    strHelper.AddSurpriseAttackHeader();

                    ///////////////////////////////////////////
                    //// Surprise attack stats at 0% light ////

                    float noLightSurpAttChance =
                        CombatHelpers.SurpriseAttackChance(atkGlowFactor: noLightFactor, defGlowFactor: caps.min);
                    // attack vs pawn with minimum LM
                    strHelper.AddSurpriseAttackRow(NoLight, noLightFactor, caps.min);

                    // skip if we need more room to show dodge stats
                    if (pawnDodgeVal.IsTrivial())
                    {
                        //skip if chance was 0% vs pawn with min LM (as it won't be different
                        if (noLightSurpAttChance.IsNonTrivial())
                        {
                            // attack vs pawn with normal LM
                            noLightSurpAttChance =
                                CombatHelpers.SurpriseAttackChance(atkGlowFactor: noLightFactor, defGlowFactor: 1f);

                            strHelper.AddSurpriseAttackRow(NoLight, noLightFactor, 1);

                            // skip as above
                            if (noLightSurpAttChance.IsNonTrivial())
                            {
                                // attack vs pawn with max LM
                                strHelper.AddSurpriseAttackRow(0, noLightFactor, caps.max);
                            }
                        }
                    }
                    ////////////////////////////////////////////

                    /////////////////////////////////////////////
                    //// Surprise attack stats at 100% light ////

                    // skip if we need more room to show dodge stats
                    if (pawnDodgeVal.IsTrivial())
                    {
                        // attack vs pawn with min LM
                        strHelper.AddSurpriseAttackRow(fullLightFactor, fullLightFactor, caps.min);
                    }

                    // attack vs pawn with normal LM
                    strHelper.AddSurpriseAttackRow(fullLightFactor, fullLightFactor, 1f);
                    strHelper.NextLine();
                    /////////////////////////////////////////////

                    /////////////////////////////////////////////
                    ////             Dodge Stats             ////


                    strHelper.AddDodgeHeader();

                    // This pawns chance to dodge when attacked

                    // attacked by pawn with min LM in no light
                    strHelper.AddDodgeRow(NoLight, caps.min, noLightFactor, pawnDodgeVal);

                    // skip if pawns dodge value is zero
                    if (pawnDodgeVal.IsNonTrivial())
                    {
                        // attacked by pawn with normal LM in no light
                        strHelper.AddDodgeRow(NoLight, 1f, noLightFactor, pawnDodgeVal);

                        // attacked by pawn with max LM in no light
                        strHelper.AddDodgeRow(NoLight, caps.max, noLightFactor, pawnDodgeVal);

                        // attacked by pawn with min LM in no light
                        strHelper.AddDodgeRow(FullLight, caps.min, fullLightFactor, pawnDodgeVal);


                    }

                    //attacked by pawn with normal LM in full light
                    strHelper.AddDodgeRow(FullLight, 1, fullLightFactor, pawnDodgeVal);
                }
            }

            return strHelper.ToString();
        }

        public static string RangedCoolDown(Pawn pawn, int skillLevel)
        {
            var strHelper = new CombatStatStrHelper();
            float glow = GlowFor.GlowAt(thing: pawn);
            float glowFactor = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

            strHelper.AddRangedCdRow(glow, skillLevel, glowFactor);

            glow = 1f;
            glowFactor = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

            strHelper.AddRangedCdRow(glow, skillLevel, glowFactor);

            glow = 0f;
            glowFactor = GlowFor.FactorOrFallBack(pawn: pawn, glow: glow);

            strHelper.AddRangedCdRow(glow, skillLevel, glowFactor);



            return strHelper.ToString();
        }




    }
}