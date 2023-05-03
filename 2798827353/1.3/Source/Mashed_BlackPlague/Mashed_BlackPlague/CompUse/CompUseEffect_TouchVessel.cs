using System;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse.AI;

namespace Mashed_BlackPlague
{
    class CompUseEffect_TouchVessel : CompUseEffect_Artifact
	{
		new public CompProperties_UseEffectTouchVessel Props
		{
			get
			{
				return (CompProperties_UseEffectTouchVessel)this.props;
			}
		}

		public int cooldownTicks = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref cooldownTicks, "PersonaTardusCooldownTicks", 0);
		}

        public override void CompTickRare()
        {
			if (cooldownTicks > 0)
			{
				cooldownTicks -= 250;
			}
			base.CompTickRare();
        }

        public override string CompInspectStringExtra()
        {
            return cooldownTicks > 0 ? "BlackPlague_Vessel_OnCooldown".Translate(cooldownTicks.ToStringTicksToDays()) : null;
        }

        public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
            if (Rand.Chance(Mashed_BlackPlague_ModSettings.Chance_InfectedTouch))
            {
				usedBy.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BlackPlague_TouchedVessel_Infected, null, null);
				usedBy.health.AddHediff(HediffDefOf.BlackPlague_TuurngaitVirus).Severity = 0.001f;
			}
			else
            {
				usedBy.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BlackPlague_TouchedVessel, null, null);
			}

			MentalState_TouchVessel mentalState_TouchVessel = usedBy.MentalState as MentalState_TouchVessel;
			if (mentalState_TouchVessel != null)
			{
				mentalState_TouchVessel.Notify_VesselTouched();
				usedBy.MentalState.RecoverFromState();
			}
            if (Props.recordDef != null)
            {
				usedBy.records.Increment(Props.recordDef);
            }
			FleckMaker.AttachedOverlay(parent, FleckDefOf.PsycastAreaEffect, Vector3.zero, 1f, -1f);
			cooldownTicks = Props.cooldownTicksDays;
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
            if (cooldownTicks > 0)
            {
				failReason = "BlackPlague_Vessl_OnCooldown".Translate();
				return false;
			}
			if (Utility.PawnHasVesselThought(p))
			{
				failReason = "BlackPlague_Vessel_AlreadyHasThought".Translate(p.Name);
				return false;
			}
			if (Utility.PawnIsTuurngait(p))
			{
				failReason = "BlackPlague_Vessel_IsTuurngait".Translate(p.Name);
				return false;
			}
			if (Utility.PawnIsNotValid(p))
			{
				failReason = "BlackPlague_Vessel_IsNotFleshy".Translate(p.Name);
				return false;
			}
			return base.CanBeUsedBy(p, out failReason);
		}
	}
}
