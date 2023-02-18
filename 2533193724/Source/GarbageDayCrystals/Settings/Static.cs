using System;
using UnityEngine;
using Verse;

namespace Crystosentient.Settings
{
	// Token: 0x02000016 RID: 22
	[StaticConstructorOnStartup]
	public static class Static
	{
		// Token: 0x0400003B RID: 59
		public static string LabelModName = "Crystosentients".Translate();

		// Token: 0x0400003C RID: 60
		public static string LabelAllowedToSpawn = "AllowedToSpawn".Translate();

		// Token: 0x0400003D RID: 61
		public static Color WarningColor = new Color(1f, 0.4f, 0.4f);
	}
}
