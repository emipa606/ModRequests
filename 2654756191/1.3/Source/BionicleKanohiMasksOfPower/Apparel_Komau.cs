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
	public class Command_Down : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Komau apparel;
		public Command_Down(Apparel_Komau apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickDown > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickDown;
				if (cooldownTicksRemaining < Apparel_Komau.CooldownDownTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Komau.CooldownDownTicks, 0, cooldownTicksRemaining);
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

	[StaticConstructorOnStartup]
	public class Command_Tame : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Komau apparel;
		public Command_Tame(Apparel_Komau apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickTame > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickTame;
				if (cooldownTicksRemaining < Apparel_Komau.CooldownTameTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Komau.CooldownTameTicks, 0, cooldownTicksRemaining);
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

	[StaticConstructorOnStartup]
	public class Command_MentalState : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Komau apparel;
		public Command_MentalState(Apparel_Komau apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickMentalState > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickMentalState;
				if (cooldownTicksRemaining < Apparel_Komau.CooldownMentalStateTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Komau.CooldownMentalStateTicks, 0, cooldownTicksRemaining);
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

	public class Apparel_Komau : Apparel
    {
		public int lastUsedTickDown;
		public int lastUsedTickMentalState;
        public int lastUsedTickTame;
        public const int CooldownDownTicks = 1800;
        public const int CooldownMentalStateTicks = 900;
		public const int CooldownTameTicks = 60000;
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
		public TargetingParameters TargetingParameters(Pawn pawn)//target pawn, not self targettable, pawn is not downed, used in down and mental state, 27 range, line of sight check
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && x.Thing is Pawn other && other != pawn && !other.Downed
			};
		}
		public TargetingParameters TargetingParametersAnimal(Pawn pawn)//target wild pawn or animal, not same faction, used in tame, 27 range, line of sight check
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && x.Thing is Pawn other && other.AnimalOrWildMan() && other.Faction != pawn.Faction
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
				yield return new Command_Down(this)
				{
					defaultLabel = "Bionicle.DownPawn".Translate(),
					defaultDesc = "Bionicle.DownPawnDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							var hediff = HediffMaker.MakeHediff(BionicleDefOf.BKMOP_MakeDown, localTargetInfo.Pawn);
							localTargetInfo.Pawn.health.AddHediff(hediff);
							lastUsedTickDown = Find.TickManager.TicksGame;
						}, highlightAction: (LocalTargetInfo x) =>
						{
							DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickDown + Apparel_Komau.CooldownDownTicks > Find.TickManager.TicksGame
				};
				yield return new Command_Tame(this)
				{
					defaultLabel = "Bionicle.Tame".Translate(),
					defaultDesc = "Bionicle.TameDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParametersAnimal(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							InteractionWorker_RecruitAttempt.DoRecruit(Wearer, localTargetInfo.Pawn);
							lastUsedTickTame = Find.TickManager.TicksGame;
						}, highlightAction: (LocalTargetInfo x) =>
						{
							DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickTame + Apparel_Komau.CooldownTameTicks > Find.TickManager.TicksGame
				};
				yield return new Command_MentalState(this)
				{
					defaultLabel = "Bionicle.ChangeMentalState".Translate(),
					defaultDesc = "Bionicle.ChangeMentalStateDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							var pawn = localTargetInfo.Pawn;
							if (pawn.MentalState != null && (pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria)==false))
                            {
								pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
							}
                            else
                            {
								pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
                            }
							lastUsedTickMentalState = Find.TickManager.TicksGame;
						}, highlightAction: (LocalTargetInfo x) =>
						{
							DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickMentalState + Apparel_Komau.CooldownMentalStateTicks > Find.TickManager.TicksGame
				};
			}
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref lastUsedTickDown, "lastUsedTickDown");
			Scribe_Values.Look(ref lastUsedTickTame, "lastUsedTickTame");
			Scribe_Values.Look(ref lastUsedTickMentalState, "lastUsedTickMentalState");
		}
    }
}