﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace HediffResourceFramework
{
    public class Verb_ResourceBase : Verb_CastBase
    {
        public IResourceProps ResourceProps
        {
            get
            {
                if (this.tool is IResourceProps resourceProps)
                {
                    return resourceProps;
                }
                else
                {
                    return base.verbProps as VerbResourceProps;
                }
            }
        }
        protected override bool TryCastShot()
        {
            if (base.EquipmentSource != null)
            {
                base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                base.EquipmentSource.GetComp<CompReloadable>()?.UsedOnce();
            }
            return false;
        }
        public override void DrawHighlight(LocalTargetInfo target)
        {
            verbProps.DrawRadiusRing(caster.Position);
            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                DrawHighlightFieldRadiusAroundTargetCustom(target);
            }
        }

        protected void DrawHighlightFieldRadiusAroundTargetCustom(LocalTargetInfo target)
        {
            var targetResourceSettings = ResourceProps.TargetResourceSettings;
            if (targetResourceSettings != null)
            {
                foreach (var hediffOption in targetResourceSettings)
                {
                    if (hediffOption.hediff != null)
                    {
                        if (hediffOption.effectRadius != -1f)
                        {
                            GenDraw.DrawFieldEdges((from x in HediffResourceUtils.GetAllCellsAround(hediffOption, new TargetInfo(target.Cell, Find.CurrentMap), CellRect.SingleCell(target.Cell))
                                                    where x.InBounds(Find.CurrentMap)
                                                    select x).ToList());
                        }
                    }
                }
            }
        }
    }

}
