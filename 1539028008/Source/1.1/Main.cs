using System;
using Verse;
using HarmonyLib;
using System.Reflection;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace WhereIsMyWeapon {
    [StaticConstructorOnStartup]
    class Main {
        public static bool inMakeDowned = false;

        static Main() {
            var harmony = new Harmony("com.tammybee.whereismyweapon");
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
                    comp.equipments.Add(equipment, equipment.stackCount);
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
            if (pawn == null) {
                return;
            }
            if (WhereIsMyWeaponMod.Settings.retakeTrigger != WhereIsMyWeaponSettings.RetakeTrigger.Undowned) {
                return;
            }
            CompLastEquipment comp = pawn.GetComp<CompLastEquipment>();
            if (comp != null) {
                comp.ReequipAndRetake();
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

        private static void AddLastEquipment(CompLastEquipment comp, Job job) {
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

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("HealthTick")]
    class Patch_Pawn_HealthTracker_HealthTick {
        static void Postfix(Pawn_HealthTracker __instance) {
            if (__instance.Dead) {
                return;
            }
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.RaceProps.IsFlesh && pawn.IsHashIntervalTick(600) && !__instance.HasHediffsNeedingTendByPlayer(false) && !HealthAIUtility.ShouldSeekMedicalRest(pawn) && !__instance.hediffSet.HasTendedAndHealingInjury()) {
                if (WhereIsMyWeaponMod.Settings.retakeTrigger != WhereIsMyWeaponSettings.RetakeTrigger.FullyHealed) {
                    return;
                }

                CompLastEquipment comp = pawn.GetComp<CompLastEquipment>();
                if (comp != null) {
                    comp.TryReequipAndRetake();
                }
            }
        }

        /*
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            List<CodeInstruction> cis = new List<CodeInstruction>(instructions);

            LocalBuilder lb = generator.DeclareLocal(typeof(bool));

            Func<object, int, bool> isLocalBuilder = delegate (object operand, int localIndex) {
                return operand is LocalBuilder && ((LocalBuilder)operand).LocalIndex == localIndex;
            };

            int insertPos = -1;
            for (int i=0;i<cis.Count - 1;i++) {
                if (cis[i].opcode == OpCodes.Stloc_S && cis[i+1].opcode == OpCodes.Ldloc_S && isLocalBuilder(cis[i].operand,7) && isLocalBuilder(cis[i+1].operand, 7)) {
                    insertPos = i;
                    Log.Message(cis[i].operand.GetType() + "/" + cis[i].operand.ToString());
                    break;
                }
            }

            if (insertPos != -1) {
                List<CodeInstruction> injections = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_S,cis[insertPos].operand),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Pawn_HealthTracker_HealthTick), "RetakeAndReequip")),
                };
                cis.InsertRange(insertPos + 1, injections);
            }

            foreach (CodeInstruction ci in cis) {
                yield return ci;
            }
        }

        private static void RetakeAndReequip(Pawn_HealthTracker health,bool flag) {
            Pawn pawn = Traverse.Create(health).Field("pawn").GetValue<Pawn>();
            if (flag && !health.HasHediffsNeedingTendByPlayer(false) && !HealthAIUtility.ShouldSeekMedicalRest(pawn) && !health.hediffSet.HasTendedAndHealingInjury()) {
                if (WhereIsMyWeaponMod.Settings.retakeTrigger != WhereIsMyWeaponSettings.RetakeTrigger.FullyHealed) {
                    return;
                }

                CompLastEquipment comp = pawn.GetComp<CompLastEquipment>();
                if (comp != null) {
                    comp.ReequipAndRetake();
                }
            }
        }
        */
    }
}
