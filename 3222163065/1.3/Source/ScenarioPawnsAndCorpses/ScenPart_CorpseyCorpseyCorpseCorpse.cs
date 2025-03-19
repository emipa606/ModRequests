using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// Base class for ScenParts that generate corpses
	/// </summary>
	public abstract class ScenPart_CorpseyCorpseyCorpseCorpse : ScenPart
	{
		/// <summary>
		/// Type of pawn to create a corpse of
		/// </summary>
		protected PawnKindDef pawnKindDef;

		/// <summary>
		/// How many corpses to create
		/// </summary>
		protected int count = 1;

		/// <summary>
		/// String representation of the count field
		/// </summary>
		protected string countBuf;

		/// <summary>
		/// Potential random defaults for initially populating the corpse count field
		/// </summary>
		private static readonly List<Pair<int, float>> CorpseCountChances = new List<Pair<int, float>>
		{
			new Pair<int, float>(1, 5f),
			new Pair<int, float>(2, 20f),
			new Pair<int, float>(3, 10f),
			new Pair<int, float>(4, 3f),
			new Pair<int, float>(5, 1f),
		};

		/// <summary>
		/// Save corpse info (count, type) to use it across save/load boundaries
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref this.count, "count", 0);
			Scribe_Defs.Look(ref this.pawnKindDef, "corpseKind");
		}

		/// <summary>
		/// Setup the UI for the part in the scenario edit interface
		/// </summary>
		/// <param name="listing">The list host for the scen part</param>
		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(scenPartRect.TopHalf());
			listing_Standard.ColumnWidth = scenPartRect.width;
			listing_Standard.TextFieldNumeric(ref this.count, ref this.countBuf, 1f);
			listing_Standard.End();
			if (!Widgets.ButtonText(scenPartRect.BottomHalf(), this.CurrentCorpseLabel().CapitalizeFirst()))
			{
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("ScenarioPawnsAndCorpses.ScenPart_RandomCorpseType".Translate().CapitalizeFirst(), delegate
			{
				this.pawnKindDef = null;
			}));
			foreach (PawnKindDef item in this.PossibleCorpses())
			{
				PawnKindDef localKind = item;
				list.Add(new FloatMenuOption("ScenarioPawnsAndCorpses.ScenPart_CorpseType".Translate(localKind.label).CapitalizeFirst(), delegate
				{
					this.pawnKindDef = localKind;
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		/// <summary>
		/// The possible corpses: basically any pawn kind def in the game
		/// </summary>
		/// <returns>the list of pawnkinddefs for the possible corpse</returns>
		protected IEnumerable<PawnKindDef> PossibleCorpses()
		{
			return DefDatabase<PawnKindDef>.AllDefs;
		}

		/// <summary>
		/// Name for the corpse to be added to the UI
		/// </summary>
		/// <returns>the string representation for the corpse</returns>
		protected string CurrentCorpseLabel()
		{
			if (this.pawnKindDef == null)
			{
				return "ScenarioPawnsAndCorpses.ScenPart_RandomCorpseType".TranslateSimple();
			}

			return "ScenarioPawnsAndCorpses.ScenPart_CorpseType".Translate(this.pawnKindDef.label).CapitalizeFirst();
		}

		/// <summary>
		/// Setup defaults for a new scenpart
		/// </summary>
		public override void Randomize()
		{
			if (Rand.Value < 0.5f)
			{
				this.pawnKindDef = null;
			}
			else
			{
				this.pawnKindDef = this.PossibleCorpses().RandomElement();
			}
			this.count = ScenPart_CorpseyCorpseyCorpseCorpse.CorpseCountChances.RandomElementByWeight((Pair<int, float> pa) => pa.Second).First;
		}

		/// <summary>
		/// Initialize the list of corpses
		/// </summary>
		/// <param name="kill">Whether the pawn should be killed; if true, return corpses, if false return pawns</param>
		/// <returns>the list of corpses (or pawns if kill = false, but unkilled corpses is just uncomfortable so why would you even)</returns>
		protected IEnumerable<Thing> GetCorpses(bool kill = true)
		{
			for (int i = 0; i < this.count; i++)
			{
				PawnKindDef kindDef = this.pawnKindDef ?? this.PossibleCorpses().RandomElement();

				var request = new PawnGenerationRequest(
					kind: kindDef,
					faction: null,
					allowDead: true,
					canGeneratePawnRelations: true);

				Pawn corpseToBe = PawnGenerator.GeneratePawn(request);

				if (!kill)
				{
					yield return corpseToBe;
				}
				else
				{
					HealthUtility.DamageUntilDead(corpseToBe);
					yield return corpseToBe.Corpse;
				}
			}
		}
	}
}