using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class ThoughtWorker_MeeseeksPain : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            float painTotal = pawn.health.hediffSet.PainTotal;
            int stage = Mathf.RoundToInt(painTotal * 10.0f);
            return ThoughtState.ActiveAtStage(stage);
        }
    }
}
