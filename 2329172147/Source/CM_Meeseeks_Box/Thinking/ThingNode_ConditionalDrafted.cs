using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace CM_Meeseeks_Box
{
    public class ThingNode_ConditionalDrafted : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return (pawn.GetComp<CompMeeseeksMemory>() != null && pawn.Drafted);
        }
    }
}
