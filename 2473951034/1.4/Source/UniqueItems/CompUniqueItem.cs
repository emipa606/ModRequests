using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace UniqueItems
{
    public class CompProperties_UniqueItem : CompProperties
    {
        public string labelUniquePrefix;
        public string labelRarePrefix;

        public string artDescriptionPostfix;

        public int maxCount = 1;
        public CompProperties_UniqueItem()
        {
            this.compClass = typeof(CompUniqueItem);
        }
    }
    public class CompUniqueItem : ThingComp
    {
        public static Dictionary<ThingDef, HashSet<Thing>> spawnedInstances = new Dictionary<ThingDef, HashSet<Thing>>();
        public static Dictionary<ThingDef, HashSet<Thing>> instances = new Dictionary<ThingDef, HashSet<Thing>>();
        public Pawn crafter;
        public bool isNoticedByPlayer;
        public CompProperties_UniqueItem Props => this.props as CompProperties_UniqueItem;
        public override string TransformLabel(string label)
        {
            if (this.parent.def.IsRare())
            {
                return Props.labelRarePrefix + base.TransformLabel(label);
            }
            return Props.labelUniquePrefix + base.TransformLabel(label);
        }
        public static Dictionary<ThingDef, HashSet<Thing>> SpawnedInstances
        {
            get
            {
                foreach (var data in spawnedInstances)
                {
                    data.Value.RemoveWhere(x => x.DestroyedOrNullOrHaveNoHolder());
                }
                return spawnedInstances;
            }
        }
        public static Dictionary<ThingDef, HashSet<Thing>> Instances
        {
            get
            {
                foreach (var data in instances)
                {
                    data.Value.RemoveWhere(x => x.DestroyedOrNullOrHaveNoHolder());
                }
                return instances;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            AddInstance();
            AddSpawnedInstance();
            //if (FinalizeLoadingPatch.isLoading)
            //{
            //    isNoticedByPlayer = true;
            //}
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref crafter, "crafter");
            AddInstance();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (crafter != null)
            {
                var thought = (Thought_UniqueItem)ThoughtMaker.MakeThought(UniqueItemsDefOf.DM_LostCraftedUniqueItem);
                thought.uniqueItemDef = this.parent.def;
                crafter.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
                crafter.needs?.mood?.thoughts?.memories.RemoveMemoriesOfDef(UniqueItemsDefOf.DM_CraftedUniqueItem);
            }
            if (instances.ContainsKey(this.parent.def))
            {
                instances[this.parent.def].Remove(this.parent);
            }
            if (spawnedInstances.ContainsKey(this.parent.def))
            {
                spawnedInstances[this.parent.def].Remove(this.parent);
            }
            base.PostDestroy(mode, previousMap);
        }
        public void AddInstance()
        {
            if (instances.ContainsKey(this.parent.def))
            {
                instances[this.parent.def].Add(this.parent);
            }
            else
            {
                instances[this.parent.def] = new HashSet<Thing> { this.parent };
            }
        }

        private void AddSpawnedInstance()
        {
            if (spawnedInstances.ContainsKey(this.parent.def))
            {
                spawnedInstances[this.parent.def].Add(this.parent);
            }
            else
            {
                spawnedInstances[this.parent.def] = new HashSet<Thing> { this.parent };
            }
        }

        public bool MaxUniqueCountExceeded(Thing thing)
        {

            if (Instances.TryGetValue(thing.def, out var list) && list.Where(x => x != thing).Count() >= Props.maxCount)
            {
                return true;
            }
            return false;
        }
    }
}
