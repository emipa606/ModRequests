﻿using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace JecsTools
{
    public class Hediff_TransformedPart : Hediff_AddedPart
    {
        private readonly List<Hediff_MissingPart> temporarilyRemovedParts = new List<Hediff_MissingPart>();

        public override bool ShouldRemove =>
            this.GetHediffComp<HediffComp_Disappears>() is HediffComp_Disappears hdc_Disappears && hdc_Disappears.CompShouldRemove;

        public override string TipStringExtra
        {
            get
            {
                var stringBuilder = new StringBuilder();
                var baseString = base.TipStringExtra;
                if (!baseString.NullOrEmpty())
                    stringBuilder.Append(baseString);
                if (def.GetHediffCompProps<HediffCompProperties_VerbGiver>()?.tools is List<Tool> tools)
                    for (var i = 0; i < tools.Count; i++)
                        stringBuilder.AppendLine("Damage".Translate() + ": " + tools[i].power);
                return stringBuilder.ToString();
            }
        }

        /// Nothing should happen.
        public override void PostAdd(DamageInfo? dinfo)
        {
            if (Part == null)
            {
                Log.Error("Part is null. It should be set before PostAdd for " + def + ".");
                return;
            }
            pawn.health.RestorePart(Part, this, false);
            temporarilyRemovedParts.Clear();
            for (var i = 0; i < Part.parts.Count; i++)
            {
                var hediff_MissingPart =
                    (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, null);
                hediff_MissingPart.IsFresh = false;
                hediff_MissingPart.lastInjury = null;
                hediff_MissingPart.Part = Part.parts[i];
                pawn.health.hediffSet.AddDirect(hediff_MissingPart, null);
                temporarilyRemovedParts.Add(hediff_MissingPart);
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            pawn.health.RestorePart(Part, this, false);
            //for (var i = 0; i < Part.parts.Count; i++)
            //{
            //}
        }
    }
}
