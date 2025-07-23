﻿using HarmonyLib;
using ProjectRimFactory.Common;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ProjectRimFactory.Storage.UI
{


    // Somebody toucha my spaghet code
    // TODO: Use harmony to make ITab_Items actually a ITab_DeepStorage_Inventory and add right click menu
    // Only do above if LWM is installed ofc - rider
    [StaticConstructorOnStartup]
    public class ITab_Items : ITab
    {
        static ITab_Items()
        {
            DropUI = (Texture2D)AccessTools.Field(AccessTools.TypeByName("Verse.TexButton"), "Drop").GetValue(null);
            menuUI = (Texture2D)AccessTools.Field(AccessTools.TypeByName("Verse.TexButton"), "ToggleLog").GetValue(null); ; // ToggleLog
        }

        private static Texture2D menuUI;
        private static Texture2D DropUI;
        private Vector2 scrollPos;
        private float scrollViewHeight;
        private string oldSearchQuery;
        private string searchQuery;
        private List<Thing> itemsToShow;

        public ITab_Items()
        {
            size = new Vector2(480f, 480f);
            labelKey = "PRFItemsTab";
        }

        private static ILinkableStorageParent oldSelectedMassStorageUnit = null;

        public ILinkableStorageParent SelectedMassStorageUnit =>
            IOPortSelected ? SelectedIOPort.BoundStorageUnit : (ILinkableStorageParent)SelThing;

        public override bool IsVisible
        {
            get
            {
                if (IOPortSelected)
                {
                    return SelectedIOPort.BoundStorageUnit?.CanReceiveIO ?? false;
                }
                return SelThing is ILinkableStorageParent;
            }
        }

        protected bool IOPortSelected => SelThing is Building_StorageUnitIOBase;

        protected Building_StorageUnitIOBase SelectedIOPort => SelThing as Building_StorageUnitIOBase;

        private static Dictionary<Thing, List<Pawn>> canBeConsumedby = new Dictionary<Thing, List<Pawn>>();

        private static List<Pawn> pawnCanReach_Touch_Deadly = new List<Pawn>();

        private static List<Pawn> pawnCanReach_Oncell_Deadly = new List<Pawn>();

        private static Dictionary<Thing, int> thing_MaxHitPoints = new Dictionary<Thing, int>();

        private struct thingIconTextureData
        {
            public thingIconTextureData(Texture texture, Color color)
            {
                this.texture = texture;
                this.color = color;
            }

            public Texture texture;
            public Color color;
        }
        private static Dictionary<Thing, thingIconTextureData> thingIconCache = new Dictionary<Thing, thingIconTextureData>();


        private static bool itemIsVisible(float curY, float ViewRecthight, float scrollY, float rowHight = 28f)
        {
            //The item is above the view (including a safty margin of one item)
            if ((curY + rowHight - scrollY) < 0)
            {
                return false;
            }

            // the item is above the lower limit (including a safty margin of one item)
            if ((curY - rowHight - scrollY - ViewRecthight) < 0)
            {
                return true;
            }

            //Item is below the lower limit
            return false;
        }

        private bool Search(string source, string target)
        {
            if (ProjectRimFactory.Common.ProjectRimFactory_ModSettings.PRF_UseFuzzySearch)
            {
                return source.NormalizedFuzzyStrength(target) < FuzzySearch.Strength.Strong;
            }
            else
            {
                return source.Contains(target);
            }
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            var curY = 0f;
            var frame = new Rect(10f, 10f, size.x - 10f, size.y - 10f);
            GUI.BeginGroup(frame);
            Widgets.ListSeparator(ref curY, frame.width, labelKey.Translate());
            curY += 5f;

            // Sniper: t.Label also contains the count (Rice x20) do we want that?
            // Rider: This new search method is REALLLYYYYYY FAST
            // This is meant to stop a possible spam call of the Fuzzy Search
            if (itemsToShow == null || searchQuery != oldSearchQuery || SelectedMassStorageUnit.StoredItemsCount != itemsToShow.Count || oldSelectedMassStorageUnit == null || oldSelectedMassStorageUnit != SelectedMassStorageUnit)
            {
                itemsToShow = new List<Thing>(from Thing t in SelectedMassStorageUnit.StoredItems
                                              where string.IsNullOrEmpty(searchQuery) || Search(t.GetInnerIfMinified().Label.ToLower(), searchQuery.ToLower())
                                              orderby t.Label descending
                                              select t);
                oldSearchQuery = searchQuery;
            }
            oldSelectedMassStorageUnit = SelectedMassStorageUnit;

            var text = SelectedMassStorageUnit.GetITabString(itemsToShow.Count);
            var MainTabText = new Rect(8f, curY, frame.width - 16f, Text.CalcHeight(text, frame.width - 16f));
            Widgets.Label(MainTabText, text);
            curY += MainTabText.height;
            searchQuery = Widgets.TextField(new Rect(frame.x, curY, MainTabText.width - 16f, 25f),
                oldSearchQuery);
            curY += 28f;

            GUI.color = Color.white;
            var outRect = new Rect(0f, curY, frame.width, frame.height - curY);
            var viewRect = new Rect(0f, 0f, outRect.width - 16f, scrollViewHeight);
            // Scrollview Start
            Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
            curY = 0f;
            if (itemsToShow.Count < 1)
            {
                Widgets.Label(viewRect, "PRFItemsTabLabel_Empty".Translate());
                curY += 22f;
            }

            // Iterate backwards to compensate for removing elements from enumerable
            // Learned this is an issue with List-like structures in AP CS 1A

            List<Pawn> pawns = SelectedMassStorageUnit.Map.mapPawns.FreeColonists;


            //Do it once as they are all on the same spot in the DSU
            //Even if is where to have multible sport's that way should work I think 
            pawnCanReach_Touch_Deadly = pawns.Where(p => p.CanReach(SelectedMassStorageUnit.GetTargetInfo, PathEndMode.ClosestTouch, Danger.Deadly)).ToList();
            pawnCanReach_Oncell_Deadly = pawns.Where(p => p.CanReach(SelectedMassStorageUnit.GetTargetInfo, PathEndMode.OnCell, Danger.Deadly)).ToList();



            for (var i = itemsToShow.Count - 1; i >= 0; i--)
            {
                //Check if we need to display it
                if (!itemIsVisible(curY, outRect.height, scrollPos.y))
                {
                    curY += 28;
                    continue;
                }

                Thing thing = itemsToShow[i];

                //Construct cache
                if (!canBeConsumedby.ContainsKey(thing))
                {
                    canBeConsumedby.Add(thing, pawns.Where(p => p.RaceProps.CanEverEat(thing) == true).ToList());
                }
                if (!thing_MaxHitPoints.ContainsKey(thing))
                {
                    thing_MaxHitPoints.Add(thing, thing.MaxHitPoints);
                }
                if (!thingIconCache.ContainsKey(thing))
                {
                    Color color;
                    Texture texture = CommonGUIFunctions.GetThingTextue(new Rect(4f, curY, 28f, 28f), thing, out color);

                    thingIconCache.Add(thing, new thingIconTextureData(texture, color));
                }

                DrawThingRow(ref curY, viewRect.width, thing, pawns);
            }
            if (Event.current.type == EventType.Layout) scrollViewHeight = curY + 30f;
            //Scrollview End
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        // Attempt at mimicking LWM Deep Storage
        // Credits to LWM Deep Storage :)
        private void DrawThingRow(ref float y, float width, Thing thing, List<Pawn> colonists)
        {
            //if (thing == null || !thing.Spawned) return; // not here, whatever happened...


            //each call to LabelCap also accesses MaxHitPoints therefor it is read here slightly diffrently;
            string labelMoCount;
            if (thing is Corpse)
            {
                labelMoCount = (thing as Corpse).Label;
            }
            else
            {
                labelMoCount = GenLabel.ThingLabel(thing.GetInnerIfMinified(), thing.stackCount, false);
            }




            string labelCap = labelMoCount.CapitalizeFirst(thing.def);


            width -= 24f;
            // row to hold the item in the GUI
            Widgets.InfoCardButton(width, y, thing);
            // rect.width -= 84f;
            width -= 24f;

            var checkmarkRect = new Rect(width, y, 24f, 24f);
            var isItemForbidden = !thing.IsForbidden(Faction.OfPlayer);
            var forbidRowItem = isItemForbidden;
            TooltipHandler.TipRegion(checkmarkRect,
                isItemForbidden ? "CommandNotForbiddenDesc".Translate() : "CommandForbiddenDesc".Translate());

            Widgets.Checkbox(checkmarkRect.x, checkmarkRect.y, ref isItemForbidden);
            if (isItemForbidden != forbidRowItem) thing.SetForbidden(!isItemForbidden, false);
            width -= 24f;
            var dropRect = new Rect(width, y, 24f, 24f);
            TooltipHandler.TipRegion(dropRect, "PRF_DropThing".Translate(labelMoCount));
            if (Widgets.ButtonImage(dropRect, DropUI, Color.gray, Color.white, false))
            {
                dropThing(thing);
            }

            Pawn p = colonists?.Where(col => col.IsColonistPlayerControlled && !col.Dead && col.Spawned && !col.Downed).ToArray().FirstOrFallback<Pawn>(null) ?? null;
            if (p != null && ChoicesForThing(thing, p, labelMoCount).Count > 0)
            {
                width -= 24f;
                var pawnInteract = new Rect(width, y, 24f, 24f);
                if (Widgets.ButtonImage(pawnInteract, menuUI, Color.gray, Color.white, false))
                {
                    var opts = new List<FloatMenuOption>();
                    foreach (var pawn in from Pawn col in colonists
                                         where col.IsColonistPlayerControlled && !col.Dead && col.Spawned && !col.Downed
                                         select col)
                    {
                        var choices =
                            ChoicesForThing(thing, pawn, labelMoCount);
                        if (choices.Count > 0)
                            opts.Add(new FloatMenuOption(pawn.Name.ToStringFull,
                                () => { Find.WindowStack.Add(new FloatMenu(choices)); }));
                    }

                    Find.WindowStack.Add(new FloatMenu(opts));
                }
            }

            var thingRow = new Rect(0f, y, width, 28f);
            // Highlights the row upon mousing over
            if (Mouse.IsOver(thingRow))
            {
                GUI.color = ITab_Pawn_Gear.HighlightColor;
                GUI.DrawTexture(thingRow, TexUI.HighlightTex);
            }


            //we need to cache the Graphic to save performence 
            // Widgets.ThingIcon dos not do that
            // Draws the icon of the thingDef in the row


            if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            {
                thingIconTextureData data = thingIconCache[thing];
                CommonGUIFunctions.ThingIcon(new Rect(4f, y, 28f, 28f), thing, data.texture, data.color);
            }


            // Draws the item name + info
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            var itemName = new Rect(36f, y, thingRow.width - 36f, thingRow.height);
            Text.WordWrap = false;
            // LabelCap is interesting to me(rider)
            // Really useful I would think
            // LabelCap == "Wort x75"
            Widgets.Label(itemName, labelCap.Truncate(itemName.width));
            Text.WordWrap = true;

            // For the toolpit
            var text2 = labelCap;

            // if uses hitpoints draw it
            if (thing.def.useHitPoints)
                text2 = string.Concat(labelCap, "\n", thing.HitPoints, " / ", thing_MaxHitPoints[thing]);

            // Custom rightclick menu
            TooltipHandler.TipRegion(thingRow, text2);

            y += 28f;
        }

        // Little helper method to stop a redundant definition I was about to do.
        private void dropThing(Thing thing)
        {
            var item = SelectedMassStorageUnit
                .StoredItems.Where(i => i == thing).ToList();
            if (IOPortSelected && SelectedIOPort.OutputItem(item[0]))
            {
                itemsToShow.Remove(thing);
            }
            else if (SelectedMassStorageUnit.OutputItem(item[0]))
            {
                itemsToShow.Remove(thing);
            }
        }


        // Decompiled code is painful to read... Continue at your own risk
        // TODO: Replace this with a cleaner solution
        // Maybe explore FloatMenuMakerMap
        public static List<FloatMenuOption> ChoicesForThing(Thing thing, Pawn pawn, string thingLabelShort)
        {
            var opts = new List<FloatMenuOption>();
            var t = thing;
            string labelShort = "";
            string label = thingLabelShort;
            if (thing.stackCount > 1) label += " x" + thing.stackCount.ToStringCached();



            // Copied from FloatMenuMakerMap.AddHumanlikeOrders
            if (t.def.ingestible != null && canBeConsumedby[t].Contains(pawn) && t.IngestibleNow)
            {
                string text;
                if (t.def.ingestible.ingestCommandString.NullOrEmpty())
                    text = "ConsumeThing".Translate(thingLabelShort, t);
                else
                    text = string.Format(t.def.ingestible.ingestCommandString, thingLabelShort);
                if (!t.IsSociallyProper(pawn)) text = text + " (" + "ReservedForPrisoners".Translate() + ")";
                FloatMenuOption item7;
                if (t.def.IsNonMedicalDrug && pawn.IsTeetotaler())
                {
                    item7 = new FloatMenuOption(text + " (" + TraitDefOf.DrugDesire.DataAtDegree(-1).label + ")", null);
                }
                else if (!pawnCanReach_Oncell_Deadly.Contains(pawn))
                {
                    item7 = new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null);
                }
                else
                {
                    var priority2 = !(t is Corpse) ? MenuOptionPriority.Default : MenuOptionPriority.Low;
                    item7 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
                    {
                        t.SetForbidden(false);
                        var job = new Job(JobDefOf.Ingest, t);
                        job.count = FoodUtility.WillIngestStackCountOf(pawn, t.def,
                            t.GetStatValue(StatDefOf.Nutrition));
                        pawn.jobs.TryTakeOrderedJob(job);
                    }, priority2), pawn, t);
                }

                opts.Add(item7);
            }

            // Add equipment commands
            // Copied from FloatMenuMakerMap.AddHumanlikeOrders
            if (thing is ThingWithComps equipment && equipment.GetComp<CompEquippable>() != null)
            {
                //ThingWithComps != Thing --> Label is diffrent implementing the override string LabelNoCount
                labelShort = thingLabelShort;
                if (equipment.AllComps != null)
                {
                    int i = 0;
                    for (int count = equipment.AllComps.Count; i < count; i++)
                    {
                        labelShort = equipment.AllComps[i].TransformLabel(labelShort);
                    }
                }

                string cantEquipReason = null;
                FloatMenuOption item4;
                if (equipment.def.IsWeapon && pawn.WorkTagIsDisabled(WorkTags.Violent))
                {
                    item4 = new FloatMenuOption(
                        "CannotEquip".Translate(labelShort) + " (" +
                        "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn) + ")", null);
                }
                else if (!pawnCanReach_Touch_Deadly.Contains(pawn))
                {
                    item4 = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "NoPath".Translate() + ")",
                        null);
                }
                else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    item4 = new FloatMenuOption(
                        "CannotEquip".Translate(labelShort) + " (" + "Incapable".Translate() + ")", null);
                }
                else if (!EquipmentUtility.CanEquip(thing, pawn, out cantEquipReason))
                {
                    item4 = new FloatMenuOption(
                        "CannotEquip".Translate(labelShort) + " (" + cantEquipReason + ")", null);
                }
                else
                {
                    string text5 = "Equip".Translate(labelShort);
                    if (equipment.def.IsRangedWeapon && pawn.story != null &&
                        pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                        text5 = text5 + " " + "EquipWarningBrawler".Translate();
                    item4 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text5, delegate
                    {
                        equipment.SetForbidden(false);
                        pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Equip, equipment));
                        FleckMaker.Static(equipment.DrawPos, equipment.Map, FleckDefOf.FeedbackEquip);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons,
                            KnowledgeAmount.Total);
                    }, MenuOptionPriority.High), pawn, equipment);
                }

                opts.Add(item4);
            }

            // Add clothing commands
            var apparel = thing as Apparel;
            if (apparel != null)
            {
                //replaced apparel.Label label (the former would spawm MaxHitPonts)
                FloatMenuOption item5;
                if (!pawnCanReach_Touch_Deadly.Contains(pawn))
                    item5 = new FloatMenuOption(
                        "CannotWear".Translate(label, apparel) + " (" + "NoPath".Translate() + ")", null);
                else if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
                    item5 = new FloatMenuOption(
                        "CannotWear".Translate(label, apparel) + " (" +
                        "CannotWearBecauseOfMissingBodyParts".Translate() + ")", null);
                else /*Apparel has no override for LabelShort therefor we can still use labelShort from ThingWithComps*/
                    item5 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
                        "ForceWear".Translate(labelShort, apparel), delegate
                        {
                            apparel.SetForbidden(false);
                            var job = new Job(JobDefOf.Wear, apparel);
                            pawn.jobs.TryTakeOrderedJob(job);
                        }, MenuOptionPriority.High), pawn, apparel);
                opts.Add(item5);
            }

            // Add caravan commands

            if (pawn.IsFormingCaravan())
                if (thing != null && thing.def.EverHaulable)
                {
                    var packTarget = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(pawn) ?? pawn;
                    var jobDef = packTarget != pawn ? JobDefOf.GiveToPackAnimal : JobDefOf.TakeInventory;
                    //replaced thing.Label label (the former would spawm MaxHitPonts)
                    if (!pawnCanReach_Touch_Deadly.Contains(pawn))
                    {
                        opts.Add(new FloatMenuOption(
                            "CannotLoadIntoCaravan".Translate(label, thing) + " (" + "NoPath".Translate() + ")",
                            null));
                    }
                    else if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, thing, 1))
                    {
                        opts.Add(new FloatMenuOption(
                            "CannotLoadIntoCaravan".Translate(label, thing) + " (" + "TooHeavy".Translate() + ")",
                            null));
                    }
                    else
                    {
                        var lordJob = (LordJob_FormAndSendCaravan)pawn.GetLord().LordJob;
                        var capacityLeft = CaravanFormingUtility.CapacityLeft(lordJob);
                        if (thing.stackCount == 1)
                        {
                            var capacityLeft4 = capacityLeft - thing.GetStatValue(StatDefOf.Mass);
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
                                CaravanFormingUtility.AppendOverweightInfo(
                                    "LoadIntoCaravan".Translate(label, thing), capacityLeft4), delegate
                                {
                                    thing.SetForbidden(false, false);
                                    var job = new Job(jobDef, thing);
                                    job.count = 1;
                                    job.checkEncumbrance = packTarget == pawn;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }, MenuOptionPriority.High), pawn, thing));
                        }
                        else
                        {
                            if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, thing, thing.stackCount))
                            {
                                opts.Add(new FloatMenuOption(
                                    "CannotLoadIntoCaravanAll".Translate(label, thing) + " (" +
                                    "TooHeavy".Translate() + ")", null));
                            }
                            else
                            {
                                var capacityLeft2 =
                                    capacityLeft - thing.stackCount * thing.GetStatValue(StatDefOf.Mass);
                                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
                                    CaravanFormingUtility.AppendOverweightInfo(
                                        "LoadIntoCaravanAll".Translate(label, thing), capacityLeft2), delegate
                                    {
                                        thing.SetForbidden(false, false);
                                        var job = new Job(jobDef, thing);
                                        job.count = thing.stackCount;
                                        job.checkEncumbrance = packTarget == pawn;
                                        pawn.jobs.TryTakeOrderedJob(job);
                                    }, MenuOptionPriority.High), pawn, thing));
                            }

                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(
                                "LoadIntoCaravanSome".Translate(thingLabelShort, thing), delegate
                                {
                                    var to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(packTarget, thing),
                                        thing.stackCount);
                                    var window = new Dialog_Slider(delegate (int val)
                                    {
                                        var capacityLeft3 = capacityLeft - val * thing.GetStatValue(StatDefOf.Mass);
                                        return CaravanFormingUtility.AppendOverweightInfo(
                                            string.Format("LoadIntoCaravanCount".Translate(thingLabelShort, thing),
                                                val), capacityLeft3);
                                    }, 1, to, delegate (int count)
                                    {
                                        thing.SetForbidden(false, false);
                                        var job = new Job(jobDef, thing);
                                        job.count = count;
                                        job.checkEncumbrance = packTarget == pawn;
                                        pawn.jobs.TryTakeOrderedJob(job);
                                    });
                                    Find.WindowStack.Add(window);
                                }, MenuOptionPriority.High), pawn, thing));
                        }
                    }
                }
            return opts;
        }

        public override void OnOpen()
        {
            base.OnOpen();

            //Clear all the cache Lists on Tab Open to Prevent unessesary memmory usage / "leek"
            canBeConsumedby.Clear();
            pawnCanReach_Touch_Deadly.Clear();
            pawnCanReach_Oncell_Deadly.Clear();
            thing_MaxHitPoints.Clear();
        }
    }
}