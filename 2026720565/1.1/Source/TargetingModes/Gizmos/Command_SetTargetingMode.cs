﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TargetingModes
{

    [StaticConstructorOnStartup]
    public class Command_SetTargetingMode : Command
    {

        private static Texture2D SetTargetingModeTex = ContentFinder<Texture2D>.Get("UI/TargetingModes/MultipleModes");
        public ITargetModeSettable settable;
        public List<ITargetModeSettable> settables;

        public Command_SetTargetingMode()
        {
            TargetingModeDef targetingMode = null;
            bool multiplePawnsSelected = false;
            foreach (Thing thing in Find.Selector.SelectedObjects)
            {

                if (thing.TryGetComp<CompTargetingMode>() !=null)
                {

                    if (thing.TryGetComp<CompTargetingMode>().GetTargetingMode() != null)
                    {
                        //single pawn?
                    }
                    else
                    {
                        //multiple pawns?
                        multiplePawnsSelected = true;
                        break;
                    }
                    targetingMode = thing.TryGetComp<CompTargetingMode>().GetTargetingMode();
                }

            }

            icon = (multiplePawnsSelected) ? SetTargetingModeTex : targetingMode.uiIcon;
            defaultLabel = (multiplePawnsSelected) ? "CommandSetTargetingModeMulti".Translate() : "CommandSetTargetingMode".Translate(targetingMode.LabelCap);
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (settables == null)
                settables = new List<ITargetModeSettable>();
            if (!settables.Contains(settable))
                settables.Add(settable);
            TargetingModes.SortBy(t => -t.displayOrder, t => t.LabelCap.Resolve());
            List<FloatMenuOption> targetingModeOptions = new List<FloatMenuOption>();
            foreach (TargetingModeDef targetMode in TargetingModes)
                targetingModeOptions.Add(new FloatMenuOption(FloatMenuLabel(targetMode),
                    () =>
                    {
                        for (int i = 0; i < settables.Count; i++)
                            settables[i].SetTargetingMode(targetMode);
                    }));
            Find.WindowStack.Add(new FloatMenu(targetingModeOptions));
        }

        private string FloatMenuLabel(TargetingModeDef targetingMode)
        {
            string label = targetingMode.LabelCap;
            if (targetingMode.HitChanceFactor != 1f)
                label += $" (x{targetingMode.HitChanceFactor.ToStringPercent()} Acc)";
            return label;
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            if (settables == null)
                settables = new List<ITargetModeSettable>();
            settables.Add(((Command_SetTargetingMode)other).settable);
            return false;
        }

        public List<TargetingModeDef> TargetingModes => DefDatabase<TargetingModeDef>.AllDefsListForReading;

    }

}
