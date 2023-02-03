﻿using System;
using System.Text.RegularExpressions;
using Verse;

namespace FacialStuff
{
    public class FacePreset : ILoadReferenceable, IExposable
    {
        public int uniqueId;

        public string label;

        public ThingFilter filter = new ThingFilter();

        public static readonly Regex ValidNameRegex = new Regex("^[a-zA-Z0-9 '\\-]*$");

        public FacePreset()
        {
        }

        public FacePreset(int uniqueId, string label)
        {
            this.uniqueId = uniqueId;
            this.label = label;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.uniqueId, "uniqueId", 0, false);
            Scribe_Values.LookValue<string>(ref this.label, "label", null, false);
            Scribe_Deep.LookDeep<ThingFilter>(ref this.filter, "filter", new object[0]);
        }

        public string GetUniqueLoadID()
        {
            return "FacePreset_" + this.label + this.uniqueId.ToString();
        }
    }
}