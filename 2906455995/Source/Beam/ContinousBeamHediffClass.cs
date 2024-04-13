using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace Beam
{
    public class ContinousBeamHediffClass : HediffWithComps
    {
        public override string SeverityLabel
        {
            get
            {
                if (!(def.lethalSeverity <= 0f))
                {
                    return (Severity / (def.lethalSeverity/2)).ToStringPercent();
                }
                return null;
            }
        }
    }
}