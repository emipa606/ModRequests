using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BillDoorsLootsAndShelves
{
    public class LootContainer : Building_Casket
    {
        LootContainerContentExtension extension;

        public bool initialized = false;

        Graphic openedGraphic;

        public string manifestLabel;

        public override bool CanOpen
        {
            get
            {
                CompUsable comp = GetComp<CompUsable>();
                if (comp != null)
                {
                    return false;
                }
                return base.CanOpen;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            extension = def.GetModExtension<LootContainerContentExtension>();
            openedGraphic = def.building.fullGraveGraphicData.Graphic;
            if (openedGraphic == null)
            {
                Log.Error(this.def.defName + " uses building.fullGraveGraphicData as open graphic data.");
                openedGraphic = Graphic;
            }
            if (extension == null)
            {
                Log.Error(this.def.defName + " requires LootContainerContentExtension.");
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (innerContainer.NullOrEmpty())
                {
                    return openedGraphic;
                }
                return base.Graphic;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initialized, "initialized", true);
            Scribe_Values.Look(ref manifestLabel, "manifestLabel");
        }

        #region loot Generation
        public void generateLootFromOption()
        {
            if (extension != null && extension.Options != null)
            {
                generateLootFromOption(extension.Options);
            }
        }

        public void generateLootFromOption(List<LootContentDefWeight> Options)
        {
            if (Options.Any())
            {
                float weight(LootContentDefWeight o) { return o.weight; }

                LootContentDefWeight selectedOption = Options.RandomElementByWeight(weight);
                if (selectedOption != null)
                {
                    foreach (LootContentData loot in selectedOption.lootDef.contents)
                    {
                        generateLootThing(loot);
                        generateLootPawn(loot);
                    }
                    manifestLabel = selectedOption.lootDef.GetManifestLabel(innerContainer);
                }
                else
                {
                    LootContentData lootDataFallBack = new LootContentData();
                    lootDataFallBack.PawnKindDef = PawnKindDefOf.Thrumbo;
                    lootDataFallBack.count = new IntRange(1, 1);
                    generateLootThing(lootDataFallBack);
                    Log.Error("An error occured when ");
                }
            }
        }

        public void clearAndGenerateLootFromOption(List<LootContentDefWeight> Options)
        {
            innerContainer.Clear();
            if (Options.Any())
            {
                generateLootFromOption(Options);
            }
            else
            {
                generateLootFromOption();
            }
        }

        public void generateLootThing(LootContentData loot)
        {
            int estimatedStackCount = loot.count.RandomInRange;
            if (estimatedStackCount > 0)
            {
                List<ThingDef> defs = new List<ThingDef>();
                if (loot.ThingDefs.Any())
                {
                    defs = defs.Union(loot.ThingDefs).ToList();
                }

                if (loot.ThingCategory != null)
                {
                    defs = defs.Union(DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.thingCategories != null && x.thingCategories.Contains(loot.ThingCategory))).ToList();
                }
                if (defs.NullOrEmpty())
                {
                    return;
                }
                if (loot.randomFromList)
                {
                    int i = 0;
                    while (estimatedStackCount > 0)
                    {
                        ThingDef thingDef = defs.RandomElement();
                        Thing thing = ThingMaker.MakeThing(thingDef);
                        thing.stackCount = Rand.Range(0, estimatedStackCount);
                        i += thing.stackCount;

                        CompQuality compQuality = thing.TryGetComp<CompQuality>();
                        compQuality?.SetQuality((QualityCategory)Rand.Range((int)loot.qualityRange.min, (int)loot.qualityRange.max), ArtGenerationContext.Outsider);
                        TryAcceptThing(thing);
                        if (i >= estimatedStackCount) { break; }
                    }
                }
                else
                {
                    Thing thing = ThingMaker.MakeThing(defs.RandomElement());
                    thing.stackCount = estimatedStackCount;

                    CompQuality compQuality = thing.TryGetComp<CompQuality>();
                    compQuality?.SetQuality((QualityCategory)Rand.Range((int)loot.qualityRange.min, (int)loot.qualityRange.max), ArtGenerationContext.Outsider);
                    TryAcceptThing(thing);
                }
            }
        }

        public void generateLootPawn(LootContentData loot)
        {
            if (loot.PawnKindDef == null) { return; }
            FactionManager factionManager = Find.FactionManager;

            int estimatedStackCount = loot.count.RandomInRange;
            while (estimatedStackCount > 0)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(loot.PawnKindDef);

                if (loot.faction != null)
                {
                    pawn.SetFaction(factionManager.FirstFactionOfDef(loot.faction));
                }
                if (loot.tamed)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                }
                TryAcceptThing(pawn);
                estimatedStackCount--;

            }
        }
        #endregion

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (DebugSettings.godMode)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = delegate
                {
                    innerContainer.ClearAndDestroyContents();
                    generateLootFromOption();
                };
                command_Action.defaultLabel = "reroll loot";
                yield return command_Action;
            }
        }

        public override string GetInspectString()
        {
            contentsKnown = false;
            string text = base.GetInspectString();
            if (!text.NullOrEmpty())
            {
                text += "\n";
            }
            if (!manifestLabel.NullOrEmpty())
            {
                text += manifestLabel;
            }
            if (DebugSettings.godMode && innerContainer.Any)
            {
                string str = innerContainer.ContentsString;
                text += ("\n" + "DebugContent: " + str.CapitalizeFirst());
            }
            text = text.Trim();
            return text;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "Unlock")
            {
                Open();
            }
        }

        public override void Open()
        {
            Thing remain = null;
            if (extension.RemainDef != null)
            {
                remain = ThingMaker.MakeThing(extension.RemainDef);
            }
            if (remain is Building_ShelfExpanded shelf)
            {
                innerContainer.TryTransferAllToContainer(shelf.tempStorage);
            }
            else
            {
                contentsKnown = true;
                base.Open();
                Notify_ColorChanged();
            }

            if (extension.Leftovers.Any())
            {
                foreach (ThingDefCountClass a in extension.Leftovers)
                {
                    Thing thing = ThingMaker.MakeThing(a.thingDef);
                    thing.stackCount = a.count;
                    GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
                }
            }

            if (remain != null)
            {
                remain.SetFactionDirect(Faction);
                GenSpawn.Spawn(remain, Position, Map, Rotation);
            }
            if (extension.destroyAfterOpened) { Destroy(); }
        }
    }

    public class LootContainerContentExtension : DefModExtension
    {
        public List<LootContentDefWeight> Options;

        public ThingDef RemainDef;

        public List<ThingDefCountClass> Leftovers = new List<ThingDefCountClass>();

        public bool destroyAfterOpened = false;
    }
}
