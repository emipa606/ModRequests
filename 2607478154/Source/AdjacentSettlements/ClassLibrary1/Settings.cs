using System;
using UnityEngine;
using Verse;

namespace allowAdjacentSettle
{
	// Token: 0x02000006 RID: 6
	public class AllowAdjacentSettle : Mod
	{
		// Token: 0x06000008 RID: 8 RVA: 0x0000210C File Offset: 0x0000030C
		public AllowAdjacentSettle(ModContentPack content) : base(content)
		{
			this.settings = base.GetSettings<Settings>();
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002121 File Offset: 0x00000321
		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(inRect);
			listing_Standard.CheckboxLabeled("Disable nearby relations hit", ref this.settings.disableRelationsHit, null);
			listing_Standard.End();
			base.DoSettingsWindowContents(inRect);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002152 File Offset: 0x00000352
		public override string SettingsCategory()
		{
			return "Allow Adjacent Settlements";
		}

		// Token: 0x04000002 RID: 2
		private Settings settings;
	}
	public class Settings : ModSettings
	{
		// Token: 0x06000006 RID: 6 RVA: 0x000020EA File Offset: 0x000002EA
		public override void ExposeData()
		{
			Scribe_Values.Look<bool>(ref this.disableRelationsHit, "disableRelationsHit", false, false);
			base.ExposeData();
		}

		// Token: 0x04000001 RID: 1
		public bool disableRelationsHit;
	}
}
