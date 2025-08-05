using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using Verse.Sound;
using Verse.AI;
using CombatExtended;
using System.Runtime.Remoting.Messaging;

namespace BDsPlasmaWeapon
{
    public class CompExtinguisherFiller : CompResource
    {
        private int tickNextCheck = 0;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent.def.tickerType == TickerType.Never)
            {
                Log.Error("CompExtinguisherFiller needs a ticker type greather than never to work!");
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tickNextCheck, "tickNextCheck", 0);
        }

        public override void CompTick()
        {
            base.CompTick();
            CheckRefill();
        }

        public override void CompTickRare()
        {
            base.CompTick();
            CheckRefill();
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            CheckRefill();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (parent != null && !parent.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }

            if (Prefs.DevMode)
            {
                Command_Action checkRefill = new Command_Action
                {
                    action = new Action(CheckRefillNow),
                    defaultLabel = "Debug: Check Refill Now",
                };
                yield return checkRefill;
            }
            yield break;
        }

        private void CheckRefill()
        {
            int tick = Find.TickManager.TicksGame;

            if (tick > tickNextCheck && PipeNet != null && PipeNet.Stored > 0)
            {
                tickNextCheck = tick + 600;
                CheckRefillNow();
            }
        }

        private void CheckRefillNow()
        {
            List<Thing> things = GridsUtility.GetThingList(parent.Position, parent.Map);
            foreach (Thing thing in things)
            {
                if (thing.def == ThingDefOf.BDP_FireExtinguisher)
                {
                    CompReloadableFromFiller extinguisher = thing.TryGetComp<CompReloadableFromFiller>();
                    if (extinguisher.NeedsReload(true))
                    {
                        float storedGasInNet = PipeNet.Stored;
                        int emptySpace = extinguisher.emptySpace;
                        if (emptySpace > storedGasInNet)
                        {
                            extinguisher.RefillGas(storedGasInNet);
                            PipeNet.DrawAmongStorage(storedGasInNet, PipeNet.storages);
                        }
                        else
                        {
                            extinguisher.RefillGas(emptySpace);
                            PipeNet.DrawAmongStorage(emptySpace, PipeNet.storages);
                        }
                    }
                }
            }
        }
    }
}
