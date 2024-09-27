using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace UniqueItems
{
    public class Thought_UniqueItem : Thought_Memory
    {
        public ThingDef uniqueItemDef;
        protected override float BaseMoodOffset => uniqueItemDef != null && UniqueItemsSettings.uniqueThingsMaxCount.ContainsKey(uniqueItemDef.defName) ? base.BaseMoodOffset / UniqueItemsSettings.uniqueThingsMaxCount[uniqueItemDef.defName] : 0f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref uniqueItemDef, "uniqueItemDef");
        }
    }


}
