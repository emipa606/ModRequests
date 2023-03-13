using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using Verse;

// IdeoLoadDev
// by GearheadLydia
// some basic ideas borrowed from Kentington

namespace IdeoLoadDev
{
	[StaticConstructorOnStartup]
	static class IdeoLoadDev
	{
		static IdeoLoadDev()
		{
			Harmony harmony = new Harmony("GearheadLydia.IdeoLoadDev");
			harmony.PatchAll();
		}
	}

	// patching the IdeoUIUtility.DoIdeoSaveLoad method because it's where the standard buttons are located

	[HarmonyPatch(typeof(IdeoUIUtility), "DoIdeoSaveLoad")]
	static class IdeoLoadDev_Patch
	{
		// this is, for whatever reason, a private variable in IdeoUIUtility, so we're recreating it here instead of trying to read it. there's probably a better way, I know
		private static readonly Vector2 SaveLoadButtonSize_Patch = new Vector2(70f, 30f);

		// here's where the magic happens
		[HarmonyPostfix]
		public static void Postfix(ref float curY, ref float width, ref Ideo ideo, ref bool ___devEditMode, ref bool allowLoad)
		{

			// need this thing from the vanilla code because it's a local variable we need to walk back the button position

			float num = width / 2f;
			float num3 = Mathf.Max(width - (width - IdeoUIUtility.PreceptBoxSize.x * 3f - 16f) / 2f - SaveLoadButtonSize_Patch.x, num + 10f);

			// everybody do the rewind! move the Y cursor back to where it was before the end of the official method
			curY -= SaveLoadButtonSize_Patch.y + 4f;

			// move the X cursor back a bit
			num3 -= SaveLoadButtonSize_Patch.x;

			// CREATE LOAD BUTTON

			// only shows under the following circumstances:
			// - dev edit mode (with the onscreen check) when loading is normally not allowed at all
			// - regular dev mode when loading is allowed, but only for player factions
			if ((!allowLoad && ___devEditMode) || (allowLoad && Faction.OfPlayer.ideos.PrimaryIdeo != ideo && Prefs.DevMode))
			{
				// create the actual load button
				Rect rect2a = new Rect(num3, curY, SaveLoadButtonSize_Patch.x, SaveLoadButtonSize_Patch.y);

				if (Widgets.ButtonText(rect2a, "Load".Translate()))
				{
					// make a copy of the existing ideo because it's passed by reference and anonymous methods don't like that
					Ideo oldIdeoClone = ideo;
					
					// we need to actually create an action delegate here. this probably could have gone outside the button definition,
					// but I'm not 100% sure whether that'd have had weird effects, so I'm not taking chances
					Action<Ideo> loadIdeoNew = delegate (Ideo loadedIdeo)
					{
						// ideo might be null if dialog canceled
						if (loadedIdeo != null)
						{
							// confirm that loading worked
							Log.Message("Loaded ideo " + loadedIdeo.name + " from file.");

							// scan the list of existing ideos for one matching the one we're replacing
							// this may skip some hidden ideos, but the player probably shouldn't be seeing those anyway?
							foreach (Ideo scanIdeo in Find.IdeoManager.IdeosInViewOrder)
                            {
								if (scanIdeo.id == oldIdeoClone.id)
								{
									// use vanilla copyTo method
									loadedIdeo.CopyTo(scanIdeo);
									Log.Message("Copied ideo " + loadedIdeo.name + " to ID " + scanIdeo.id + ".");
									// we only want to replace the one (there should be only one!)
									break;
								}
                            }
						}
					};

					// add the actual button
					Find.WindowStack.Add(new Dialog_IdeoList_Load(loadIdeoNew));
				}

				// move the cursor forward again to where it was before we started messing around
				
			}
			curY += SaveLoadButtonSize_Patch.y + 4f;

			// CREATE SPLIT/JOIN BUTTONS (in devedit mode only)


		}
	}
}