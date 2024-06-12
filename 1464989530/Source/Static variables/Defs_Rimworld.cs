// Nightvision NightVision Constants.cs
// 
// 03 08 2018
// 
// 16 10 2018

using RimWorld;
using Verse;

namespace NightVision
{
    /// <summary>
    /// Various vanilla rimworld defs
    /// </summary>
    public static class Defs_Rimworld
    {
        // relevant body parts
        public static readonly BodyPartTagDef EyeTag = BodyPartTagDefOf.SightSource;
        public static readonly BodyPartGroupDef Eyes = BodyPartGroupDefOf.Eyes;
        public static readonly BodyPartGroupDef Head = BodyPartGroupDefOf.FullHead;

        // skills and stats affected by night vision
        public static readonly SkillDef ShootSkill = SkillDefOf.Shooting;
        public static readonly SkillDef MeleeSkill = SkillDefOf.Melee;
        public static readonly StatDef MeleeHitStat = StatDefOf.MeleeHitChance;
        public static readonly StatDef MeleeDodgeStat = StatDefOf.MeleeDodgeChance;

        // parent category for stat reports
        public static readonly StatCategoryDef BasicStats = StatCategoryDefOf.Basics;

        // for the night vision incident
        public static readonly GameConditionDef SolarFlare = GameConditionDefOf.SolarFlare;
        public static readonly ThingDef ShieldDef = ThingDef.Named("Apparel_ShieldBelt");
        public static readonly PawnGroupKindDef CombatGroup = PawnGroupKindDefOf.Combat;

    }

}