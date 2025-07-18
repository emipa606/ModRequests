using RimWorld;
using UnityEngine;
using Verse;

namespace Kraltech_Industries.Command;

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

    // Token: 0x0600E22C RID: 57900 RVA: 0x0009103E File Offset: 0x0008F23E
    public Command_VerbReloadable(CompReloadable_Weapon comp)
    {
        this.comp = comp;
    }

    // Token: 0x1700234C RID: 9036
    // (get) Token: 0x0600E22D RID: 57901 RVA: 0x0009104D File Offset: 0x0008F24D
    public override string TopRightLabel
    {
        get { return this.comp.GizmoExtraLabel; }
    }

    // Token: 0x1700234D RID: 9037
    // (get) Token: 0x0600E22E RID: 57902 RVA: 0x0043F174 File Offset: 0x0043D374
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

    // Token: 0x0600E22F RID: 57903 RVA: 0x0009105A File Offset: 0x0008F25A
    public override void GizmoUpdateOnMouseover()
    {
        this.verb.DrawHighlight(LocalTargetInfo.Invalid);
    }

    // Token: 0x0600E230 RID: 57904 RVA: 0x0043F1A0 File Offset: 0x0043D3A0
    public override bool GroupsWith(Gizmo other)
    {
        Command_VerbReloadable command_VerbOwner;
        return base.GroupsWith(other) && (command_VerbOwner = (other as Command_VerbReloadable)) != null && this.comp.parent.def == command_VerbOwner.comp.parent.def && this.comp.parent.def.mergeVerbGizmos;
    }

    // Token: 0x04009236 RID: 37430
    private readonly CompReloadable_Weapon comp;

    // Token: 0x04009237 RID: 37431
    public Color? overrideColor;
}
