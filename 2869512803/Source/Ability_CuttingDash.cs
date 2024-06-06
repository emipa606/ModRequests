using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using Command_Ability = VFECore.Abilities.Command_Ability;

namespace VFECore.Abilities
{
    public class Ability_CuttingDash : Ability
    {
		
		
        public override bool CanHitTarget(LocalTargetInfo target) => target.Cell.WalkableBy(pawn.Map, pawn) && target.Cell.DistanceTo(pawn.Position) <= 20f; //Traverse.Create(this).Field<float>("range").Value;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var map = pawn.Map;
            var position = pawn.Position;
            var destCell = targets[0].Cell;
            AbilityPawnFlyer flyer = (PawnFlyer_KyokatanaDash) PawnFlyer.MakeFlyer(WP_DefOf.WP_CuttingDashFlyer, pawn, destCell);
            flyer.ability = this;
            flyer.target = destCell.ToVector3();
            GenSpawn.Spawn(flyer, destCell, map);
        }
    }

    public class PawnFlyer_KyokatanaDash : AbilityPawnFlyer
    {
		public List<string> HitPawns = new List<string>();
		//need to erase the list on landing/starting flight
        public override Vector3 DrawPos => position;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var a = Mathf.Max(Traverse.Create(this).Field<float>("flightDistance").Value, 1f) / 70; //70 is flight speed 
                ticksFlightTime = a.SecondsToTicks();
                ticksFlying = 0;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            FlyingPawn.Drawer.renderer.RenderPawnAt(position, direction);
        }

        public override void Tick()
        {
			SearchForTargets(position.ToIntVec3(), 2.9f, Map, FlyingPawn);
            base.Tick();
        }
		
		public void SearchForTargets(IntVec3 center, float radius, Map map, Pawn pawn)
        {       
            IEnumerable<IntVec3> targets = GenRadial.RadialCellsAround(center, radius, true);
            foreach (IntVec3 curCell in targets)
            {
                if (curCell.InBounds(map) && curCell.IsValid)
                {
                    Pawn victim = curCell.GetFirstPawn(map);
                    if (victim != null && victim.Faction != pawn.Faction)
                    {
						if (!(HitPawns.Contains(victim.GetUniqueLoadID())))
                        {
							damageEntities(victim, null, 15, WP_DefOf.WP_WhiteHotBurn);           
							HitPawns.Add(victim.GetUniqueLoadID());	//we store the ID of everything hit so that we won't hit twice (or, more accurately, once each tick)
						}
                    }
                }
            }
        }
		
		public void damageEntities(Pawn victim, BodyPartRecord hitPart, int amt, DamageDef type)
        {
            DamageInfo dinfo;
            dinfo = new DamageInfo(type, amt, 0, (float)-1, this.FlyingPawn, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown);
            dinfo.SetAllowDamagePropagation(false);
            victim.TakeDamage(dinfo);
        }
    }
}