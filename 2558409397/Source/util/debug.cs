using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;
using Verse.Profile;
using Verse.Sound;
using Verse;

namespace YayoNature
{
	public static class DebugActionsMisc
	{



		[DebugAction("Yayo's Nature", null, false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ChangeBiome()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption($"start.NextBiome", DebugMenuOptionMode.Action, delegate
			{
				dataUtility.GetData(Current.Game.CurrentMap).startChange();
			}));
			foreach (BiomeDef b in core.ar_b)
			{
				list.Add(new DebugMenuOption($"start.{b.label}", DebugMenuOptionMode.Action, delegate
				{
					dataUtility.GetData(Current.Game.CurrentMap).startChange(b);
				}));
			}
			foreach (BiomeDef b in core.ar_b)
			{
				list.Add(new DebugMenuOption($"make.{b.label}(few minute)", DebugMenuOptionMode.Action, delegate
				{
					dataUtility.GetData(Current.Game.CurrentMap).debugChangeImmediately(b);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}



		[DebugAction("Yayo's Nature", null, false, false, allowedGameStates = AllowedGameStates.Playing)]
		private static void ResetPlanetForNewBiome()
		{
			core.resetPlanet(true);
		}
		



	}
}