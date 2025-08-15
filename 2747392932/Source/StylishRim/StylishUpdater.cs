using AlienRace;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace StylishRim
{
	public class StylishUpdater
	{
		public static readonly float MOD_VERSION = 8.7f;
		// スタイリッシュなアップデーター
		// アップデート時に設定へ変更が必要になったらここでやる
		// 仕様が固まってきたらぶっちゃけいらんやつ
		public static void Run()
		{
			if (StylishModSettings.ModVersion < 1.0f) Update1_0();
			if (StylishModSettings.ModVersion < 1.1f) Update1_1();
			if (StylishModSettings.ModVersion < 1.2f) Update1_2();
			if (StylishModSettings.ModVersion < 2.0f) Update2_0();
			if (StylishModSettings.ModVersion < 3.0f) Update3_0();
			if (StylishModSettings.ModVersion < 3.1f) Update3_1();
			if (StylishModSettings.ModVersion < 4.0f) Update4_0();
			if (StylishModSettings.ModVersion < 4.1f) Update4_1();
			if (StylishModSettings.ModVersion < 5.0f) Update5_0();
			if (StylishModSettings.ModVersion < 5.2f) Update5_2();
			if (StylishModSettings.ModVersion < 6.0f) Update6_0();
			if (StylishModSettings.ModVersion < 6.2f) Update6_2();
			if (StylishModSettings.ModVersion < 7.0f) Update7_0();
			if (StylishModSettings.ModVersion < 7.3f) Update7_3();
			if (StylishModSettings.ModVersion < 7.5f) Update7_5();
			if (StylishModSettings.ModVersion < 8.1f) Update8_1();
			if (StylishModSettings.ModVersion < 8.4f) Update8_4();
			if (StylishModSettings.ModVersion < 8.5f) Update8_5();
			if (StylishModSettings.ModVersion < 8.7f) Update8_7();
		}

		private static void Update1_0()
		{
			//Update 1.0
			Log.Message("[Stylish Rim] 1.0 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				pss.adjustHeadOffset.x = 0f;
				pss.adjustBodyAddonOffset.x = 0f;
				pss.adjustHeadAddonOffset.x = 0f;
			};
			StylishModSettings.ModVersion = 1.0f;
			Log.Message("[Stylish Rim] Added some offset X, Update to version 1.0 is now stylishly done.");
		}
		private static void Update1_1()
		{
			//Update 1.1
			Log.Message("[Stylish Rim] 1.1 Updating...");
			foreach (KeyValuePair<string, PawnStyleSet> pair in PawnStyleSet.Styles)
			{
				pair.Value.key = pair.Key;
				pair.Value.ApplyLargestValue();
			}
			StylishModSettings.ModVersion = 1.1f;
			StylishModSettings.skipCalc = true;
			Log.Message("[Stylish Rim] Added key value, Update to version 1.1 is now stylishly done.");
		}
		private static void Update1_2()
		{
			// Update 1.2
			Log.Message("[Stylish Rim] 1.2 Updating...");
			StylishModSettings.ModVersion = 1.2f;
			Log.Message("[Stylish Rim] The only thing to update was the version notation, Update to version 1.2 is now stylishly done.");
		}
		private static void Update2_0()
		{
			// Update 2.0
			Log.Message("[Stylish Rim] 2.0 Updating...");
			StylishModSettings.ModVersion = 2.0f;
			Log.Message("[Stylish Rim] The only thing to update was the version notation, Update to version 2.0 is now stylishly done.");
		}
		private static void Update3_0()
		{
			// Update 3.0
			Log.Message("[Stylish Rim] 3.0 Updating...");
			StylishModSettings.ModVersion = 3.0f;
			Log.Message("[Stylish Rim] The only thing to update was the version notation, Update to version 3.0 is now stylishly done.");
		}
		private static void Update3_1()
		{
			// Update 3.1
			Log.Message("[Stylish Rim] 3.1 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				if (pss.adjustPortraitSize < 10) pss.adjustPortraitSize *= 100;
				pss.adjustPortraitOffset -= pss.adjustPortraitOffset * 2;
				if (pss.Hairs != null)
				{
					foreach (StylishSet hss in pss.Hairs.Values)
					{
						hss.size.z = hss.size.x;
						hss.offset.z = hss.offset.x;
					}
				}
				if (pss.Apparels != null)
				{
					foreach (ApparelDict ad in pss.Apparels.Values)
					{
						if (ad != null && ad.Values != null)
						{
							foreach (StylishSet ass in ad.Values)
							{
								ass.size.y = ass.size.z;
								ass.size.z = ass.size.x;
							}
						}
					}
				}
			}
			StylishModSettings.ModVersion = 3.1f;
			Log.Message("[Stylish Rim] Fix value for new adjustments, Update to version 3.1 is now stylishly done.");
		}
		private static void Update4_0()
		{
			// Update 4.0
			Log.Message("[Stylish Rim] 4.0 Updating...");
			StylishModSettings.ModVersion = 4.0f;
			Log.Message("[Stylish Rim] The only thing to update was the version notation, Update to version 4.0 is now stylishly done.");
		}
		private static void Update4_1()
		{
			// Update 4.1
			Log.Message("[Stylish Rim] 4.1 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				pss.adjustPortraitOffset *= 1000;
			}
			StylishModSettings.ModVersion = 4.1f;
			Log.Message("[Stylish Rim] Fixed offset digit for input area, Update to version 4.1 is now stylishly done.");
		}
		private static void Update5_0()
		{
			// Update 5.0
			Log.Message("[Stylish Rim] 5.0 Updating...");
			StylishModSettings.ModVersion = 5.0f;
			Log.Message("[Stylish Rim] The only thing to update was the version notation, Update to version 5.0 is now stylishly done.");
		}
		private static void Update5_2()
		{
			// Update 5.2
			Log.Message("[Stylish Rim] 5.2 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				pss.adjustHeadSize.z = pss.adjustHeadSize.x;
				pss.adjustHeadAddonSize.z = pss.adjustHeadAddonSize.x;
				pss.adjustBodyAddonSize.z = pss.adjustBodyAddonSize.x;
			}
			StylishModSettings.ModVersion = 5.2f;
			Log.Message("[Stylish Rim] The only thing to update was the version notation, Update to version 5.2 is now stylishly done.");
		}
		private static void Update6_0()
		{
			// Update 6.0
			Log.Message("[Stylish Rim] 6.0 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				if (pss.Hairs != null)
				{
					foreach (KeyValuePair<string, StylishSet> p in pss.Hairs)
					{
						p.Value.key = p.Key;
					}
				}
				if (pss.Apparels != null)
				{
					foreach (KeyValuePair<string, ApparelDict> p in pss.Apparels)
					{
						if (p.Value != null && p.Value.Values != null)
						{
							foreach (StylishSet ass in p.Value.Values)
							{
								ass.key = p.Key;
								ass.priority *= 2;
							}
						}
					}
				}
			}
			StylishModSettings.ModVersion = 6.0f;
			Log.Message("[Stylish Rim] Adjusted apparel prioriry value, Update to version 6.0 is now stylishly done.");
		}
		private static void Update6_2()
		{
			// Update 6.2
			Log.Message("[Stylish Rim] 6.2 Updating...");
			StylishModSettings.ModVersion = 6.2f;
			Log.Message("[Stylish Rim] Change config file name, Update to version 6.2 is now stylishly done.");
		}
		private static void Update7_0()
		{
			// Update 7.0
			Log.Message("[Stylish Rim] 7.0 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				if (pss.changeBodyRaceName == string.Empty) pss.changeBodyRaceName = null;
				if (pss.changeBodyTypeName == string.Empty) pss.changeBodyTypeName = null;
			}
			StylishModSettings.ModVersion = 7.0f;
			Log.Message("[Stylish Rim] The only thing to update was the version notation, Update to version 7.0 is now stylishly done.");
		}
		private static void Update7_3()
		{
			// Update 7.3
			Log.Message("[Stylish Rim] 7.3 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				if (pss.Hairs != null)
				{
					foreach (StylishSet ss in pss.Hairs.Values)
					{
						ss.offset /= 2;
					}
				}
				if (pss.Apparels != null)
				{
					foreach (KeyValuePair<string, ApparelDict> p in pss.Apparels)
					{
						if (p.Value != null && p.Value.Values != null)
						{
							foreach (StylishSet ss in p.Value.Values)
							{
								ss.offset /= 2;
							}
						}
					}
				}
				if (pss.Equipments != null)
				{
					foreach (StylishSet ss in pss.Equipments.Values)
					{
						ss.offset /= 2;
					}
				}
			}
			StylishModSettings.ModVersion = 7.3f;
			Log.Message("[Stylish Rim] Adjusted offset value, Update to version 7.3 is now stylishly done.");
		}
		private static void Update7_5()
		{
			// Update 7.5
			Log.Message("[Stylish Rim] 7.5 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				if (pss.Apparels != null)
				{
					foreach (KeyValuePair<string, ApparelDict> p in pss.Apparels)
					{
						if (p.Value != null && p.Value.Values != null)
						{
							foreach (StylishSet ss in p.Value.Values)
							{
								ss.northInvert = true;
							}
						}
					}
				}
			}
			StylishModSettings.ModVersion = 7.5f;
			Log.Message("[Stylish Rim] All apparel north inverting, Update to version 7.5 is now stylishly done.");
		}
		private static void Update8_1()
		{
			// Update 8.1
			Log.Message("[Stylish Rim] 8.1 Updating...");
			if (StylishAtlasManager.leaveOneCache)
			{
				foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
				{
					pss.leaveOneCache = true;
				}
			}
			StylishAtlasManager.leaveOneCache = false;
			StylishModSettings.ModVersion = 8.1f;
			Log.Message("[Stylish Rim] LeaveOneCache has changed from global to each race., Update to version 8.1 is now stylishly done.");
		}
		private static void Update8_4()
		{
			// Update 8.4
			Log.Message("[Stylish Rim] 8.4 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				foreach (ApparelDict ad in pss.Apparels.Values)
				{
					foreach (StylishSet ss in ad.Values)
					{
						if (ss.offset.w == 0) ss.offset.w = ss.offset.y;
					}
				}
				foreach (StylishSet ss in pss.Hairs.Values)
				{
					if (ss.offset.w == 0) ss.offset.w = ss.offset.y;
				}
				foreach (StylishSet ss in pss.Equipments.Values)
				{
					if (ss.offset.w == 0) ss.offset.w = ss.offset.y;
				}
			}
			StylishModSettings.ModVersion = 8.4f;
			Log.Message("[Stylish Rim] Apparels and Hairs added w slider, Update to version 8.4 is now stylishly done.");
		}
		private static void Update8_5()
		{
			// Update 8.5
			Log.Message("[Stylish Rim] 8.5 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				if (pss.Addons != null && pss.Addons.Count > 0)
				{
					foreach (StylishAddon sa in pss.Addons.Values)
					{
						if (sa.offset.w == 0) sa.offset.w = sa.offset.y;
					}
				}
				if (pss.Equipments != null && pss.Equipments.Count > 0)
				{
					foreach (StylishSet ss in pss.Equipments.Values)
					{
						if (ss.angle.z == 0) ss.angle.z = ss.angle.x;
						if (ss.angle.w == 0) ss.angle.w = ss.angle.y;
					}
				}
			}
			StylishModSettings.ModVersion = 8.5f;
			Log.Message("[Stylish Rim] Addons added w slider, Equipments added attack angle, Update to version 8.5 is now stylishly done.");
		}
		private static void Update8_7()
		{
			// Update 8.7
			Log.Message("[Stylish Rim] 8.7 Updating...");
			foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values)
			{
				if (pss.IsRace)
				{
					if (pss.adjustDrawLoc.y != 0f)
					{
						foreach (ThingDef_AlienRace def in DefDatabase<ThingDef_AlienRace>.AllDefs.Where(x => x.defName == pss.raceName))
						{
							if (def != null)
							{
								pss.drawLocCorrections = new Dictionary<string, UnityEngine.Vector3>();
								foreach (BodyTypeDef bt in def.alienRace.generalSettings.alienPartGenerator.bodyTypes)
								{
									pss.drawLocCorrections.Add(bt.defName, new UnityEngine.Vector3(0, pss.adjustDrawLoc.y, 0));
								}
							}
						}
						pss.adjustDrawLoc = UnityEngine.Vector2.zero;
					}
				}
			}
			StylishModSettings.ModVersion = 8.7f;
			Log.Message("[Stylish Rim] Added drawloc by bodytype slider, Update to version 8.7 is now stylishly done.");
		}
	}
}
