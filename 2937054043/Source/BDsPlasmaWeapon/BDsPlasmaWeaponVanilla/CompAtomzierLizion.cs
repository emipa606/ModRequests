using System;
using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace BDsPlasmaWeaponVanilla
{
    public class CompAtomzierLizion : CompResource
    {
        public CompProperties_AtomzierLizion Props
        {
            get
            {
                return (CompProperties_AtomzierLizion)props;
            }
        }

        private int cachedStack = 0;

        private CompAtomizer compAtomizer;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compAtomizer = parent.GetComp<CompAtomizer>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (compAtomizer != null && !compAtomizer.innerContainer.NullOrEmpty())
            {
                int stack = compAtomizer.innerContainer[0].stackCount;
                if (cachedStack > stack)
                {
                    PipeNet.DistributeAmongStorage(Props.ProductionPerAtomize);
                }
                cachedStack = stack;
            }
        }
    }

    public class CompProperties_AtomzierLizion : CompProperties_Resource
    {
        public int ProductionPerAtomize = 100;


        public CompProperties_AtomzierLizion()
        {
            compClass = typeof(CompAtomzierLizion);
        }
    }
}
