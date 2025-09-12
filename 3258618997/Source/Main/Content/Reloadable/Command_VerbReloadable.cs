using RimWorld;
using UnityEngine;
using Verse;

namespace Necro;


public class Command_VerbReloadable : Command_VerbTarget
{
    public override string Label
    {
        get
        {
            if (this.comp.parent.def.mergeVerbGizmos || Find.Selector.SelectedPawns.Count < 2)
            {
                return base.Label;
            }

            return base.Label + " (" + this.verb.caster.LabelShortCap + ")";
        }
    }

    public Command_VerbReloadable(CompReloadable_Weapon comp)
    {
        this.comp = comp;
    }

    public override string TopRightLabel
    {
        get { return this.comp.GizmoExtraLabel; }
    }

    public override Color IconDrawColor
    {
        get
        {
            Color? color = this.overrideColor;
            if (color == null)
            {
                return base.IconDrawColor;
            }

            return color.GetValueOrDefault();
        }
    }

    public override void GizmoUpdateOnMouseover()
    {
        this.verb.DrawHighlight(LocalTargetInfo.Invalid);
    }

    public override bool GroupsWith(Gizmo other)
    {
        Command_VerbReloadable command_VerbOwner;
        return base.GroupsWith(other) && (command_VerbOwner = (other as Command_VerbReloadable)) != null && this.comp.parent.def == command_VerbOwner.comp.parent.def && this.comp.parent.def.mergeVerbGizmos;
    }

    private readonly CompReloadable_Weapon comp;

    public Color? overrideColor;
}
