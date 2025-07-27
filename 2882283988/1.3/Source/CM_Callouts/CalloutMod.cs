using System;
using CM_Callouts.Patches.InteractionWorkers;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
	// Token: 0x02000003 RID: 3
	public class CalloutMod : Mod
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static CalloutMod Instance
		{
			get
			{
				return CalloutMod._instance;
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		public CalloutMod(ModContentPack content) : base(content)
		{
			Harmony harmony = new Harmony("CM_Callouts");
			harmony.PatchAll();
			InteractionWorker_RecruitAttempt_Patches.InteractionWorker_RecruitAttempt_DoRecruit.Patch(harmony);
			CalloutMod._instance = this;
			CalloutMod.settings = base.GetSettings<CalloutModSettings>();
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002098 File Offset: 0x00000298
		public override string SettingsCategory()
		{
			return "Callouts!";
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020AF File Offset: 0x000002AF
		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			CalloutMod.settings.DoSettingsWindowContents(inRect);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000020C6 File Offset: 0x000002C6
		public override void WriteSettings()
		{
			base.WriteSettings();
			CalloutMod.settings.UpdateSettings();
		}

		// Token: 0x04000019 RID: 25
		private static CalloutMod _instance;

		// Token: 0x0400001A RID: 26
		public static CalloutModSettings settings;
	}
}
