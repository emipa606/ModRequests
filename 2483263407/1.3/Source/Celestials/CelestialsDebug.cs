using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace Celestials
{
    public static class CelestialsDebug
    {
        [DebugAction("Celestials", "Make Celestial", false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DebugCommand_MakeCelestial(Pawn pawn)
        {
            Comp_Celestials comp = pawn.TryGetComp<Comp_Celestials>();

            if(comp != null && comp.ViableCandidate())
            {
                comp.isCelestial = true;
                comp.AddCelestialHediff();
            }
            else
            {
                Messages.Message("Pawn is not capable of being Celestial. This is not a bug, they simply do not meet the requirements.", MessageTypeDefOf.RejectInput);
            }
        }

        [DebugAction("Celestials", "Make Mortal", false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DebugCommand_MakeMortal(Pawn pawn)
        {
            Comp_Celestials comp = pawn.TryGetComp<Comp_Celestials>();

            if (comp != null && comp.ViableCandidate())
            {
                comp.isCelestial = false;
                comp.RemoveCelestialHediff();
            }
            else
            {
                Messages.Message("Pawn already cannot be a celestial, if they have the hediff, you must remove it manually with Remove Hediff instead.", MessageTypeDefOf.RejectInput);
            }
        }
    }
}
