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
    public class Thought_HaveUniqueItem : Thought_Situational
    {
        protected override float BaseMoodOffset
        {
            get
            {
                var baseValue = base.BaseMoodOffset;
                var uniqueItem = UniqueItemsUtils.GetUniqueThingFrom(pawn);
                if (uniqueItem != null)
                {
                    return baseValue / UniqueItemsSettings.uniqueThingsMaxCount[uniqueItem.def.defName];
                }
                return baseValue;
            }
        }
    }
    public class ThoughtWorker_HaveUniqueItem : ThoughtWorker
	{
        protected override ThoughtState CurrentStateInternal(Pawn p)
		{
            if (UniqueItemsUtils.GetUniqueThingFrom(p) != null)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
		}
	}
}
