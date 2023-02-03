using System;
using Verse;

namespace Diabetes
{
	public static class DrugUtility
	{
		public static int GetMaxAmountToPickup(Thing drug, Pawn pawn, int wantedCount)
		{
			int num = Math.Min(wantedCount, drug.stackCount);
			if (drug.Spawned && drug.Map != null)
			{
				return Math.Min(num, drug.Map.reservationManager.CanReserveStack(pawn, drug));
			}
			return num;
		}
	}
}
