using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using Verse;
using System.Text;

namespace LevelUp
{
    public class LevelUpHealthHediff : Hediff
    {
        public override string LabelInBrackets
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(base.LabelInBrackets);
                if (pawn != null)
                {
                    if (pawn.health.hediffSet.GetFirstHediffOfDef(LevellingHediffDefOf.HealthLevel) != null)
                    {
                        float PawnHealth = pawn.health.hediffSet.GetFirstHediffOfDef(LevellingHediffDefOf.HealthLevel).Severity;
                        float PercentageHealth = PawnHealth - Mathf.FloorToInt(PawnHealth);
                        string Level = "Level " + Mathf.FloorToInt(PawnHealth) + ": " + PercentageHealth.ToStringPercent();
                        stringBuilder.Append(Level);
                        return stringBuilder.ToString();
                    }
                }
                return stringBuilder.ToString();
            }
        }
    }
}
