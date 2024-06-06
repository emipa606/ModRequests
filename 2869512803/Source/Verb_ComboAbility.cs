using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFECore.Abilities
{

    public class Ability_Combo : Ability
    {
		private int StrikesSoFar;
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
			Pawn targetPawn = targets[0].Thing as Pawn;
			//betweenPawnsDrawPos.x = (this.pawn.DrawPos.x + targetPawn.DrawPos.x)/2;
			//betweenPawnsDrawPos.y = (this.pawn.DrawPos.y + targetPawn.DrawPos.y)/2;
			//betweenPawnsDrawPos.z = (this.pawn.DrawPos.z + targetPawn.DrawPos.z)/2;
			//MoteThrown moteThrown = null;
			if (StrikesSoFar < 5)
			{
				//moteThrown = (MoteThrown)ThingMaker.MakeThing(WarcasketPersonaDefOf.MoteComboSlash, null);
				FleckMaker.Static(targets[0].Cell, targets[0].Map, WP_DefOf.MoteComboSlash, 3f);
			}
			else
			{
				//moteThrown = (MoteThrown)ThingMaker.MakeThing(WarcasketPersonaDefOf.MoteComboBlast, null);
				FleckMaker.Static(targets[0].Cell, targets[0].Map, WP_DefOf.MoteComboBlast, 3f);
			}
			//moteThrown.Scale = 3f;
			//moteThrown.rotationRate = 0;
			//moteThrown.exactPosition = targetPawn.DrawPos;
			//GenSpawn.Spawn(moteThrown, targetPawn.DrawPos, targetPawn.Map, WipeMode.Vanish);
			//second argument is probably IntVec3 instead of Vector3...
			StrikesSoFar++;	
        }
    
		/*private void SpawnOn(GlobalTargetInfo target)
		{
			if (!fleckDefs.NullOrEmpty())
				for (var i = 0; i < fleckDefs.Count; i++)
					SpawnFleck(target, fleckDefs[i]);

			if (fleckDef != null) SpawnFleck(target, fleckDef);

			sound?.PlayOneShot(target.HasThing ? target.Thing : new TargetInfo(target.Cell, target.Map));
		}

		private void SpawnFleck(GlobalTargetInfo target, FleckDef def)
		{
			if (target.HasThing)
				FleckMaker.AttachedOverlay(target.Thing, def, Vector3.zero, scale);
			else
				FleckMaker.Static(target.Cell, target.Map, def, scale);
		}*/
	}
}