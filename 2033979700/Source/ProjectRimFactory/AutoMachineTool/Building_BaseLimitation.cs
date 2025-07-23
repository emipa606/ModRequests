﻿using ProjectRimFactory.Common;
using RimWorld;
using System.Linq;
using ProjectRimFactory.Common.HarmonyPatches;
using Verse;
using static ProjectRimFactory.AutoMachineTool.Ops;

namespace ProjectRimFactory.AutoMachineTool
{
    public abstract class Building_BaseLimitation<T> : Building_BaseMachine<T>, IProductLimitation where T : Thing
    {
        public int ProductLimitCount { get => this.productLimitCount; set => this.productLimitCount = value; }
        public bool ProductLimitation { get => this.productLimitation; set => this.productLimitation = value; }
        private SlotGroup targetSlotGroup = null;
        public SlotGroup TargetSlotGroup { get => targetSlotGroup; set => targetSlotGroup = value; }
        public bool CountStacks { get => this.countStacks; set => this.countStacks = value; }
        public virtual bool ProductLimitationDisable => false;

        private int productLimitCount = 100;
        private bool productLimitation = false;
        private bool countStacks = false;

        private ILoadReferenceable slotGroupParent = null;
        private string slotGroupParentLabel = null;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.productLimitCount, "productLimitCount", 100);
            Scribe_Values.Look<bool>(ref this.productLimitation, "productLimitation", false);
            Scribe_Values.Look<bool>(ref this.countStacks, "countStacks", false);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.slotGroupParentLabel = this.targetSlotGroup?.parent?.SlotYielderLabel();
                this.slotGroupParent = this.targetSlotGroup?.parent as ILoadReferenceable;
            }
            Scribe_References.Look<ILoadReferenceable>(ref this.slotGroupParent, "slotGroupParent");
            Scribe_Values.Look<string>(ref this.slotGroupParentLabel, "slotGroupParentLabel", null);
        }

        public override void PostMapInit()
        {
            //Maybe rewrite that
            //From my understanding this gets that saved slot group
            this.targetSlotGroup = this.Map.haulDestinationManager.AllGroups
                .Where(g => g.parent.SlotYielderLabel() == this.slotGroupParentLabel)
                .Where(g => Option(slotGroupParent).Fold(true)(p => p == g.parent)).FirstOption().Value;
            base.PostMapInit();
        }

        /* Use IsLimit(Thing thing) below
        [Obsolete("Warning, using IsLimit(ThingDef def) instead of (Thing t) does not work with all storage mods.")]
        public bool IsLimit(ThingDef def)
        {
            if (!this.ProductLimitation)
            {
                return false;
            }
            this.targetSlotGroup = this.targetSlotGroup.Where(s => this.Map.haulDestinationManager.AllGroups.Contains(s));
            return this.targetSlotGroup.Fold(() => this.CountFromMap(def) >= this.ProductLimitCount) // no slotGroup
                (s => !s.Settings.filter.Allows(def)
                || this.CountFromSlot(s, def) >= this.ProductLimitCount 
                || !s.CellsList.Any(c => c.GetFirstItem(this.Map) == null 
                || c.GetFirstItem(this.Map).def == def)); // this is broken anyway.  What if it's a full stack?
        }
        */

        // TODO: This may need to be cached somehow! (possibly by map?)
        // returns true if there IS something that limits adding this thing to storage.
        public bool IsLimit(Thing thing)
        {
            if (!productLimitation) return false;
            
            if (targetSlotGroup == null)
            {
                return this.CountFromMap(thing.def) >= productLimitCount;
            }
            
            // Use the faster limitWatcher if Available
            if (targetSlotGroup.parent is ILimitWatcher limitWatcher)
            {
                if (limitWatcher.ItemIsLimit(thing.def,this.countStacks, productLimitCount)) return true;
            }
            else
            {
                if (this.CheckSlotGroup(targetSlotGroup, thing.def, productLimitCount)) return true;
            }
            
            // Disable Accepts Patch override for this call(s) of IsValidStorageFor
            PatchStorageUtil.SkippAcceptsPatch = true;
            bool isValidCheck = !targetSlotGroup.CellsList.Any(c => c.IsValidStorageFor(this.Map, thing)); 
            PatchStorageUtil.SkippAcceptsPatch = false;
            return isValidCheck;

        }

        private int CountFromMap(ThingDef def)
        {
            return this.countStacks ? this.Map.listerThings.ThingsOfDef(def).Count : this.Map.resourceCounter.GetCount(def);
        }

        private bool CheckSlotGroup(SlotGroup s, ThingDef def,int Limit = int.MaxValue)
        {
            int count = 0;
            var Held = s.HeldThings;
    
            foreach (var t in Held)
            {    
                if (t.def == def)
                {
                    if (this.countStacks)
                    {
                        count++;
                    }
                    else
                    {
                        count += t.stackCount;
                    }
                    if (count >= Limit) return true;
                }
            }
            return false;
        }
    }
}
