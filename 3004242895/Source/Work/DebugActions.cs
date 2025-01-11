using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;

namespace HumanResources.Work
{
    public static class DebugActions
    {
        [DebugAction("HumanResources", "Learn Tech", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Learn()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (Pawn p in Find.CurrentMap.mapPawns.AllPawnsSpawned)
            {
                var know = p.TryGetComp<CompKnowledge>();
                if (know == null)
                {
                    continue;
                }
                list.Add(new DebugMenuOption(p.Name.ToString(), DebugMenuOptionMode.Action, delegate
                {
                    List<DebugMenuOption> list2 = new List<DebugMenuOption>();
                    foreach (ResearchProjectDef tech in DefDatabase<ResearchProjectDef>.AllDefs.Except(know.knownTechs))
                    {
                        list2.Add(new DebugMenuOption(tech.label + " (" + tech.defName + ")", DebugMenuOptionMode.Action, delegate
                        {
                            know.LearnTech(tech);
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }

        [DebugAction("HumanResources", "Forget Tech", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Forget()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            foreach (Pawn p in Find.CurrentMap.mapPawns.AllPawnsSpawned)
            {
                var know = p.TryGetComp<CompKnowledge>();
                if (know is null)
                {
                    continue;
                }
                list.Add(new DebugMenuOption(p.Name.ToStringShort, DebugMenuOptionMode.Action, delegate
                {
                    List<DebugMenuOption> list2 = new List<DebugMenuOption>();
                    foreach (ResearchProjectDef tech in know.knownTechs)
                    {
                        list2.Add(new DebugMenuOption(tech.label + " (" + tech.defName + ")", DebugMenuOptionMode.Action, delegate
                        {
                            know.expertise.Remove(tech);
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }

        [DebugAction("HumanResources", "Write Back to Vanilla", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void WriteBackToVanilla()
        {
            foreach (ResearchProjectDef tech in 
                Find.World.GetComponent<TechDatabase>().techsArchived.Keys
            )
            {
                Find.ResearchManager.FinishProject(tech);
            }
        }
    }
}
