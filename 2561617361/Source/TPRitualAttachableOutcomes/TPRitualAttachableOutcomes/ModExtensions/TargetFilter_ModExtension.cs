﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TPRitualAttachableOutcomes
{

    class TargetFilter_ModExtension : DefModExtension
    {
        public List<string> extraCandidates = new List<string>();
        public string missingDesc = "";

        public void ExposeData()
        {
            Scribe_Values.Look(ref extraCandidates, "extraCandidates");
        }
    }
}
