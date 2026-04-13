using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace BionicleKanohiMasksOfPower
{
	[StaticConstructorOnStartup]
	public class Command_Ruru : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Ruru apparel;
		public Command_Ruru(Apparel_Ruru apparel)
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
				if (cooldownTicksRemaining < Apparel_Ruru.CooldownTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Ruru.CooldownTicks, 0, cooldownTicksRemaining);
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
	public class Apparel_Ruru : Apparel
    {
		public int lastUsedTick;
		public const int CooldownTicks = 450;
		public const float EffectiveRange = 27f;
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
		public TargetingParameters TargetingParameters(Pawn pawn)//target pawn, not self targettable, pawn is not downed, used in stun, 27 range, line of sight check
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && x.Thing is Pawn other && other != pawn && !other.Downed
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
				yield return new Command_Ruru(this)
				{
					defaultLabel = "Bionicle.StunPawn".Translate(),
					defaultDesc = "Bionicle.StunPawnDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							localTargetInfo.Pawn.stances.stunner.StunFor(300, Wearer);
							lastUsedTick = Find.TickManager.TicksGame;
						}, highlightAction: (LocalTargetInfo x) =>
						{
							DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTick + Apparel_Ruru.CooldownTicks > Find.TickManager.TicksGame
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