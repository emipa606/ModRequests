﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace Celestials
{
    public class CompProperties_Celestials : CompProperties
    {
        public CompProperties_Celestials()
        {
            compClass = typeof(Comp_Celestials);
        }
    }
}
