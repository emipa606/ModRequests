using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{

    [Obsolete]
    public class CompChangeableProjectileMagazine : CompChangeableProjectile, IGenericBar
    {
        public new CompProperties_ChangeableProjectileMagazine Props => (CompProperties_ChangeableProjectileMagazine)props;
        public new bool Loaded => loadedCount > 0;

        public bool Full => loadedCount >= Props.magazineSize;

        public int CountNeeded => Props.magazineSize - loadedCount;

        Gizmo_GenericBar gizmo;

        Gizmo_GenericBar Gizmo
        {
            get
            {
                if (gizmo == null)
                {
                    gizmo = new Gizmo_GenericBar() { mag = this };
                }
                return gizmo;
            }
        }

        public float MaxFillableBarLength => Props.magazineSize;

        public float CurrentFillableBarLength => loadedCount;

        public string FillableBarTitle => LoadedShell != null ? LoadedShell.label : parent.Label;

        public Color? FillableBarColor => null;

        public Color? FillableBarBGColor => null;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.ParentHolder == null || ((Thing)parent.ParentHolder).Faction.IsPlayer)
            {
                yield return Gizmo;
            }
        }

        public new void LoadShell(ThingDef shell, int count)
        {
            int countPre = loadedCount;
            base.LoadShell(shell, count);
            loadedCount = countPre + count;
        }
    }

    public class CompProperties_ChangeableProjectileMagazine : CompProperties
    {
        public CompProperties_ChangeableProjectileMagazine()
        {
            compClass = typeof(CompChangeableProjectileMagazineI);
        }

        public int magazineSize = 1;

        float stageCount = 0;

        public float StageCount => stageCount > 0 ? stageCount : magazineSize;

        public int reloadTicks = 100;
    }
}
