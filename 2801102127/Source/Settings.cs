using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResumableJobProgress
{
	public class Settings : ModSettings
	{
		static readonly IEnumerable<string> listedResumingJobs =
		[
			"DesignatorDeconstruct",
			"DesignatorUninstall",
			"DesignatorHarvest",
		];

		static bool strictIngredient = false;
		static Dictionary<string, bool> disabledResumingJobs = [];

		public static bool IsStrictIngredient => strictIngredient;
		public static bool IsDisabledDeconstruct => IsDisabledJob("DesignatorDeconstruct");
		public static bool IsDisabledUninstall => IsDisabledJob("DesignatorUninstall");
		public static bool IsDisabledHarvest => IsDisabledJob("DesignatorHarvest");

		override public void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref strictIngredient, "strictIngredient");
			Scribe_Collections.Look(ref disabledResumingJobs, "disabledResumingJobs", LookMode.Value, LookMode.Value);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				// 古いバージョンの設定がある場合は、引き継ぐ
				var loopResumingJobs = new Dictionary<string, bool>(disabledResumingJobs);
				foreach (var tempResumingJob in loopResumingJobs)
				{
					var oldJobItemKey = tempResumingJob.Key.Replace("Designator", "");
					if (disabledResumingJobs.ContainsKey(oldJobItemKey))
					{
						disabledResumingJobs.SetOrAdd(tempResumingJob.Key, disabledResumingJobs[oldJobItemKey]);
						disabledResumingJobs.Remove(oldJobItemKey);
					}
				}
			}
		}

		public static void DoSettingsWindowContents(Rect inRect)
		{
			var listing = new Listing_Standard();

			listing.Begin(new Rect(inRect.x, inRect.y, inRect.width * 0.6f, inRect.height));
			listing.CheckboxLabeled("ResumableJobProgress.StrictIngredient.label".Translate(), ref strictIngredient, "ResumableJobProgress.StrictIngredient.desc".Translate());
			listing.Gap();
			listing.Label("ResumableJobProgress.DisablesResuming".Translate());
			listing.GapLine(6f);
			foreach (var listedResumingJob in listedResumingJobs)
			{
				if (!disabledResumingJobs.TryGetValue(listedResumingJob, out bool isResuming))
				{
					isResuming = false;
				};
				listing.CheckboxLabeled(listedResumingJob.Translate(), ref isResuming);
				disabledResumingJobs[listedResumingJob] = isResuming;
			}
			listing.End();
		}

		static bool IsDisabledJob(string key)
		{
			if (disabledResumingJobs.TryGetValue(key, out bool value))
			{
				return value;
			}
			return false;
		}
	}
}
