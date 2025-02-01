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

namespace LostTechnology
{
	public static class DebugActionsMisc
	{

		[DebugAction("LostTechnology World", null, false, false, allowedGameStates = AllowedGameStates.Playing)]
		private static void SetupWorldTechprint()
		{
			LostTechnologySettings.regenerate();
		}

		[DebugAction("LostTechnology World", null, false, false, allowedGameStates = AllowedGameStates.Playing)]
		private static void RemoveAllTechprintSite()
		{
			LostTechnologySettings.removeAllTechprintSite();
		}

        [DebugAction("LostTechnology World", null, false, false, allowedGameStates = AllowedGameStates.Playing)]
        private static void SpawnTechprintSiteRandom()
        {
            var researchProject = DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.IsFinished is false).RandomElement();
            if (researchProject != null)
            {
                Core.MakeTechPrintSite(researchProject.Techprint, researchProject.techLevel);
            }
        }

        [DebugAction("LostTechnology World", null, false, false, allowedGameStates = AllowedGameStates.Playing)]
        private static void SpawnTechprintSiteSelect()
        {
            List<DebugMenuOption> outerList = new List<DebugMenuOption>();
            foreach (var researchProject in DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.IsFinished is false))
            {
                outerList.Add(new DebugMenuOption(researchProject.LabelCap, DebugMenuOptionMode.Action, delegate
                {
                    Core.MakeTechPrintSite(researchProject.Techprint, researchProject.techLevel);
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(outerList));
        }
    }
}