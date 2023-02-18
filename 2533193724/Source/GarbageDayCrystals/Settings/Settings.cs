using System;
using Verse;

namespace Crystosentient.Settings
{
	// Token: 0x02000015 RID: 21
	public class Settings : ModSettings
	{
		// Token: 0x06000073 RID: 115 RVA: 0x00002494 File Offset: 0x00000694
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<IntRange>(ref Settings.StoneTypesAvailable, "Crystosentients", new IntRange(4, 5), false);
			Scribe_Values.Look<bool>(ref Settings.SpawnGemGarden, "SpawnGemGardens", true, false);
		}

		// Token: 0x04000039 RID: 57
		internal static IntRange StoneTypesAvailable = new IntRange(4, 5);

		// Token: 0x0400003A RID: 58
		internal static bool SpawnGemGarden = true;
	}
}
