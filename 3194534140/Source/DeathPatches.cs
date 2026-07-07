using HarmonyLib;
using RimWorld;
using Verse;

[StaticConstructorOnStartup]
public static class DeathPatches
{
	static DeathPatches()
	{
		var harmony = new Harmony("com.jck.VengefulRage");
		harmony.PatchAll();
	}

	[HarmonyPatch(typeof(Pawn), "Kill")]
	public static class Pawn_Kill_Patch
	{
		public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
		{
			if (Rand.Value > VengefulRageMod.settings.triggerChance) return;

			if (__instance.RaceProps.Humanlike && __instance.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse) is Pawn partner)
			{
				bool killedByHumanoid = dinfo.HasValue && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn killer && killer.RaceProps.Humanlike;
				if (killedByHumanoid)
				{
					var vengefulRageDef = DefDatabase<MentalStateDef>.GetNamed("VengefulRage", true);
					if (partner.mindState.mentalStateHandler.TryStartMentalState(vengefulRageDef, "Spouse killed", true))
					{
						if (partner.MentalState is MentalState_VengefulRage vengefulRage)
						{
							vengefulRage.Target = dinfo.Value.Instigator as Pawn;
							string letterLabel = "Vengeful Rage";
							string letterText = $"{partner.NameShortColored} has flown into a vengeful rage and seeks revenge against {vengefulRage.Target.NameShortColored}.";
							Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.ThreatSmall, partner);
						}
					}
				}
			}
		}
	}
}