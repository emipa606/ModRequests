using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.Noise;

namespace BionicleKanohiMasksOfPower
{
	[StaticConstructorOnStartup]
	public class Command_Jump : Command_Action//create gizmo
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Miru apparel;
		public Command_Jump(Apparel_Miru apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTick > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTick;
				if (cooldownTicksRemaining < Apparel_Miru.JumpCooldownTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Miru.JumpCooldownTicks, 0, cooldownTicksRemaining);
					Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
				}
			}
			if (result.State == GizmoState.Interacted)
			{
				return result;
			}
			return new GizmoResult(result.State);
        }
	}
	public class Apparel_Miru : Apparel//apparel class
    {
		public int lastUsedTick;
		public const float EffectiveRange = 15f;
		public const int JumpCooldownTicks = 300;
		public static bool CanHitTargetFrom(Pawn caster, IntVec3 root, LocalTargetInfo targ)//check for line of sight
		{
			float num = EffectiveRange * EffectiveRange;
			IntVec3 cell = targ.Cell;
			if ((float)caster.Position.DistanceToSquared(cell) <= num)
			{
				return GenSight.LineOfSight(root, cell, caster.Map);
			}
			return false;
		}

		public static void DrawHighlight(Pawn caster, LocalTargetInfo target)//highlight outline
		{
			if (target.IsValid && ValidJumpTarget(caster.Map, target.Cell))
			{
				GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
			}
			GenDraw.DrawRadiusRing(caster.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(caster.Position, c, caster.Map) && ValidJumpTarget(caster.Map, c));
		}

		public static bool ValidJumpTarget(Map map, IntVec3 cell)//check landing spot
		{
			if (!cell.IsValid || !cell.InBounds(map))//check if cell is valid and inside the map
			{
				return false;
			}
			if (cell.Impassable(map) || !cell.Walkable(map) || cell.Fogged(map))// check if you can land there, is walkable, and is not obscured
			{
				return false;
			}
			Building edifice = cell.GetEdifice(map);
			Building_Door building_Door;
			if (edifice != null && (building_Door = edifice as Building_Door) != null && !building_Door.Open)// check if door is open to move through
			{
				return false;
			}
			return true;
		}

		public static TargetingParameters TargetingParameters(Pawn pawn)
		{
			return new TargetingParameters
			{
				canTargetLocations = true,
				canTargetPawns = false,
				canTargetBuildings = false,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && ValidJumpTarget(pawn.Map, x.Cell)//check for line of sight and is landing spot
			};
        }


		public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (var g in base.GetWornGizmos())
            {
                yield return g;
            }
            if (Wearer.IsColonistPlayerControlled && this.IsMasterworkOrLegendary())
            {
				yield return new Command_Jump(this)
				{
					defaultLabel = "Bionicle.Jump".Translate(),
					defaultDesc = "Bionicle.JumpDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							var map = Wearer.Map;
							Wearer.rotationTracker.FaceCell(localTargetInfo.Cell);//face direction of movement
							PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnJumper, Wearer, localTargetInfo.Cell);//change pawn into flyer pawn with target in mind
							GenSpawn.Spawn(pawnFlyer, localTargetInfo.Cell, map);//spawn flyer pawn onto map
							lastUsedTick = Find.TickManager.TicksGame;
						}, highlightAction: (LocalTargetInfo x) =>
						{
							GenDraw.DrawRadiusRing(Wearer.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(Wearer.Position, c, Wearer.Map) && ValidJumpTarget(Wearer.Map, c));
							DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					onHover = delegate
					{
						GenDraw.DrawRadiusRing(Wearer.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(Wearer.Position, c, Wearer.Map) && ValidJumpTarget(Wearer.Map, c));
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTick + Apparel_Miru.JumpCooldownTicks > Find.TickManager.TicksGame
				};
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref lastUsedTick, "lastUsedTick");
        }
    }
}