using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using BillDoorsUnifiedHaulJob;

namespace BillDoorsFramework
{
    public class CompChangeableProjectileMagazineI : CompChangeableProjectile, IRefuelable, IGenericBar
    {
        public new CompProperties_ChangeableProjectileMagazine Props => (CompProperties_ChangeableProjectileMagazine)props;
        public new bool Loaded => loadedCount > 0;

        public bool Full => loadedCount >= Props.magazineSize;

        Gizmo_GenericBar gizmo;

        Gizmo_GenericBar Gizmo
        {
            get
            {
                if (gizmo == null)
                {
                    gizmo = new Gizmo_GenericBar()
                    {
                        mag = this,
                        stageCount = Props.StageCount,
                    };
                }
                return gizmo;
            }
        }

        #region interfaces
        public float MaxFillableBarLength => Props.magazineSize;

        public float CurrentFillableBarLength => loadedCount;

        public string FillableBarTitle => LoadedShell != null ? LoadedShell.label : parent.Label;

        public Color? FillableBarColor => null;

        public Color? FillableBarBGColor => null;


        public ThingRequest CurrentRequest => ThingRequest.ForGroup(ThingRequestGroup.Shell);

        public int RequestAmount => Props.magazineSize - loadedCount;

        public bool ShouldRefuelNow => !Loaded;

        public Predicate<Thing> FuelValidator => t => allowedShellsSettings.AllowedToAccept(t);

        public float SearchRadius => 100;

        public int RefuelWaitTick => Props.reloadTicks;

        private Thing foundFuel = null;
        public virtual Thing FoundFuel { get => foundFuel; set => foundFuel = value; }

        public Thing Parent => parent;

        public void RefuelEffect(Thing fuel, Pawn pawn, params object[] parms)
        {
            LoadShell(fuel.def, fuel.stackCount);
            fuel.Destroy();
        }
        #endregion
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
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            IRefuelableUtil.SpawnRegister(this);
        }

        public override void PostDeSpawn(Map map)
        {
            IRefuelableUtil.Deregister(map, this);
        }

        public string GetUniqueLoadID()
        {
            return $"CompChangableMag_{parent.AllComps.IndexOf(this)}_" + parent.ThingID;
        }
    }
}
