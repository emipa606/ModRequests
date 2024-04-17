using System;
using Verse;
using Harmony;
using System.Reflection;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;

namespace WhereIsMyWeapon {
    [StaticConstructorOnStartup]
    class Main {
        public static bool inMakeDowned = false;

        static Main() {
            var harmony = HarmonyInstance.Create("com.tammybee.whereismyweapon");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker))]
    [HarmonyPatch("DropAllEquipment")]
    class Patch_Pawn_EquipmentTracker_DropAllEquipment {
        static void Prefix(Pawn_EquipmentTracker __instance, ref bool forbid) {
            if (!Main.inMakeDowned) {
                return;
            }

            Pawn pawn = __instance.ParentHolder as Pawn;
            if (pawn == null) {
                return;
            }

            if (pawn.IsColonist) {
                forbid = false;
            }
            CompLastEquipment comp = pawn.GetComp<CompLastEquipment>();
            if (comp != null) {
                if (comp.equipments == null || comp.equipments.list == null) {
                    return;
                }
                foreach (ThingWithComps equipment in __instance.AllEquipmentListForReading) {
                    comp.equipments.Add(equipment,equipment.stackCount);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_InventoryTracker))]
    [HarmonyPatch("DropAllNearPawn")]
    class Patch_Pawn_InventoryTracker_DropAllNearPawn {
        static void Prefix(Pawn_InventoryTracker __instance, ref bool forbid) {
            if (!Main.inMakeDowned) {
                return;
            }

            Pawn pawn = __instance.pawn;
            if (pawn == null) {
                return;
            }

            if (pawn.IsColonist) {
                forbid = false;
            }
            CompLastEquipment comp = pawn.GetComp<CompLastEquipment>();
            if (comp != null) {
                if (comp.inventory == null || comp.inventory.list == null) {
                    return;
                }
                foreach (ThingWithComps inv in __instance.innerContainer) {
                    comp.inventory.Add(inv, inv.stackCount);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("MakeUndowned")]
    class Patch_Pawn_HealthTracker_MakeUndowned {
        static void Postfix(Pawn_HealthTracker __instance) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            CompLastEquipment comp = pawn.GetComp<CompLastEquipment>();
            if (comp != null && pawn.Spawned && pawn.jobs != null) {
                if (pawn.CurJob != null) {
                    if (!pawn.CurJob.def.isIdle) {
                        if (pawn.CurJob.TryMakePreToilReservations(pawn, true)) {
                            pawn.jobs.jobQueue.EnqueueFirst(pawn.CurJob);
                        } else {
                            pawn.ClearReservationsForJob(pawn.CurJob);
                        }
                    }
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }

                if (WhereIsMyWeaponMod.Settings.retake) {
                    foreach (ThingPair inv in comp.inventory.list) {
                        if (inv.thing.Spawned && pawn.Map == inv.thing.Map && pawn.Position.DistanceTo(inv.thing.Position) < WhereIsMyWeaponMod.Settings.searchDistance) {
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
                comp.inventory.list.Clear();

                if (WhereIsMyWeaponMod.Settings.reequip) {
                    foreach (ThingPair equipment in comp.equipments.list) {
                        if (equipment.thing.Spawned && pawn.Map == equipment.thing.Map && pawn.Position.DistanceTo(equipment.thing.Position) < WhereIsMyWeaponMod.Settings.searchDistance) {
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
                comp.equipments.list.Clear();
                
                
                if (pawn.jobs.jobQueue.Count > 0 && pawn.jobs.jobQueue.Peek().job.CanBeginNow(pawn, false)) {
                    QueuedJob queuedJob2 = pawn.jobs.jobQueue.Dequeue();
                    //Log.Message("Start:" + queuedJob2.job.def.defName);
                    pawn.jobs.StartJob(queuedJob2.job, JobCondition.InterruptForced);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("MakeDowned")]
    class Patch_Pawn_HealthTracker_MakeDowned {
        static void Prefix(Pawn_HealthTracker __instance) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            //Log.Message("Hoge:" + (pawn != null));
            CompLastEquipment comp = pawn.GetComp<CompLastEquipment>();
            //Log.Message(pawn.Name?.ToStringShort + "/" + (pawn.CurJob != null) + "/" + (comp != null) + "/" + (comp?.equipments?.list != null) + "/" + (comp?.inventory?.list != null));
            if (comp != null && pawn.Spawned && pawn.jobs != null) {
                comp.Clear();
                AddLastEquipment(comp, pawn.CurJob);
                //Log.Message(pawn.Name?.ToStringShort + "/" + (pawn.jobs.jobQueue != null));
                if (pawn.jobs != null && pawn.jobs.jobQueue != null) {
                    foreach (QueuedJob qj in pawn.jobs.jobQueue) {
                        AddLastEquipment(comp, qj.job);
                    }
                }
            }

            Main.inMakeDowned = true;
        }

        static void Postfix() {
            Main.inMakeDowned = false;
        }

        private static void AddLastEquipment(CompLastEquipment comp,Job job) {
            if (comp != null && job != null && job.targetA != null && job.targetA.Thing != null) {
                if (job.def == JobDefOf.Equip) {
                    //Log.Message("Equip:" + job.targetA.Thing.Label);
                    if (comp.equipments == null) {
                        comp.equipments = new ThingList();
                    }
                    comp.equipments.Add(job.targetA.Thing, 1);
                } else if (job.def == JobDefOf.TakeInventory) {
                    //Log.Message("TakeInventory:" + job.targetA.Thing.Label);
                    if (comp.inventory == null) {
                        comp.inventory = new ThingList();
                    }
                    comp.inventory.Add(job.targetA.Thing, job.count);
                }
            }
        }
    }
}
