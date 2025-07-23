﻿using ProjectRimFactory.Common;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ProjectRimFactory.AutoMachineTool
{
    [StaticConstructorOnStartup] // for registering settings
    public class Building_ItemPuller : Building_BaseLimitation<Thing>, IStorageSetting, IStoreSettingsParent
    {
        public Building_ItemPuller()
        {
            this.outputToEntireStockpile = true;
        }
        protected bool active = false;
        protected bool takeForbiddenItems = true;
        protected bool takeSingleItems = false;
        public override Graphic Graphic => this.def.GetModExtension<ModExtension_Graphic>()?.GetByName(GetGraphicName()) ?? base.Graphic;
        protected const float defaultWorkAmount = 120f;

        private string GetGraphicName()
        {
            string name = null;
            if (this.OutputSides)
            {
                name += this.right ? "Right" : "Left";
            }
            if (this.active)
            {
                name += "Working";
            }
            return name;
        }

        public bool StorageTabVisible => true;

        public StorageSettings settings;

        public StorageSettings GetStoreSettings()
        {
            if (settings == null)
            {
                settings = new StorageSettings();
                //To "Prevent" a null Refrence as GetParentStoreSettings() seems to be null on first Placing the Building
                if (GetParentStoreSettings() != null)
                {
                    settings.CopyFrom(GetParentStoreSettings());
                }
            }
            return settings;
        }
        public StorageSettings GetParentStoreSettings() => def.building?.fixedStorageSettings;

        protected StorageSettings storageSettings;
        public StorageSettings StorageSettings => this.storageSettings;

        private bool right = false;

        public bool Getright => right;

        private bool OutputSides => this.def.GetModExtension<ModExtension_Puller>()?.outputSides ?? false;

        protected override LookMode WorkingLookMode => LookMode.Deep; // despawned
        /// <summary>
        /// Whether the puller grabs a single item or the entire stack
        /// </summary>
        public bool TakeSingleItems { get => takeSingleItems; set => takeSingleItems = value; }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref this.active, "active", false);
            Scribe_Values.Look<bool>(ref this.right, "right", false);
            Scribe_Deep.Look(ref settings, "settings", new object[] { this });
            Scribe_Values.Look<bool>(ref this.takeForbiddenItems, "takeForbidden", true);
            Scribe_Values.Look<bool>(ref this.takeSingleItems, "takeSingleItems", false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.settings = GetStoreSettings(); // force init
            this.forcePlace = false;

            if (!respawningAfterLoad) Messages.Message("PRF.NeedToTurnOnPuller".Translate(),
                     this, MessageTypeDefOf.CautionInput);
        }

        protected override void Reset()
        {
            if (this.working != null)
            {
                if (!working.Spawned)
                {
                    GenPlace.TryPlaceThing(working, Position, Map, ThingPlaceMode.Near, null, p => p != Position);
                }
            }
            base.Reset();
        }

        protected override TargetInfo ProgressBarTarget()
        {
            return this;
        }


        protected virtual Thing TargetThing()
        {
            Thing target = null;
            var allThings = (this.Position + this.Rotation.Opposite.FacingCell).AllThingsInCellForUse(this.Map,true,true).ToArray();

            var AllReserved = this.Map.reservationManager.AllReservedThings().ToHashSet();

 
            for (int i = 0; i < allThings.Length; i++)
            {
                var thing = allThings[i];
                if (!this.takeForbiddenItems && thing.IsForbidden(Faction.OfPlayer)) continue;
                if (!this.settings.AllowedToAccept(thing)) continue;
                if (AllReserved.Contains(thing)) continue;

                if (!this.IsLimit(thing))
                {
                    target = thing;
                    break;
                }
            }

            if (target == null) return target;
            if (this.takeSingleItems) return (target.SplitOff(1));
            // SplitOff ensures any item-removal effects happen:
            return (target.SplitOff(target.stackCount));
        }

        public override IntVec3 OutputCell()
        {
            return this.def.GetModExtension<ModExtension_Puller>().GetOutputCell(this.Position, this.Rotation, this.right);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            foreach (Gizmo g2 in StorageSettingsClipboard.CopyPasteGizmosFor(settings))
                yield return g2;
            yield return new Command_Toggle()
            {
                isActive = () => this.active,
                toggleAction = () => this.active = !this.active,
                defaultLabel = "PRF.AutoMachineTool.Puller.SwitchActiveLabel".Translate(),
                defaultDesc = "PRF.AutoMachineTool.Puller.SwitchActiveDesc".Translate(),
                icon = RS.PlayIcon
            };
            yield return new Command_Toggle()
            {
                isActive = () => this.takeForbiddenItems,
                toggleAction = () => this.takeForbiddenItems = !this.takeForbiddenItems,
                defaultLabel = "PRF.Puller.TakeForbiddenItems".Translate(),
                defaultDesc = "PRF.Puller.TakeForbiddenItemsDesc".Translate(),
                icon = TexCommand.ForbidOff
            };
            if (this.OutputSides)
            {
                yield return new Command_Action()
                {
                    action = () => this.right = !this.right,
                    defaultLabel = "PRF.AutoMachineTool.Puller.SwitchOutputSideLabel".Translate(),
                    defaultDesc = "PRF.AutoMachineTool.Puller.SwitchOutputSideDesc".Translate(),
                    icon = RS.OutputDirectionIcon
                };
            }
        }

        protected override bool IsActive()
        {
            return base.IsActive() && this.active;
        }

        protected override bool WorkInterruption(Thing working)
        {
            return false;
        }

        protected override bool TryStartWorking(out Thing target, out float workAmount)
        {
            workAmount = Building_ItemPuller.defaultWorkAmount; // 120
            target = TargetThing();
            if (target?.Spawned == true) target.DeSpawn();
            return target != null;
        }

        protected override bool FinishWorking(Thing working, out List<Thing> products)
        {
            // why do we need to create a *new* list???  Why not just append
            //   directly to this.products()??  It IS the C# object-oriented
            //   way (altho, if Nobo comes from a background where variables
            //   are immutable that might explain the choice?) Nevertheless,
            //   I will use and return the current instantiation of products
            this.products.Append(working);
            products = this.products;
            return true;
        }
        protected override void Placing()
        {
            // unforbid any items picked up before they are put down:
            if (!products.NullOrEmpty())
            {
                foreach (Thing t in products)
                    if (t.IsForbidden(Faction.OfPlayer))
                        t.SetForbidden(false);
            }
            base.Placing();
        }
        public override bool AcceptsThing(Thing newThing, IPRF_Building giver)
        {
            if (this.State == WorkingState.Ready)
            {
                this.ClearActions();
                if (newThing.Spawned) newThing.DeSpawn();
                this.ForceStartWork(newThing, defaultWorkAmount);
                return true;
            }
            return false;
        }

        static Building_ItemPuller()
        {
            Common.ITab_ProductionSettings.RegisterSetting(ShouldShowSingleVsStackSetting,
                                           ExtraHeightNeeded, DoSettingsWindowContents);
        }
        public static bool ShouldShowSingleVsStackSetting(Thing thing)
        {
            return thing is Building_ItemPuller;
        }
        public static float ExtraHeightNeeded(Thing t)
        {
            return 21f;
        }
        public static void DoSettingsWindowContents(Thing t, Listing_Standard ls)
        {
            if (t is Building_ItemPuller puller)
            {
                bool tmp = puller.takeSingleItems;
                ls.CheckboxLabeled("PRF.Puller.takeSingleItemsHmm".Translate(), ref tmp, "PRF.Puller.takeSingleItemsDesc".Translate());
                if (tmp != puller.takeSingleItems) puller.TakeSingleItems = tmp;
            }
        }

        public void Notify_SettingsChanged()
        {
            // Might allow us to cache StorageSettings
            // unsure about the potential gains / current load
        }
    }
}
