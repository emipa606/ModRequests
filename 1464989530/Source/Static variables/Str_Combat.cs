// Nightvision NightVision Str_Combat.cs
// 
// 23 10 2018
// 
// 23 10 2018

using Verse;

namespace NightVision
{
    /// <summary>
    /// Combat strings defined in xml file in nightvision/languages
    /// </summary>
    public static class Str_Combat
    {


        public const char NumSpace = '\u2007';

        public static readonly string LMDef = "NightVisionLMDef".Translate();

        public static readonly string AnimalAndMechNote = "NightVisionAnimalAndMechNote".Translate();

        public static readonly string HitChanceTitle = "NightVisionHitChanceTitle".Translate();

        public static readonly string SurpriseAttackTitle = "NightVisionSurpriseAtkTitle".Translate();

        public static readonly string DodgeTitle = "NightVisionDodgeTitle".Translate();

        public static string HitChanceHeader() => $" {"NightVisionTarget".Translate()}:   0% {"Lit".Translate()}".PadLeft(40, NumSpace) + $"  |  100% {"Lit".Translate()}";
        public static string ShootTargetAtGlow()
        {
            return "NightVisionShootTargetAtGlow".Translate();
        }


        public static string ShotChanceTransform(float distance, float hitChance, float nvResult, float psResult)
        {
            return
                $"{"distance".Translate().CapitalizeFirst()} {distance.ToString().PadLeft(2, '\u2007')}, {hitChance.ToStringPercent("F1")} ==>".PadLeft(30, NumSpace) + $"{nvResult.ToStringPercent("F1").PadLeft(7, NumSpace)}  |  {psResult.ToStringPercent("F1")}";
        }

        public static string StrikeChanceTransform(float hitChance, float nvResult, float psResult)
        {
            return $"{hitChance.ToStringPercent("F1")}  ==>".PadLeft(28, NumSpace) + $"{nvResult.ToStringPercent("F1").PadLeft(7, NumSpace)}  |  {psResult.ToStringPercent("F1")}";
        }

        public static string StrikeTargetAtGlow()
        {
            return "NightVisionStrikeTargetAtGlow".Translate();
        }

        public static string SurpriseAtkChance()
        {
            return "NightVisionSurpriseAtkChance".Translate();
        }

        public static string SurpriseAtkDesc()
        {
            return "NightVisionSurpriseAtkDesc".Translate();
        }

        public static string SurpriseAtkCalcHeader()
        {
#if RW10
            return "NightVisionOurLM".Translate().PadLeft(20, NumSpace) + "  |" + "NightVisionTargetLM".Translate().PadLeft(12, NumSpace) + "  |" + "NightVisionSurpriseAtkSh".Translate().PadLeft(10, NumSpace);
#else
return "NightVisionOurLM".Translate().RawText.PadLeft(20, NumSpace) + "  |" + "NightVisionTargetLM".Translate().RawText.PadLeft(12, NumSpace) + "  |" + "NightVisionSurpriseAtkSh".Translate().RawText.PadLeft(10, NumSpace);
#endif
        }

        public static string SurpriseAtkCalcRow(float glow, float atkGlowF, float defGlowF, float chance)
        {
            return
                $"{glow.ToStringPercent("F0").PadLeft(5, NumSpace)} {"Lit".Translate()}:{atkGlowF.ToStringPercent().PadLeft(10, NumSpace)}  |{defGlowF.ToStringPercent().PadLeft(10, NumSpace)}  |{chance.ToStringPercent().PadLeft(6, NumSpace)}";
        }

        public static string Dodge()
        {
            return "NightVisionDodge".Translate();
        }

        public static string DodgeCalcHeader()
        {
#if RW10
            return "NightVisionOurLM".Translate().PadLeft(20, NumSpace) + "  |" + "NightVisionAtkLM".Translate().PadLeft(12, NumSpace) + "  |" + "NightVisionDodgeChanceShort".Translate().PadLeft(10, NumSpace);
#else
return "NightVisionOurLM".Translate().RawText.PadLeft(20, NumSpace) + "  |" + "NightVisionAtkLM".Translate().RawText.PadLeft(12, NumSpace) + "  |" + "NightVisionDodgeChanceShort".Translate().RawText.PadLeft(10, NumSpace);
#endif
        }

        public static string DodgeCalcRow(float glow, float atkGlowF, float defGlowF, float dodge, float newDodge)
        {


            return
                $"{glow.ToStringPercent().PadLeft(5, NumSpace)} {"Lit".Translate()}:{defGlowF.ToStringPercent().PadLeft(10, NumSpace)}  |{atkGlowF.ToStringPercent().PadLeft(10, NumSpace)}  |{dodge.ToStringPercent("F1").PadLeft(6, NumSpace)} ==> {newDodge.ToStringPercent("F1")}";
        }

        public static string AimFactorFromLight(float glowAtTarget, float result)
        {
            return "NightVisionAimFactorFromLight".Translate(arg1: glowAtTarget.ToStringPercent(), arg2: result.ToStringPercent()).CapitalizeFirst();
        }

        public static string RangedCooldown(float glow, int skill, float result)
        {
            return "NightVisionRangedCooldown".Translate(arg1: glow.ToStringPercent(), arg2: skill, arg3: result.ToStringPercent());
        }

        public static string RangedCooldownDemo(float glow, float result)
        {
            return "NightVisionRangedCooldownDemo".Translate(arg1: glow.ToStringPercent(), arg2: result.ToStringPercent());
        }
    }
}