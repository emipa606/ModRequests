﻿using System.Text;
using RimWorld;
using Verse;

namespace WhatTheHack.Stats;

internal class StatPart_CarryingCapacity : StatPart
{
    public override string ExplanationPart(StatRequest req)
    {
        var sb = new StringBuilder();
        if (req.Thing is not Pawn pawn)
        {
            return sb.ToString();
        }

        foreach (var h in pawn.health.hediffSet.hediffs)
        {
            if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                modExt.carryingCapacityOffset != 0)
            {
                sb.AppendLine(
                    $"{h.def.label}: {modExt.carryingCapacityOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset)}");
            }
        }

        return sb.ToString();
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (req.Thing is not Pawn pawn)
        {
            return;
        }

        float offset = 0;
        foreach (var h in pawn.health.hediffSet.hediffs)
        {
            if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                modExt.carryingCapacityOffset != 0)
            {
                offset += val * modExt.carryingCapacityOffset;
            }
        }

        val += offset;
    }
}