// ******************************************************************
//       /\ /|       @file       RaidStrategyWorkerPoaching.cs
//       \ V/        @brief      袭击策略 偷猎
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-06-17 19:01:36
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SR.ModRimWorld.RaidExtension
{
    [UsedImplicitly]
    public class RaidStrategyWorkerPoaching : RaidStrategyWorker
    {
        public Pawn animal;
        public bool isSurprise = false;

        /// <summary>
        /// 该策略适用于
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="groupKind"></param>
        /// <returns></returns>
        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            return base.CanUseWith(parms, groupKind) && parms.faction != null && parms.faction != Faction.OfMechanoids;
        }

        public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption g, List<PawnGenOptionWithXenotype> chosenGroups, Faction faction = null)
        {
            if( !g.kind.RaceProps.Humanlike )
                return false;
            // Unfortunately CanUsePawn() cannot filter out pawns with poor weapons on its own,
            // because it's called repeatedly until it either succeeds or until it's ignored.
            // So if the pawn kind allows only poor weapons, one will be selected in the end anyway.
            // Try to filter out such pawn kinds, even if guessing just based on weapon tags doesn't work very well.
            bool possiblyHasSuitable = false;
            foreach( string weaponTag in g.kind.weaponTags )
            {
                if( !weaponTag.Contains( "Grenade" ) && !weaponTag.Contains( "Melee" )
                    && !weaponTag.Contains( "GunSingleUse" ) && !weaponTag.Contains( "Flame" )
                    && !weaponTag.Contains( "Tox" ))
                {
                    possiblyHasSuitable = true;
                    break;
                }
            }
            if( !possiblyHasSuitable )
                return false;
            return base.CanUsePawnGenOption( pointsTotal, g, chosenGroups, faction );
        }

        public override bool CanUsePawn(float pointsTotal, Pawn p, List<Pawn> otherPawns)
        {
            if( !base.CanUsePawn(pointsTotal, p, otherPawns))
                return false;
            // Avoid poorly suitable weapons (melee, non-lethal, too dangerous).
            ThingDef weaponDef = p.equipment.Primary?.def;
            if( weaponDef == null )
                return false;
            if( !weaponDef.IsRangedWeapon )
                return false;
            if( weaponDef.weaponTags.Contains("GunSingleUse")) // Dangerous guns such as rocket launchers.
                return false;
            DamageDef damage = p.equipment.PrimaryEq.PrimaryVerb.GetDamageDef();
            if( damage == DamageDefOf.ToxGas
                || damage == DamageDefOf.Smoke
                || damage == DamageDefOf.NerveStun
                || damage == DamageDefOf.EMP
                || damage == DamageDefOf.Stun
                || damage == DamageDefOf.Flame
                || damage == DamageDefOf.Bomb)
            {
                return false;
            }
            if( !HasReasonableShooting( p ))
                return false;
            return true;
        }

        // Pretty much SappersUtility.CanMineReasonablyFast().
        private bool HasReasonableShooting(Pawn p)
        {
            if (p.RaceProps.Humanlike && !p.skills.GetSkill(SkillDefOf.Shooting).TotallyDisabled && !StatDefOf.ShootingAccuracyPawn.Worker.IsDisabledFor(p))
            {
                // At most 7% accuracy loss per cell, which is still rather a poor accuracy (healthy baseliner with shooting skill of 2),
                // but somewhat difficult to achieve e.g. for neandearthals, so filter out at least
                // the awful shots.
                return p.GetStatValue(StatDefOf.ShootingAccuracyPawn) > 0.93f;
            }
            return false;
        }

        // HACK: Some things related to the incident need to be done late, finding the animal needs
        // parms.spawnCenter, but that one is resolved fairly late, after calculating raid points.
        // This function is called after that, so as a hack override this to do late setup.
        // Also used to carry over the information decided here to the lordjob (it appears it's not easily
        // possible to decide some additional info in Resolve* functions and than pass it to the lord job,
        // so it needs a hack one way or another.
        public override List<Pawn> SpawnThreats(IncidentParms parms)
        {
            ResolveLateInfo(parms);
            return base.SpawnThreats(parms);
        }

        private void ResolveLateInfo(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                Log.Error($"{MiscDef.LogTag}target must be a map.");
                return;
            }
            //设置狩猎目标
            animal = map.FindTargetAnimal(parms.spawnCenter, MiscDef.MinTargetRequireHealthScale);
            if (animal == null)
            {
                Log.Warning($"{MiscDef.LogTag}can't find any animal.");
                return;
            }

            isSurprise = this is RaidStrategyWorkerPoachingSurprise;
            if( isSurprise )
                parms.points *= 0.8f;
            else
            {
                // If the target is either a colony's animal or it is within the colony,
                // reduce the raid size somewhat (as in that case it is essentially a direct raid
                // with no delay and all raiders grouped up, which is harder to defend against).
                float factor = 1f;
                if( animal.Faction != null && animal.Faction == Faction.OfPlayer )
                    factor = 0.8f;
                if( animal.Position.InBounds( animal.Map ) && animal.Map.areaManager.Home[ animal.Position ] )
                    factor = 0.8f;
                parms.points *= factor;
            }
            parms.points = Math.Clamp( parms.points, MiscDef.MinThreatPoints, MiscDef.MaxThreatPoints );
        }

        /// <summary>
        /// 创建集群AI工作
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="map"></param>
        /// <param name="pawns"></param>
        /// <param name="raidSeed"></param>
        /// <returns></returns>
        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            var siegePositionFrom =
                RCellFinder.FindSiegePositionFrom(parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld,
                    map);
            return new LordJobPoaching(siegePositionFrom, animal, isSurprise);
        }
    }

    public class RaidStrategyWorkerPoachingSurprise : RaidStrategyWorkerPoaching
    {
    }
}
