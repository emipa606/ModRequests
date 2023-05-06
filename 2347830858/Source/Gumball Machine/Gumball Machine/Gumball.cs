using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Gumball
{
	public class IngestionOutcomeDoer_AteGumball : IngestionOutcomeDoer
	{
		public String gumballName;

		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
            if(CheckGumballHediffAlreadyExists(pawn))
				return;

			HediffDef hediffDef = DetermineOutcomeHediffDef();

			Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
			hediff.Severity = GumballMachineSettings.gumballSeverity;
			pawn.health.AddHediff(hediff);
		}

		private HediffDef DetermineOutcomeHediffDef()
		{

			var outcomeChance = Rand.Range(0f, 1f);
			var outcomeChance2 = Rand.Range(0f, 1f);
			var outcomeChance3 = Rand.Range(0f, 1f);

			if (GumballMachineSettings.badOutcomes && outcomeChance < GumballMachineSettings.badOutcomeChance)
			{
				if (GumballMachineSettings.catastrophicOutcomes && outcomeChance2 < GumballMachineSettings.catastrophicOutcomeChance)
				{
					return HediffDef.Named(gumballName + "_Catastrophic") ?? HediffDef.Named("Gumball_Catastrophic");
				}

				return HediffDef.Named(gumballName + "_Bad") ?? HediffDef.Named("Gumball_Bad");
			}
			else if (outcomeChance3 < GumballMachineSettings.neutralOutcomeChance || !GumballMachineSettings.goodOutcomes)
			{
				return HediffDef.Named(gumballName + "_Neutral") ?? HediffDef.Named("Gumball_Neutral");
			}
			else
			{
				if (GumballMachineSettings.extraordinaryOutcomes && outcomeChance2 < GumballMachineSettings.extraordinaryOutcomeChance)
				{
					return HediffDef.Named(gumballName + "_Extraordinary") ?? HediffDef.Named("Gumball_Extraordinary");
				}

				return HediffDef.Named(gumballName + "_Good") ?? HediffDef.Named("Gumball_Good");
			}
		}

		private bool CheckGumballHediffAlreadyExists(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named(gumballName + "_Extraordinary"))
				|| pawn.health.hediffSet.HasHediff(HediffDef.Named(gumballName + "_Good"))
				|| pawn.health.hediffSet.HasHediff(HediffDef.Named(gumballName + "_Neutral"))
				|| pawn.health.hediffSet.HasHediff(HediffDef.Named(gumballName + "_Bad"))
				|| pawn.health.hediffSet.HasHediff(HediffDef.Named(gumballName + "_Catastrophic")))
			{
				return true;
            }

			return false;
        }

	}
}