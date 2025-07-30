using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using VFED;

namespace ImperialFunctionality
{
    public class QuestNode_SpawnSupplyCrates : QuestNode
    {
        public override void RunInt()
        {
            SetVars(QuestGen.slate);
            var map = QuestGen.slate.Get<Map>("map");
            var rating = QuestGen.quest.challengeRating;
            var items = GenerateSupplyCrates(rating);
            QuestGen.slate.Set("itemPodsContents", items);
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Roofed(map) 
            && !x.Fogged(map) && map.reachability.CanReachColony(x), map, 1f, out var dropSpot))
            {
                dropSpot = DropCellFinder.RandomDropSpot(map);
            }
            QuestGen.slate.Set("dropSpot", dropSpot);
        }

        public List<Thing> GenerateSupplyCrates(int rating)
        {
            var items = new List<Thing>();
            var count = rating * new IntRange(1, 5).RandomInRange;
            for (var i = 0; i < count; i++)
            {
                var crate = ThingMaker.MakeThing(Rand.Bool ? IF_DefOf.VFED_SupplyCrate : IF_DefOf.VFED_BiosecuredCrate);
                if (crate is Building_CrateBiosecured biosecured)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        var (def, ext) = ContrabandManager.AllContraband.RandomElement();
                        var thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null).TryMakeMinified();
                        thing.stackCount = Mathf.Clamp(ext.useCriticalIntel ? Mathf.CeilToInt(3f / ext.intelCost) : Mathf.CeilToInt(10f / ext.intelCost), 1,
                            thing.def.stackLimit);
                        biosecured.innerContainer.TryAdd(thing);
                    }
                }
                else if (crate is Building_SupplyCrate supplyCrate)
                {
                    var (def, ext) = ContrabandManager.AllContraband.RandomElement();
                    var thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null).TryMakeMinified();
                    thing.stackCount = Mathf.Clamp(ext.useCriticalIntel ? Mathf.CeilToInt(3f / ext.intelCost) : Mathf.CeilToInt(10f / ext.intelCost), 1,
                        thing.def.stackLimit);
                    supplyCrate.innerContainer.TryAdd(thing);
                }
                crate.questTags ??= new List<string>();
                crate.questTags.Add("MaxpackTribute");
                items.Add(crate);
            }
            return items;

        }
        public override bool TestRunInt(Slate slate)
        {
            SetVars(slate);
            return true;
        }

        private static void SetVars(Slate slate)
        {
            slate.Set("enemyFaction", Find.FactionManager.FirstFactionOfDef(IF_DefOf.VFEE_Deserters));
            slate.Set("arrivalMode", PawnsArrivalModeDefOf.EdgeWalkIn);
        }
    }
}
