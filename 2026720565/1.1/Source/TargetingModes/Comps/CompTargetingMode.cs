using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace TargetingModes
{
    public class CompTargetingMode : ThingComp, ITargetModeSettable
    {

        private const int TargetModeResetCheckInterval = 60;

        public Pawn Pawn => parent as Pawn;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction == Faction.OfPlayer && (Pawn == null || (Pawn.training != null && Pawn.training.HasLearned(TrainableDefOf.Obedience)) || Pawn.Drafted))
                yield return TargetingModesUtility.SetTargetModeCommand(this);
        }

        public override void CompTick()
        {
            base.CompTick();

            // For compatibility with existing saves
            if (_targetingMode == null)
                SetTargetingMode(TargetingModesUtility.DefaultTargetingMode);

            // Check if targeting mode should be reset every 60 ticks
            else if (parent.IsHashIntervalTick(TargetModeResetCheckInterval))
            {
                bool updateResetTick  = false;
                if (CanResetTargetingMode(updateResetTick))
                    SetTargetingMode(TargetingModesUtility.DefaultTargetingMode);
                else
                    _resetTargetingModeTick = Find.TickManager.TicksGame + TargModeResetAttemptInterval();
            }
        }

        public bool CanResetTargetingMode(bool updateResetTick)
        {
            updateResetTick = false;

            // World pawns are exempt to player restrictions
            if (parent.Map == null)
                return Find.TickManager.TicksGame < _resetTargetingModeTick;       

            // If player's set it to never reset, if the mode is already default or if it isn't time to update
            if (TargetingModesSettings.TargModeResetFrequencyInt == 0 || _targetingMode == TargetingModesUtility.DefaultTargetingMode || Find.TickManager.TicksGame < _resetTargetingModeTick)
                return false;

            // If the parent pawn is drafted or considered in dangerous combat
            if (Pawn != null && (Pawn.Drafted || GenAI.InDangerousCombat(Pawn)))
            {
                updateResetTick = true;
                return false;
            }  

            // If the parent is a turret and is targeting something
            if (typeof(Building_Turret).Equals(parent)  && ((Building_Turret)parent).CurrentTarget != null)
            {
                updateResetTick = true;
                return false;
            }
                
            return true;
        }

        private int TargModeResetAttemptInterval()
        {
            switch (parent.Faction == Faction.OfPlayer ? TargetingModesSettings.TargModeResetFrequencyInt : 3)
            {
                // 1 = 1d
                case 1:
                    return GenDate.TicksPerDay;
                // 2 = 12h
                case 2:
                    return GenDate.TicksPerHour * 12;
                // 3 = 6h
                case 3:
                    return GenDate.TicksPerHour * 6;
                // 4 = 3h
                case 4:
                    return GenDate.TicksPerHour * 3;
                // other = 1h
                default:
                    return GenDate.TicksPerHour;
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _targetingMode, "targetingMode");
            Scribe_Values.Look(ref _resetTargetingModeTick, "resetTargetingModeTick", -1);
        }

        public override string ToString() =>
            $"CompTargetingMode for {parent} :: _targetingMode={_targetingMode.LabelCap}; _resetTargetingModeTick={_resetTargetingModeTick}; (TicksGame={Find.TickManager.TicksGame})";

        public TargetingModeDef GetTargetingMode() => _targetingMode;

        public void SetTargetingMode(TargetingModeDef targetMode)
        {
            // Actually set the targeting mode
            _targetingMode = targetMode;

            // Set which tick the game will try to reset the mode at
            _resetTargetingModeTick = Find.TickManager.TicksGame + TargModeResetAttemptInterval();
        }

        private TargetingModeDef _targetingMode = TargetingModesUtility.DefaultTargetingMode;
        private int _resetTargetingModeTick = -1;

    }
}
