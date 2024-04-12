using RimWorld;
using Verse;

namespace FriendlyFireTweaks
{
    public static class Utility
    {
        internal static int GetHitChance(Thing shooter)
        {
            return GetHitChance(GetShootingLevel(shooter));
        }

        internal static int GetHitChance(int shootingSkill, bool overrideSettings = false)
        {
            int hitChance = 0;
            
            if (Main.config.ShootingLevelAffectsFriendlyFireChance || overrideSettings)
                hitChance = 100 - shootingSkill * Main.config.PercentBonus;

            if (hitChance <= 0)
                hitChance = 0;
            
            return hitChance;
        }

        internal static int GetShootingLevel(Thing shooter)
        {
            int shootingLevel;
            if (shooter is Pawn pawn)
            {
                if (pawn.skills is null) //Potential fix for #9
                {
                    shootingLevel = 20;
                }
                else
                {
                    shootingLevel = pawn.skills.GetSkill(SkillDefOf.Shooting).Level;
                }
            }
            else if (shooter.IsBuilding())
            {
                shootingLevel = 20;
            }
            else
            {
                shootingLevel = 0;
            }

            return shootingLevel;
        }

        internal static bool ProtectedThingType(Thing target)
        {
            return
                Main.config.ProtectPawns && target.IsPawn() ||
                Main.config.ProtectAnimals && target.IsAnimal() ||
                Main.config.ProtectBuildings && target.IsBuilding() ||
                Main.config.ProtectMineables && target.IsMineable();
        }

        internal static bool ProtectedFaction(Thing shooter, Thing target)
        {
            if (target.Faction is null || shooter.Faction is null)
                return true;
            
            if (shooter.Faction != target.Faction)
            {
                return shooter.Faction.RelationKindWith(target.Faction) != FactionRelationKind.Hostile;
            }
            else
            {
                return true;
            }
        }
    }
}