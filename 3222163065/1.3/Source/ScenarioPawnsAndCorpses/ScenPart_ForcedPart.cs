using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ScenarioPawnsAndCorpses
{
	/// <summary>
	/// Scenpart adding part hediffs
	/// </summary>
	public class ScenPart_ForcedPart : ScenPart_PawnModifier
	{
		/// <summary>
		/// The hediff def this scenpart will add.
		/// </summary>
		private HediffDef hediff;

		/// <summary>
		/// The body type that this scenpart applies to.
		/// </summary>
		private BodyDef bodyType;

		/// <summary>
		/// The part that the hediff will be added to.
		/// </summary>
		protected BodyPartRecord part;

		/// <summary>
		/// Setup the UI for the part in the scenario edit interface
		/// </summary>
		/// <param name="listing">The list host for the scen part</param>
		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 5f);
			Rect rect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, scenPartRect.height / 5f);
			Rect rect2 = new Rect(scenPartRect.x, scenPartRect.y + scenPartRect.height / 5f, scenPartRect.width, scenPartRect.height / 5f);
			Rect rect3 = new Rect(scenPartRect.x, scenPartRect.y + (2 * scenPartRect.height / 5f), scenPartRect.width, scenPartRect.height / 5f);

			if (Widgets.ButtonText(rect, this.bodyType.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (BodyDef body in this.PossibleBodyDefs().OrderBy(bd => $"{bd.label} ({bd.defName})"))
				{
					list.Add(new FloatMenuOption($"{body.LabelCap} ({body.defName})", delegate
					{
						this.bodyType = body;
					}));
				}

				Find.WindowStack.Add(new FloatMenu(list));
			}

			if (Widgets.ButtonText(rect2, this.part.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (BodyPartRecord b in this.PossibleBodyParts(this.bodyType).OrderBy(bp => bp.Label))
				{
					list.Add(new FloatMenuOption(b.LabelCap, delegate
					{
						this.part = b;
					}));
				}

				Find.WindowStack.Add(new FloatMenu(list));
			}

			if (Widgets.ButtonText(rect3, this.hediff.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (HediffDef h in this.PossiblePartHediffs().OrderBy(hd => hd.label))
				{
					list.Add(new FloatMenuOption(h.LabelCap, delegate
					{
						this.hediff = h;
					}));
				}

				Find.WindowStack.Add(new FloatMenu(list));
			}

			DoPawnModifierEditInterface(scenPartRect.BottomPart(0.4f));
		}

		/// <summary>
		/// The possible hediffs: basically any replacement part or implant in the game
		/// </summary>
		/// <returns>the list of implants</returns>
		private IEnumerable<HediffDef> PossiblePartHediffs()
		{
			return DefDatabase<HediffDef>.AllDefsListForReading.Where((HediffDef x) => x.countsAsAddedPartOrImplant);	
		}

		/// <summary>
		/// The possible body defs
		/// </summary>
		/// <returns>the list of body defs</returns>
		private IEnumerable<BodyDef> PossibleBodyDefs()
		{
			return DefDatabase<BodyDef>.AllDefsListForReading;
		}

		/// <summary>
		/// The possible body parts for the given body def
		/// </summary>
		/// <param name="body">the body type to retrieve the body parts from</param>
		/// <returns>all body parts relevant to the provided body</returns>
		private IEnumerable<BodyPartRecord> PossibleBodyParts(BodyDef body = null)
		{
			if (body == null)
			{
				HashSet<string> bodies = new HashSet<string>();
				List<BodyPartRecord> bodyParts = new List<BodyPartRecord>();

				foreach (BodyDef b in this.PossibleBodyDefs())
                {
					if (!bodies.Contains(b.defName))
                    {
						bodies.Add(b.defName);
						bodyParts.AddRange(b.AllParts);
                    }
				}

				return bodyParts;
			}

			return body.AllParts;
		}

		/// <summary>
		/// Save hediff info: the def and body part if applicable
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref this.hediff, "hediff");
			Scribe_BodyParts.Look(ref this.part, "part");
			Scribe_Defs.Look(ref this.bodyType, "bodyType");
		}

		/// <summary>
		/// Get the summary for this scenpart
		/// </summary>
		/// <param name="scen">unused</param>
		/// <returns>The string representation of this backstory scenpart</returns>
		public override string Summary(Scenario scen)
		{
			return "ScenarioPawnsAndCorpses.ScenPart_ForcedPart_Summary".Translate(
				this.context.ToStringHuman(),
				this.bodyType.label,
				this.chance.ToStringPercent(),
				this.hediff.label,
				this.part.Label);
		}

		/// <summary>
		/// Setup defaults for a new scenpart
		/// </summary>
		public override void Randomize()
		{
			base.Randomize();
			var bodies = this.PossibleBodyDefs();
			this.bodyType = bodies.Where(b => b.defName.Equals("human", System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

			if (this.bodyType == null)
            {
				this.bodyType = bodies.ElementAt(Rand.Range(0, bodies.Count()));
			}

			var bodyParts = this.PossibleBodyParts(this.bodyType);
			var tempBodyParts = bodyParts.Where(b => b.def.defName.Equals("shoulder", System.StringComparison.OrdinalIgnoreCase));

			if (tempBodyParts.Any())
			{
				this.part = tempBodyParts.ElementAt(Rand.Range(0, tempBodyParts.Count()));
			}
			else
			{
				this.part = bodyParts.ElementAt(Rand.Range(0, bodyParts.Count()));
			}

			var hediffs = this.PossiblePartHediffs();

			if (tempBodyParts.Any())
            {
				var tempHediffs = hediffs.Where(h => h.defName.Equals("bionicarm", System.StringComparison.OrdinalIgnoreCase));

				if (tempHediffs.Any())
                {
					this.hediff = tempHediffs.ElementAt(Rand.Range(0, tempHediffs.Count()));
				}
			}

			if (this.hediff == null)
            {
				this.hediff = hediffs.ElementAt(Rand.Range(0, hediffs.Count()));
			}

			this.chance = 1f;
			this.context = PawnGenerationContext.PlayerStarter;
		}

		/// <summary>
		/// Merge the chances with other scenparts if overlapping
		/// </summary>
		/// <param name="other">The other scenpart to merge with</param>
		/// <returns>Whether a merge occurred</returns>
		public override bool TryMerge(ScenPart other)
		{
			if (other is ScenPart_ForcedPart scenPart_ForcedPart
				&& this.bodyType == scenPart_ForcedPart.bodyType
				&& this.part.Label == scenPart_ForcedPart.part.Label
				&& this.hediff == scenPart_ForcedPart.hediff)
			{
				chance = GenMath.ChanceEitherHappens(chance, scenPart_ForcedPart.chance);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Whether the hediff this scenpart is adding can coexist with other hediffs from other scenparts
		/// </summary>
		/// <param name="other">A scenpart to review</param>
		/// <returns>Whether this scenpart can coexist with the other</returns>
		public override bool CanCoexistWith(ScenPart other)
		{
			if (other is ScenPart_ForcedPart scenPart_ForcedPart
				&& this.bodyType == scenPart_ForcedPart.bodyType
				&& this.part.Label == scenPart_ForcedPart.part.Label
				&& this.hediff.hediffClass.Equals(typeof(Hediff_AddedPart))
				&& context.OverlapsWith(scenPart_ForcedPart.context))
			{
				return false;
			}

			return true;
		}

		public override bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
		{
			if (!base.AllowPlayerStartingPawn(pawn, tryingToRedress, req))
			{
				return false;
			}
			if (hideOffMap)
			{
				if (!req.AllowDead && pawn.health.WouldDieAfterAddingHediff(this.hediff, this.part, 1f))
				{
					return false;
				}
				if (!req.AllowDowned && pawn.health.WouldBeDownedAfterAddingHediff(hediff, this.part, 1f))
				{
					return false;
				}
			}
			return true;
		}

		protected override void ModifyNewPawn(Pawn p)
		{
			AddHediff(p);
		}

		protected override void ModifyHideOffMapStartingPawnPostMapGenerate(Pawn p)
		{
			AddHediff(p);
		}

		private void AddHediff(Pawn p)
		{
			Hediff hediff = HediffMaker.MakeHediff(this.hediff, p, this.part);
			p.health.AddHediff(hediff);
		}

		public override void Notify_NewPawnGenerating(Pawn pawn, PawnGenerationContext context)
		{
			if (this.context.Includes(context)
				&& (!hideOffMap || context != PawnGenerationContext.PlayerStarter)
				&& Rand.Chance(chance)
				&& pawn.RaceProps.body == this.bodyType)
			{
				ModifyNewPawn(pawn);
			}
		}

		public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context, bool redressed)
		{
			if (this.context.Includes(context)
				&& (!hideOffMap || context != PawnGenerationContext.PlayerStarter)
				&& Rand.Chance(chance)
				&& pawn.RaceProps.body == this.bodyType)
			{
				ModifyPawnPostGenerate(pawn, redressed);
			}
		}
	}
}
