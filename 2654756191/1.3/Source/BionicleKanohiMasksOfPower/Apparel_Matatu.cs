using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.AI.Group;
using Verse.Noise;
//using VFECore.Abilities;
using static UnityEngine.GraphicsBuffer;

namespace BionicleKanohiMasksOfPower
{
	[StaticConstructorOnStartup]
	public class Command_Disarm : Command_Action//disarm ability command action redefined for a second command action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Matatu apparel;
		public Command_Disarm(Apparel_Matatu apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickDisarm > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickDisarm;
				if (cooldownTicksRemaining < Apparel_Matatu.CooldownDisarmTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Matatu.CooldownDisarmTicks, 0, cooldownTicksRemaining);
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

	/*
	[StaticConstructorOnStartup]
	public class Command_WallRaise : Command_Action//wallraise ability
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Matatu apparel;
		public Command_WallRaise(Apparel_Matatu apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickWallRaise > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickWallRaise;
				if (cooldownTicksRemaining < Apparel_Matatu.CooldownWallRaise)
				{
					float num = Mathf.InverseLerp(Apparel_Matatu.CooldownWallRaise, 0, cooldownTicksRemaining);
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
*/
	public class Apparel_Matatu : Apparel //apparel class
	{
		public int lastUsedTickDisarm;
		//public int lastUsedTickWallRaise;
		public const float EffectiveRange = 27f;
		public const int CooldownDisarmTicks = 600;
		//public const int CooldownWallRaise = 900;
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
		public TargetingParameters TargetingParameters(Pawn pawn)//target pawn, not self targettable, pawn is not downed, used in disarm, 27 range, line of sight check
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && x.Thing is Pawn other && other != pawn && !other.Downed && pawn.Position.DistanceTo(other.Position) <= 27f
			};
		}
		public TargetingParameters TargetingParametersPawn(Pawn pawn)//target pawn, not self targettable, 27 tile from origin, used for pawn in pull, los check
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && x.Thing is Pawn other && other != pawn && pawn.Position.DistanceTo(other.Position) <= 27f
			};
		}

		public TargetingParameters TargetingParametersTeleport(Pawn pawn)//target walkable tile, 27 tile from origin, used for pulled pawn target, los check
		{
			return new TargetingParameters
			{
				canTargetLocations = true,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && x.Cell.Walkable(pawn.Map) && x.Cell.DistanceTo(pawn.Position) <= 27f
			};
		}
		public TargetingParameters TargetingParametersCell(Pawn pawn)//target dependednt on "canapplyon" class, used to target location to spawn walls
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => CanApplyOn(new LocalTargetInfo(x.Cell))
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
				yield return new Command_Disarm(this)
				{
					defaultLabel = "Bionicle.DisarmPawn".Translate(),
					defaultDesc = "Bionicle.DisarmPawnDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)//targetting of pawn, target saved as localTargetInfo
						{
							var equipment = localTargetInfo.Pawn.equipment?.Primary;//determine equipped weapon
							if (equipment != null)//check if no weapon
							{
								localTargetInfo.Pawn.equipment.TryDropEquipment(equipment, out _, localTargetInfo.Pawn.Position);//pawn drop weapon
								lastUsedTickDisarm = Find.TickManager.TicksGame;
							}
						}, highlightAction: (LocalTargetInfo x) =>
						{
							DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickDisarm + Apparel_Matatu.CooldownDisarmTicks > Find.TickManager.TicksGame//set ability as disabled by comparing cooldown ticks+ last used tick to global
				};
/*
				yield return new Command_WallRaise(this)
				{
					defaultLabel = "Bionicle.WallRaise".Translate(),
					defaultDesc = "Bionicle.WallRaiseDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParametersCell(Wearer), delegate (LocalTargetInfo localTargetInfo)//target location, saved to localTargetInfo
						{
							Apply(localTargetInfo);//activate walls being summoned
							lastUsedTickWallRaise = Find.TickManager.TicksGame;
						}, highlightAction: (LocalTargetInfo x) => DrawEffectPreview(x), null, Wearer);
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickWallRaise + Apparel_Matatu.CooldownWallRaise > Find.TickManager.TicksGame
				};
*/
				yield return new Command_Action
				{
					defaultLabel = "Bionicle.Pull".Translate(),
					defaultDesc = "Bionicle.PullDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParametersPawn(Wearer), delegate (LocalTargetInfo target)//select pawn to move, save as target
						{
							Find.Targeter.BeginTargeting(TargetingParametersTeleport(target.Pawn), delegate (LocalTargetInfo dest)// select location to move pawn target, save to dest
							{
								var map = Wearer.Map;
                                //if (target.Pawn != null && target.Pawn.Faction != Wearer.Faction)
                                //{
                                //    target.Pawn.stances.stunner.StunFor(600, Wearer, addBattleLog: false, showMote: true);//stun pawn for 60 ticks
                                //}
                                Pawn pawn2 = target.Pawn;
								//IntVec3 destination = dest.Cell + ((pawn2.Position - dest.Cell).ToVector3().normalized * 2).ToIntVec3();//set selected location as destination
								//AbilityPawnFlyer flyer = (AbilityPawnFlyer)PawnFlyer.MakeFlyer(VFE_DefOf_Abilities.VFEA_AbilityFlyer, pawn2, dest.Cell);//change pawn into flyer pawn with target in mind
								//flyer.target = destination.ToVector3();
								//GenSpawn.Spawn(flyer, target.Cell, map);//spawn flyer pawn onto map
								PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnJumper, pawn2, dest.Cell);//change pawn into flyer pawn with target in mind
								GenSpawn.Spawn(pawnFlyer, dest.Cell, map);//spawn flyer pawn onto map
							}, highlightAction: (LocalTargetInfo x) =>
							{
								DrawHighlight(target.Pawn, x);
							}, null, Wearer);
						}, highlightAction: (LocalTargetInfo x) =>
						{
							DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					icon = this.def.uiIcon,
				};
			}
		}
		/*
		public void Apply(LocalTargetInfo target)//spawns walls
		{
			Map map = Wearer.Map;
			List<Thing> list = new List<Thing>();
			list.AddRange(AffectedCells(target, map).SelectMany((IntVec3 c) => from t in c.GetThingList(map)
																			   where t.def.category == ThingCategory.Item
																			   select t));
			foreach (Thing item in list)
			{
				item.DeSpawn();
			}
			foreach (IntVec3 item2 in AffectedCells(target, map))
			{
				GenSpawn.Spawn(ThingDefOf.RaisedRocks, item2, map);
				FleckMaker.ThrowDustPuffThick(item2.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), CompAbilityEffect_Wallraise.DustColor);
			}
			foreach (Thing item3 in list)
			{
				IntVec3 intVec = IntVec3.Invalid;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 intVec2 = item3.Position + GenRadial.RadialPattern[i];
					if (intVec2.InBounds(map) && intVec2.Walkable(map) && map.thingGrid.ThingsListAtFast(intVec2).Count <= 0)
					{
						intVec = intVec2;
						break;
					}
				}
				if (intVec != IntVec3.Invalid)
				{
					GenSpawn.Spawn(item3, intVec, map);
				}
				else
				{
					GenPlace.TryPlaceThing(item3, item3.Position, map, ThingPlaceMode.Near);
				}
			}
		}
*/
		public bool CanApplyOn(LocalTargetInfo target)//used for wallraise
		{
			return Valid(target, throwMessages: false);
		}

		public void DrawEffectPreview(LocalTargetInfo target)//used for wallraise
		{
			GenDraw.DrawFieldEdges(AffectedCells(target, Wearer.Map).ToList(), Valid(target) ? Color.white : Color.red);
		}

		private static List<IntVec2> pattern = new List<IntVec2>
		{
			new IntVec2(0, 0), new IntVec2(1, 0), new IntVec2(-1, 0), new IntVec2(0, 1), new IntVec2(0, -1)
		};
		private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
		{
			foreach (IntVec2 item in pattern)
			{
				IntVec3 intVec = target.Cell + new IntVec3(item.x, 0, item.z);
				if (intVec.InBounds(map))
				{
					yield return intVec;
				}
			}
		}

		public bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			if (AffectedCells(target, Wearer.Map).Any((IntVec3 c) => c.Filled(Wearer.Map)))
			{
				return false;
			}
			if (AffectedCells(target, Wearer.Map).Any((IntVec3 c) => !c.Standable(Wearer.Map)))
			{
				if (throwMessages)
				{
					Messages.Message("AbilityUnwalkable".Translate(Wearer.def.LabelCap), target.ToTargetInfo(Wearer.Map), MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}
	
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref lastUsedTickDisarm, "lastUsedTickDisarm");
		//	Scribe_Values.Look(ref lastUsedTickWallRaise, "lastUsedTickWallRaise");
		}
	}
}