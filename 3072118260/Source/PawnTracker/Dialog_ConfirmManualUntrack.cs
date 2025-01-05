using RimWorld;
using RimWorld.Planet;
using System;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using static PawnTrackerMain.PawnTrackerUtility;


namespace PawnTrackerMain 
{
    public class Dialog_ConfirmManualUntrack : Window {
        Pawn pawn;

        private void Setup()
        {
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = false;
            this.soundAppear = SoundDefOf.InfoCard_Open;
            this.soundClose = SoundDefOf.InfoCard_Close;
            this.resizeable = false;
        }

        public Dialog_ConfirmManualUntrack(Pawn p) 
        {
            this.pawn = p;
        }
        
        protected override void SetInitialSizeAndPosition()
        {
            float x = 500f;
            float y = 250f;
            this.windowRect = new Rect((float) (((double) UI.screenWidth - (double) x) / 2.0), (float) (((double) UI.screenHeight - (double) y) / 2.0), x, y);
            this.windowRect = this.windowRect.Rounded();
        }

        public override void DoWindowContents(Rect rect)
        {
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(new Rect(0f, 0f, rect.width, 300f), "Are you 100% sure you want to untrack this pawn?\n\nExisting events will not be deleted, but future events will not be added and the pawn will not appear in the main menu.\n\nThis CANNOT BE UNDONE");
            if (Widgets.ButtonText(new Rect(rect.width / 3f - 150f, (rect.height / 2f) + 30f, 200f, 40f),"Yes! Get 'em gone!"))
            {
                RemovePawn(pawn);
            }
            if (Widgets.ButtonText(new Rect((rect.width / 3f * 2f) - 50f, (rect.height / 2f) + 30f, 200f, 40f),"On second thought..."))
            {
                Close();
            }
            
            Text.Anchor = TextAnchor.UpperLeft;
            return;
        }

        private void RemovePawn(Pawn p)
        {
            if (GetComp().ManuallyUntracked.Contains(p) || !GetComp().TrackedPawns.Contains(p))
                return;
            GetComp().ManuallyUntracked.Add(p);
            GetComp().TrackedPawns.Remove(p);
            Log.Message($"Removed: {GetComp().ManuallyUntracked.Contains(p)}, {GetComp().TrackedPawns.Contains(p)}");
            this.Close();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            Setup();        
        }
    }
}
