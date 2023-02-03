using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using AbilityUser;

namespace Vampire
{
	public class CaitiffFunctions
	{

		public static bool IsBloodline(Pawn vampire, BloodlineDef bloodline)
		{
			if (GetBloodline(vampire) == bloodline)
			{
				return true;
			}

			//if (vampire.VampComp().Bloodline == bloodline)
			//{
			//	return true;
			//}
			else
				return false;
			
		}

		public static BloodlineDef GetBloodline(Pawn vampire)
		{
			BloodlineDef vampireBloodline = vampire?.VampComp().Bloodline;
			return vampireBloodline;
		}
	}
}
