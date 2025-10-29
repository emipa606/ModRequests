using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResumableJobProgress
{
	internal static class OtherModCompatibility
	{
		private static bool cacheIsSmartDecon;
		private static bool hasChachedIsSmartDecon = false;

		public static bool IsSmartDecon()
		{
			if (!hasChachedIsSmartDecon)
			{
				cacheIsSmartDecon = ModLister.HasActiveModWithName("Smarter Deconstruction and Mining");
				hasChachedIsSmartDecon = true;
			}
			return cacheIsSmartDecon;
		}
	}
}
