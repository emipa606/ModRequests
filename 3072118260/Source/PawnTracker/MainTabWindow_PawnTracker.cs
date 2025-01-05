using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PawnTrackerMain {
    public class MainTabWindow_PawnTracker : MainTabWindow {
        
        private List<Pawn> DisplayPawns => Current.Game.GetComponent<CHGameComp>().TrackedPawns;

        public override void PreOpen()
        {
            
            var dialog = new Dialog_PawnTracker_Main();
            Find.WindowStack.Add(dialog);
        }

        public override void DoWindowContents(Rect rect)
        {
            return;
        }



    }
}

