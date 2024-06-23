using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace The_Amazing_Tree
{
    [StaticConstructorOnStartup]
    public static class AmazingTreeUtility
    {
        public static bool CanUseMedium(CompPsycastMedium comp, Pawn pawn)
        {
            var focuses = comp.MedicationFocuses;
            for (int i = 0; i < focuses.Count; i++)
            {
                MeditationFocusDef meditationFocusDef = focuses[i];
                if (meditationFocusDef.CanPawnUse(pawn))
                {
                    return true;
                }
            }
            return false;

        }
    }
}


