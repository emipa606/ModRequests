using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UniversalTaxidermy
{
	public class Thing_TaxidermyMount : ThingWithComps, EstupendoBase.IDisplayable, IThingHolder, IThingHolderWithDrawnPawn
	{
		public ThingDef_TaxidermyMount Def => this.Def as ThingDef_TaxidermyMount;

		public const float          IngredientMassFactor		= 0.8f;
		public const float          ButcheryFractionReturned	= 0.1f;
		public const float          ConstantImpactOffset		= 2f;

		private float               IngredientFactor = 0f;
		private List<string>        IngredientBreakdown;
		private ThingOwner<Pawn>    Pawns;
		private CompArt             InnerArt;
		private bool                InnerIsDisplayed;
		private bool                RendererResolved = false;

		public CompArt GetArt() => InnerArt ?? ( InnerArt = this.GetComp<CompArt>() );
		public Pawn HeldPawn => Pawns.Count > 0 ? Pawns[0] : null;
		public PawnRenderer DisplayRenderer => HeldPawn?.Drawer.renderer;
		public Vector3 DisplayHeadOffset(Rot4 rot) => HeldPawn.Drawer.renderer.BaseHeadOffsetAt(rot);
		public bool WantsToRetain => true;
		public bool PermitThing(Thing t) => false;
		public bool IsDisplayed { get { return InnerIsDisplayed; } set { InnerIsDisplayed = value; } }

		public Thing_TaxidermyMount() : base()
		{
			IngredientBreakdown = new List<string>();
			Pawns = new ThingOwner<Pawn>(this, true, LookMode.Deep);
			RendererResolved = false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Pawns.ExposeData();
			RendererResolved = false;
		}

		public void ReportDebugInfo()
		{
			string info = "Debug Info for " + Label + "\r\n\r\n";

			if( HeldPawn == null )
			{
				info += "HeldPawn: null\r\n";
			} else
			{
				info += "HeldPawn: " + HeldPawn.Label + "\r\n";
			}

			if( GetArt() == null )
			{
				info += "Art: null\r\n";
			} else
			{
				info += "InnerArt: " + InnerArt.Title + "\r\n";
			}

			info += "Displayed: " + IsDisplayed.ToString() + "\r\n";

			if( DisplayRenderer == null )
			{
				info += "DisplayRenderer: null\r\n"; 
			} else
			{
				info += "DisplayRenderer: present\r\n";
				info += "Renderer Graphics: " + ( DisplayRenderer.graphics == null ? "null" : "present" ) + "\r\n";
				info += "Naked Graphics: " + ( DisplayRenderer.graphics.nakedGraphic == null ? "null" : "present" ) + "\r\n";
			}


			Find.LetterStack.ReceiveLetter("Debug Info", info, LetterDefOf.NeutralEvent);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if( HeldPawn == null )
			{   // Some alternate spawn type, like drop pods or dev mode.
				PawnKindDef kind = DefDatabase<PawnKindDef>.AllDefs.Where(pkd => (pkd.race.race.Humanlike == false) && (pkd.race.race.IsMechanoid == false)).RandomElement();
				PawnGenerationRequest pgr = new PawnGenerationRequest(kind, Faction.OfAncients, PawnGenerationContext.NonPlayer, map.Tile, allowDead: true, forceDead: true);

				try
				{
					Pawn newPawn = (Pawn)ThingMaker.MakeThing(kind.race);
					newPawn.kindDef = kind;
					newPawn.SetFactionDirect(Faction.OfAncients);
					PawnComponentsUtility.CreateInitialComponents(newPawn);
					newPawn.gender = newPawn.RaceProps.hasGenders ? ( Rand.Value < 0.5f ? Gender.Male : Gender.Female ) : Gender.None;
					newPawn.ageTracker.AgeBiologicalTicks = (long)( newPawn.RaceProps.lifeStageAges.Last().minAge * 3600000f );
					newPawn.ageTracker.AgeChronologicalTicks = newPawn.ageTracker.AgeBiologicalTicks;

					newPawn.Kill(null);
					RegisterIngredients(new List<Thing>(){newPawn}).ToList();
				} catch (Exception ex)
				{
					Log.Error("Exception taxidermying pawn!\r\n" + ex.Message);
					return;
				}

			}
		}

		public IEnumerable<Thing> RegisterIngredient(Thing t)
		{
			Pawn pawn = null;

			if( t is Corpse c )
			{
				pawn = c.InnerPawn;
			}
			else if( t is Pawn p )
			{
				pawn = p;
			}

			if( pawn != null )
			{
				pawn.Rotation = Rot4.West;
				pawn.health.hediffSet.Clear();
				Utility.MakeTaleFor(pawn);
				Pawns.TryAddOrTransfer(pawn);

				IEnumerable<Thing> extras = pawn.ButcherProducts(null, ButcheryFractionReturned);
				foreach( Thing extra in extras )
				{
					yield return extra;
				}
			}

			yield break;
		}
		public IEnumerable<Thing> RegisterIngredients(IEnumerable<Thing> ts)
		{
			foreach( Thing t in ts )
			{
				IEnumerable<Thing> extras = RegisterIngredient(t);
				foreach( Thing extra in extras )
				{
					yield return extra;
				}
			}
			if( HeldPawn != null ) GetArt().InitializeArt(HeldPawn);
			yield break;
		}

		
		public string TransformPlaqueLabel(string OldLabel)
		{
			return this.Label.Replace("Mount", "Display");
		}

		public override string Label => (GetArt().Active ? GetArt().Title : (HeldPawn?.Label ?? base.Label));	

		public IEnumerable<Dialog_InfoCard.Hyperlink> GetLinks()
		{
			foreach( Pawn p in Pawns )
			{
				yield return new Dialog_InfoCard.Hyperlink(p.def);
			}
			yield break;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			yield return new StatDrawEntry(EstupendoBase.Defs.EstupendoDefs.Estupendo_DisplayStats, "Impact", "x" + GetImpactFactor().ToString("F2") + " + " + GetImpactOffset().ToString("F2"), GetImpactBreakdown(), 1300, null, GetLinks(), false);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach(Gizmo g in base.GetGizmos() )
			{
				yield return g;
			}

			if( Prefs.DevMode )
			{
				yield return new Command_Action
				{
					defaultLabel = "Debug Info",
					defaultDesc = "Get the downlow.",
					icon = ContentFinder<Texture2D>.Get("things/unknown", true),
					action = ReportDebugInfo
				};

				yield return new Command_Action
				{
					defaultLabel = "Resolve Graphics",
					defaultDesc = "Resolve graphics!",
					icon = ContentFinder<Texture2D>.Get("things/unknown", true),
					action = delegate () { DisplayRenderer.graphics.ResolveAllGraphics(); }
				};
			}
			
		}

		#region Graphics
		public override void Print(SectionLayer layer)
		{
			if( this.StoringThing() is EstupendoBase.IDisplay display )
			{

			}
			else
			{
				base.Print(layer);
				PrintGraphic(DisplayRenderer, layer, DrawPos, new Vector2(1.1f, 1.1f), Rot4.West);
			}
		}

		public void DrawOnDisplay(Vector3 loc, Rot4 rot)
		{
			DrawGraphic(DisplayRenderer, loc, Vector2.negativeInfinity, rot);
		}

		public override void Draw()
		{
			DrawGraphic(DisplayRenderer, DrawPos, RotatedSize.ToVector2(), Rot4.West);
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			DrawGraphic(DisplayRenderer, drawLoc, RotatedSize.ToVector2(), Rot4.West);
		}

		public void PrintOnDisplay(SectionLayer layer, Vector3 loc, Rot4 orientation)
		{
			PrintGraphic(DisplayRenderer, layer, loc, DisplayRenderer.graphics.nakedGraphic.drawSize, orientation.Rotated(RotationDirection.Counterclockwise));
		}

		private void PrintGraphic(PawnRenderer renderer, SectionLayer layer, Vector3 pos, Vector2 target_size, Rot4 orientation)
		{
			if( !RendererResolved )
			{
				RendererResolved = true;
				DisplayRenderer?.graphics?.ResolveAllGraphics();
			}

			if( renderer == null || renderer.graphics == null || renderer.graphics.nakedGraphic == null ) return;
			Graphic graphic = renderer.graphics.nakedGraphic;
			float scale = 1f;

			if( target_size.x > 0f && target_size.y > 0f )
			{
				scale = Mathf.Min(target_size.x / graphic.drawSize.x, target_size.y / graphic.drawSize.y);
			}

			Vector3 scaled = new Vector3(scale, 1f, scale);
			Matrix4x4 pos_matrix = default(Matrix4x4);
			pos_matrix.SetTRS(pos + Vector3.up, Rot4.North.AsQuat, scaled);

			Printer_Mesh.PrintMesh(layer, pos_matrix, graphic.MeshAt(orientation), graphic.MatAt(orientation));
			if( renderer.graphics.headGraphic != null )
			{
				Vector3 HeadOffset = renderer.BaseHeadOffsetAt(orientation);
				pos_matrix.SetTRS(pos + Vector3.up + (HeadOffset * scale), Rot4.North.AsQuat, scaled);
				Printer_Mesh.PrintMesh(layer, pos_matrix, MeshPool.humanlikeHeadSet.MeshAt(orientation), renderer.graphics.headGraphic.MatAt(orientation));
			}
		}

		private void DrawGraphic(PawnRenderer renderer, Vector3 pos, Vector2 target_size, Rot4 orientation)
		{
			if( !RendererResolved )
			{
				RendererResolved = true;
				DisplayRenderer?.graphics?.ResolveAllGraphics();
			}

			if( HeldPawn != null && HeldPawn.RaceProps.Humanlike )
			{
				renderer.RenderPawnAt(pos, orientation);
				return;
			}

			if( renderer == null || renderer.graphics == null || renderer.graphics.nakedGraphic == null ) return;
			Graphic graphic = renderer.graphics.nakedGraphic;
			float scale = 1f;

			if( target_size.x > 0f && target_size.y > 0f )
			{
				scale = Mathf.Min(target_size.x / graphic.drawSize.x, target_size.y / graphic.drawSize.y);
			}

			Vector3		scaled		= new Vector3(scale, 1f, scale);
			Matrix4x4	pos_matrix	= default(Matrix4x4);
			pos_matrix.SetTRS(pos + Vector3.up, Rot4.North.AsQuat, new Vector3(scale, 1f, scale));

			Graphics.DrawMesh(graphic.MeshAt(orientation), pos_matrix, graphic.MatAt(orientation), 0);
			if( renderer.graphics.headGraphic != null )
			{
				Vector3 HeadOffset = DisplayHeadOffset(orientation);
				pos_matrix.SetTRS(pos + Vector3.up + ( HeadOffset * scale ), Rot4.North.AsQuat, scaled);
				Graphics.DrawMesh(MeshPool.humanlikeHeadSet.MeshAt(orientation), pos_matrix, renderer.graphics.headGraphic.MatAt(orientation), 0);
			}
		}
		#endregion

		#region Displayable

		public float GetImpactOffset() => 0f;
		public float GetImpactFactor()
		{
			return GetIngredientFactor() + ConstantImpactOffset;
		}

		public string GetImpactBreakdown()
		{
			return "Taxidermy: x" + GetImpactFactor().ToString("F2") + "\r\n" + string.Join("\r\n\t", IngredientBreakdown);
		}

		public float GetExtraMass()
		{
			float ret = 0f;
			foreach( Pawn p in Pawns )
			{
				ret += p.GetStatValue(StatDefOf.Mass, true) * IngredientMassFactor;
			}
			return ret;
		}
		public float GetIngredientFactor()
		{
			IngredientFactor = 0f;
			IngredientBreakdown.Clear();
			foreach( Pawn ingredient in Pawns )
			{
				IngredientBreakdown.Add(ingredient.Label);
				IngredientFactor += GetIngredientFactor(ingredient.RaceProps.AnyPawnKind);
				IngredientBreakdown.Add("\tBeauty (" + ingredient.RaceProps.leatherDef?.label + "): +" + ingredient.RaceProps.leatherDef?.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Beauty).ToString("F2"));
				IngredientBreakdown.Add("\tBody Size: x" + ingredient.BodySize.ToString("F2"));
			}
			return IngredientFactor;
		}

		public float GetIngredientFactor(PawnKindDef ingredient)
		{
			return ingredient.RaceProps.baseBodySize * ( ingredient.RaceProps.leatherDef?.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.Beauty) ?? 1f );
		}

		public IntVec2 GetRequiredDisplaySize()
		{
			IntVec2 ret = IntVec2.One;
			foreach( Pawn pawn in Pawns )
			{
				int width = (int)Mathf.Floor(pawn.BodySize - 1);
				int height = (int)Mathf.Floor(pawn.BodySize / 2);
				if( width > ret.x ) ret.x = width;
				if( height > ret.z ) ret.z = height;
			}
			return ret;
		}

		#endregion

		#region Holding
		public float HeldPawnDrawPos_Y => AltitudeLayer.ItemImportant.AltitudeFor();
		public float HeldPawnBodyAngle => Rot4.North.AsAngle;
		public PawnPosture HeldPawnPosture => PawnPosture.Standing;
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}
		public ThingOwner GetDirectlyHeldThings() => Pawns;
		#endregion
	}

}
