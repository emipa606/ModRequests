using Hospitality;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace KriilMod_CD
{
	[StaticConstructorOnStartup]
	public static class CompatibilityUtility
	{
		public static readonly bool HospitalityEnabled = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.Name == "Hospitality");

		public static bool IsGuest(Pawn pawn)
		{
			bool result = false;
			if (HospitalityEnabled)
			{
				try
				{
					result = ((Func<bool>)delegate
					{
						if (!pawn.IsValidGuestPawn())
						{
							return false;
						}
						LordJob lordJob = ((((ThingWithComps)pawn)?.GetComp<CompGuest>())?.lord)?.LordJob;
						return lordJob is Hospitality.LordJob_VisitColony;
					})();
				}
				catch (TypeLoadException ex)
				{
					Log.Warning("Failed to check whether ped is a guest. " + ex.Message);
				}
			}
			return result;
		}

		public static bool IsValidGuestPawn(this Pawn pawn)
		{
			if (pawn == null)
			{
				return false;
			}
			if (pawn.Destroyed)
			{
				return false;
			}
			if (!pawn.Spawned)
			{
				return false;
			}
			if (pawn.thingIDNumber == 0)
			{
				return false;
			}
			if (pawn.Name == null)
			{
				return false;
			}
			if (pawn.Dead)
			{
				return false;
			}
			RaceProperties raceProps = pawn.RaceProps;
			if (raceProps == null || !raceProps.Humanlike)
			{
				return false;
			}
			if (pawn.guest == null)
			{
				return false;
			}
			if (pawn.guest.HostFaction != Faction.OfPlayer && pawn.Map.ParentFaction != Faction.OfPlayer)
			{
				return false;
			}
			if (pawn.Faction == null)
			{
				return false;
			}
			if (pawn.IsPrisonerOfColony || pawn.Faction == Faction.OfPlayer)
			{
				return false;
			}
			if (pawn.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			return true;
		}
	}
}
