using System;

namespace Vampire
{
	public class HediffVampirism_Caitiff : HediffVampirism_VampGiver
	{
		public override BloodlineDef Bloodline
		{
			get
			{
				return VampDefOf.ROMV_ClanCaitiff;
			}
		}
	}
}
