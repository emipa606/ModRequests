using HarmonyLib;
using System;
using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using RimWorld.Planet;
using static RimWorld.ColonistBar;

namespace SlaveBar
{

    public class SlaveBarSettings : ModSettings
    {
        public static bool toggleSlaves = true;
        public static bool togglePrisoners = true;
        public static bool toggleGhouls = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref toggleSlaves, "toggleSlaves", true);
            Scribe_Values.Look(ref togglePrisoners, "togglePrisoners", true);
            Scribe_Values.Look(ref toggleGhouls, "toggleGhouls", false);
            base.ExposeData();
        }
    }

    public class SlaveBarMod : Mod
    {
        public SlaveBarSettings settings;
        public SlaveBarMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<SlaveBarSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect contentRect = new Rect(0, 0, inRect.width, inRect.height - (10f)).ContractedBy(10f);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Display slaves", ref SlaveBarSettings.toggleSlaves, "Should slabes have their own display bar");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Display prisoners", ref SlaveBarSettings.togglePrisoners, "Should prisoners have their own display bar");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Display ghouls", ref SlaveBarSettings.toggleGhouls, "Should ghouls have their own display bar");
            listingStandard.End();
            Toggle.dirtyDisplay();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Slave Bar";
        }
    }

    [StaticConstructorOnStartup]
    public static class Toggle
    {
        public static int displayslave = 0;
        public static int displayCounter = 0;
        public static List<int> displays = new List<int>() { };

        static Toggle()
        {
            dirtyDisplay();
        }

        public static void dirtyDisplay()
        {
            displays.Clear();
            displays.Add(0);
            if (SlaveBarSettings.toggleSlaves) displays.Add(1);
            if (SlaveBarSettings.togglePrisoners) displays.Add(2);
            if (SlaveBarSettings.toggleGhouls) displays.Add(3);
        }
    }

    [StaticConstructorOnStartup]
    public static class Patches
    {
        public static readonly Texture2D slavesetting = ContentFinder<Texture2D>.Get("UI/togglebar", true);
        static Patches()
        {
            Harmony harmony = new Harmony("temmie3754.slavebar");
            harmony.Patch(AccessTools.Method(typeof(RimWorld.ColonistBar), "CheckRecacheEntries", null, null), new HarmonyMethod(Patches.patchType, "Patch_CheckRecacheEntries", null), null, null);
            harmony.Patch(AccessTools.Method(typeof(RimWorld.ColonistBar), "ColonistBarOnGUI", null, null), new HarmonyMethod(Patches.patchType, "Patch_ColonistBarOnGUI", null), null, null);

        }

        public static void Patch_ColonistBarOnGUI(ColonistBar __instance, ref List<Vector2> ___cachedDrawLocs, ref bool ___entriesDirty)
        {
            Vector2 start;
            if (___cachedDrawLocs.Count > 0)
            {
                start = ___cachedDrawLocs.Last();
                start.x += 60;
            }
            else
            {
                start = new Vector2 (UI.screenWidth/2, 0);
            }
            Rect rect = new Rect(start.x, start.y + 8, 32, 32);
            if (Widgets.ButtonImage(rect, slavesetting))
            {
                Toggle.displayCounter = (Toggle.displayCounter + 1) % Toggle.displays.Count;
                Toggle.displayslave = Toggle.displays[Toggle.displayCounter];
                ___entriesDirty = true;
            }
        }
        public static bool Patch_CheckRecacheEntries(RimWorld.ColonistBar __instance, ref List<Vector2> ___cachedDrawLocs, ref bool ___entriesDirty, ref List<Map> ___tmpMaps, ref List<Pawn> ___tmpPawns, ref List<Entry> ___cachedEntries, ref List<Caravan> ___tmpCaravans, ref ColonistBarDrawLocsFinder ___drawLocsFinder, ref List<int> ___cachedReorderableGroups, ref float ___cachedScale)
        {
            if (!___entriesDirty)
            {
                return true;
            }

            ___entriesDirty = false;
            ___cachedEntries.Clear();
            int num = 0;
            if (Find.PlaySettings.showColonistBar)
            {
                ___tmpMaps.Clear();
                ___tmpMaps.AddRange(Find.Maps);
                ___tmpMaps.SortBy((Map x) => !x.IsPlayerHome, (Map x) => x.uniqueID);
                for (int i = 0; i < ___tmpMaps.Count; i++)
                {
                    ___tmpPawns.Clear();
                    if (Toggle.displayslave == 0) ___tmpPawns.AddRange(___tmpMaps[i].mapPawns.FreeColonists.Where(pawn => !pawn.IsSlave));
                    else if (Toggle.displayslave == 1) ___tmpPawns.AddRange(___tmpMaps[i].mapPawns.FreeColonists.Where(pawn => pawn.IsSlave));
                    else if (Toggle.displayslave == 2) ___tmpPawns.AddRange(___tmpMaps[i].mapPawns.PrisonersOfColony);
                    else if (Toggle.displayslave == 3) ___tmpPawns.AddRange(___tmpMaps[i].mapPawns.AllHumanlike.Where(pawn => pawn.IsGhoul));
                    List<Thing> list = ___tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (!list[j].IsDessicated())
                        {
                            Pawn innerPawn = ((Corpse)list[j]).InnerPawn;
                            if (innerPawn != null && ((innerPawn.IsColonist && ((Toggle.displayslave==0 && !innerPawn.IsSlave) || (Toggle.displayslave==1 && innerPawn.IsSlave))) || (Toggle.displayslave == 2 && innerPawn.IsPrisonerOfColony)))
                            {
                                ___tmpPawns.Add(innerPawn);
                            }
                        }
                    }

                    IReadOnlyList<Pawn> allPawnsSpawned = ___tmpMaps[i].mapPawns.AllPawnsSpawned;
                    for (int k = 0; k < allPawnsSpawned.Count; k++)
                    {
                        Corpse corpse = allPawnsSpawned[k].carryTracker.CarriedThing as Corpse;
                        if (corpse != null && !corpse.IsDessicated() && ((corpse.InnerPawn.IsColonist && ((Toggle.displayslave == 0 && !corpse.InnerPawn.IsSlave) || (Toggle.displayslave == 1 && corpse.InnerPawn.IsSlave))) || (Toggle.displayslave == 2 && corpse.InnerPawn.IsPrisonerOfColony)))
                        {
                            ___tmpPawns.Add(corpse.InnerPawn);
                        }
                    }

                    PlayerPawnsDisplayOrderUtility.Sort(___tmpPawns);
                    for (int l = 0; l < ___tmpPawns.Count; l++)
                    {
                        ___cachedEntries.Add(new ColonistBar.Entry(___tmpPawns[l], ___tmpMaps[i], num));
                    }

                    if (!___tmpPawns.Any())
                    {
                        ___cachedEntries.Add(new ColonistBar.Entry(null, ___tmpMaps[i], num));
                    }

                    num++;
                }

                ___tmpCaravans.Clear();
                ___tmpCaravans.AddRange(Find.WorldObjects.Caravans);
                ___tmpCaravans.SortBy((Caravan x) => x.ID);
                for (int m = 0; m < ___tmpCaravans.Count; m++)
                {
                    if (!___tmpCaravans[m].IsPlayerControlled)
                    {
                        continue;
                    }

                    ___tmpPawns.Clear();
                    ___tmpPawns.AddRange(___tmpCaravans[m].PawnsListForReading);
                    PlayerPawnsDisplayOrderUtility.Sort(___tmpPawns);
                    for (int n = 0; n < ___tmpPawns.Count; n++)
                    {
                        
                        if ((___tmpPawns[n].IsColonist && ((Toggle.displayslave == 0 && !___tmpPawns[n].IsSlave) || (Toggle.displayslave == 1 && ___tmpPawns[n].IsSlave))) || (Toggle.displayslave == 2 && ___tmpPawns[n].IsPrisonerOfColony))
                        {
                            ___cachedEntries.Add(new ColonistBar.Entry(___tmpPawns[n], null, num));
                        }
                    }

                    num++;
                }
            }

            ___cachedReorderableGroups.Clear();
            foreach (ColonistBar.Entry cachedEntry in ___cachedEntries)
            {
                _ = cachedEntry;
                ___cachedReorderableGroups.Add(-1);
            }
            
            __instance.drawer.Notify_RecachedEntries();
            ___tmpPawns.Clear();
            ___tmpMaps.Clear();
            ___tmpCaravans.Clear();
            ___drawLocsFinder.CalculateDrawLocs(___cachedDrawLocs, out ___cachedScale, num);
            return true;
        }



        private static readonly Type patchType = typeof(Patches);
    }

}
