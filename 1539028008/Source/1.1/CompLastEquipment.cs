using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhereIsMyWeapon {
    public class CompLastEquipment : ThingComp {
        public ThingList equipments = new ThingList();
        public ThingList inventory = new ThingList();

        public bool TryReequipAndRetake() {
            if ((this.equipments != null && !this.equipments.list.NullOrEmpty()) || (this.inventory != null && !this.inventory.list.NullOrEmpty())) {
                ReequipAndRetake();
                return true;
            }
            return false;
        }

        public void ReequipAndRetake() {
            Pawn pawn = this.parent as Pawn;
            if (pawn != null && pawn.Spawned && pawn.jobs != null) {
                Log.Message("ReequipAndRetake:" + pawn.Label);

                if (pawn.CurJob != null) {
                    if (!pawn.CurJob.def.isIdle) {
                        if (pawn.CurJob.TryMakePreToilReservations(pawn, true)) {
                            pawn.jobs.jobQueue.EnqueueFirst(pawn.CurJob);
                        } else {
                            pawn.ClearReservationsForJob(pawn.CurJob);
                        }
                    }
                    //pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }

                if (this.inventory != null && this.inventory.list != null) {
                    if (WhereIsMyWeaponMod.Settings.retake) {
                        foreach (ThingPair inv in this.inventory.list) {
                            if (inv.thing != null && inv.count > 0 && inv.thing.Spawned && pawn.Map == inv.thing.Map && pawn.Position.DistanceTo(inv.thing.Position) < WhereIsMyWeaponMod.Settings.searchDistance) {
                                Job job = new Job(JobDefOf.TakeInventory, inv.thing) {
                                    count = inv.count
                                };
                                job.playerForced = true;
                                if (job.TryMakePreToilReservations(pawn, true)) {
                                    pawn.jobs.jobQueue.EnqueueFirst(job);
                                } else {
                                    pawn.ClearReservationsForJob(job);
                                }
                            }
                        }
                    }
                    this.inventory.list.Clear();
                } else {
                    this.inventory.list = new List<ThingPair>();
                }

                if (this.equipments != null && this.equipments.list != null) {
                    if (WhereIsMyWeaponMod.Settings.reequip) {
                        foreach (ThingPair equipment in this.equipments.list) {
                            if (equipment.thing != null && equipment.count > 0 && equipment.thing.Spawned && pawn.Map == equipment.thing.Map && pawn.Position.DistanceTo(equipment.thing.Position) < WhereIsMyWeaponMod.Settings.searchDistance) {
                                Job job = new Job(JobDefOf.Equip, equipment.thing);
                                job.playerForced = true;
                                if (job.TryMakePreToilReservations(pawn, true)) {
                                    pawn.jobs.jobQueue.EnqueueFirst(job);
                                } else {
                                    pawn.ClearReservationsForJob(job);
                                }
                            }
                        }
                    }
                    this.equipments.list.Clear();
                } else {
                    this.equipments.list = new List<ThingPair>();
                }

                if (pawn.jobs.jobQueue.Count > 0 && pawn.jobs.jobQueue.Peek().job.CanBeginNow(pawn, false)) {
                    QueuedJob queuedJob2 = pawn.jobs.jobQueue.Dequeue();
                    //Log.Message("Start:" + queuedJob2.job.def.defName);
                    pawn.jobs.StartJob(queuedJob2.job, JobCondition.InterruptForced);
                }
            }
        }

        public void Clear() {
            if (equipments != null) {
                equipments.Clear();
            } else {
                equipments = new ThingList();
            }

            if (inventory != null) {
                inventory.Clear();
            } else {
                inventory = new ThingList();
            }
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref this.equipments, "wimw_equipments");
            Scribe_Deep.Look(ref this.inventory, "wimw_inventory");
        }
    }
}
