using HarmonyLib;
using Reload.Components;
using Reload.Defs;
using Reload.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Linq;
using Verse.AI;
using RimWorld.Planet;
using RimWorld.BaseGen;
using AchtungMod;

namespace Reload
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static MethodInfo _cleanupCurrentJob = typeof(Pawn_JobTracker).GetMethod("CleanupCurrentJob", BindingFlags.Instance | BindingFlags.NonPublic);

        static MethodInfo _canPawnTakeOpportunisticJob = typeof(Pawn_JobTracker).GetMethod("CanPawnTakeOpportunisticJob", BindingFlags.Instance | BindingFlags.NonPublic);

        static MethodInfo _tryFindAndStartJob = typeof(Pawn_JobTracker).GetMethod("TryFindAndStartJob", BindingFlags.Instance | BindingFlags.NonPublic);

        static HarmonyPatches()
        {
            var harmony = new Harmony("nkm.reload");
            harmony.Patch(AccessTools.Method(typeof(Verb), "Available"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("PostFix_Available")));
            harmony.Patch(AccessTools.Method(typeof(Verb), "TryCastNextBurstShot"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("PreFix_TryCastNextBurstShot")));
            harmony.Patch(AccessTools.Method(typeof(Verb), "TryCastNextBurstShot"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("PostFix_TryCastNextBurstShot")));
            harmony.Patch(AccessTools.Method(typeof(WorkGiver_HunterHunt), "HasHuntingWeapon"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Postfix_HasHuntingWeapon")));
            // Reload while moving
            harmony.Patch(AccessTools.Method(typeof(Pawn), "TicksPerMove"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_TicksPerMove")));
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "PawnGotoAction"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_PawnGotoAction")));
            harmony.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "PatherArrived"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_PatherArrived")));
            harmony.Patch(AccessTools.Method(typeof(Pawn_JobTracker), "EndCurrentJob"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_EndCurrentJob")));
            // AI
            harmony.Patch(AccessTools.Method(typeof(JobGiver_AIFightEnemy), "TryGiveJob"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Postfix_AIFightEnemy_TryGiveJob")));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_ConfigurableHostilityResponse), "TryGiveJob"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Postfix_ConfigurableHostilityResponse_TryGiveJob")));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn_InventoryTracker), "FirstUnloadableThing"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_FirstUnloadableThing")));
            
            harmony.Patch(AccessTools.Method(typeof(TraderKindDef), "WillTrade"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_WillTrade")));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateGearFor"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Postfix_GenerateGearFor")));
            harmony.Patch(AccessTools.Method(typeof(ThingSetMaker_TraderStock), "Generate", new Type[] { typeof(ThingSetMakerParams), typeof(List<Thing>) }), null, null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Transpiler_Generate")));            
            harmony.Patch(AccessTools.Method(typeof(ThingFilter), "SetFromPreset"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_SetFromPreset")));
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Postfix_AddHumanlikeOrders")));
            harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Postfix_GetGizmos"))); 
            harmony.Patch(AccessTools.PropertyGetter(typeof(ThingDef), "PlayerAcquirable"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_PlayerAcquirable")));
            harmony.Patch(AccessTools.PropertyGetter(typeof(RecipeDef), "AvailableNow"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_AvailableNow")));

            if (Compatibility.Achtung)
            {
                Type colonistType = AccessTools.TypeByName("AchtungMod.Colonist");
                if (colonistType != null)
                {
                    harmony.Patch(AccessTools.Method(colonistType, "OrderTo"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Prefix_OrderTo")));
                }
            }
        }
        public static void PostFix_Available(Verb __instance, ref bool __result)
        {
            if (!__result)
                return;

            var comp = __instance?.EquipmentSource?.GetComp<CompReload>();
            if (comp != null && !comp.CanBeUsed)
            {
                __result = false;
                comp.Reload();
            }
        }
        public static bool PreFix_TryCastNextBurstShot(Verb __instance, ref int ___burstShotsLeft)
        {
            var comp = __instance?.EquipmentSource?.GetComp<CompReload>();
            if (comp != null)
                ___burstShotsLeft = Math.Min(___burstShotsLeft, comp.remainingAmmo);



            return true;
        }
        public static void PostFix_TryCastNextBurstShot(Verb __instance)
        {
            var comp = __instance?.EquipmentSource?.GetComp<CompReload>();
            if (comp == null || __instance.IsMeleeAttack)
                return;

            comp.UsedOnce();
            if (!comp.CanBeUsed)
            {
                if(comp.Holder.CurJobDef == JobDefOf.Hunt)
                    comp.Holder.jobs.StopAll();
                comp.Reload();
            }
        }
        public static void Postfix_HasHuntingWeapon(Pawn p, ref bool __result)
        {
            if (__result)
            {
                var comp = p.equipment.Primary.GetComp<CompReload>();
                if (comp != null)
                    __result = !Setting.EnableAmmo || comp.CanBeUsed || comp.CanLoad();
            }
        }
        public static bool Prefix_TicksPerMove(Pawn __instance, ref int __result, bool diagonal)
        {
            if (__instance.CurJobDef != ReloadJobDefOf.R_Reload)
                return true;

            float num = __instance.GetStatValue(StatDefOf.MoveSpeed);
            CompReload comp = __instance.CurJob.targetB.Thing.TryGetComp<CompReload>();
            num *= (100f - (float)comp.Props.moveSpeedPenalty) / 100f;

            if (RestraintsUtility.InRestraints(__instance))
            {
                num *= 0.35f;
            }

            if (__instance.carryTracker != null && __instance.carryTracker.CarriedThing != null && __instance.carryTracker.CarriedThing.def.category == ThingCategory.Pawn)
            {
                num *= 0.6f;
            }

            float num2 = num / 60f;
            float num3;
            if (num2 == 0f)
            {
                num3 = 450f;
            }
            else
            {
                num3 = 1f / num2;
                if (__instance.Spawned && !__instance.Map.roofGrid.Roofed(__instance.Position))
                {
                    num3 /= __instance.Map.weatherManager.CurMoveSpeedMultiplier;
                }

                if (diagonal)
                {
                    num3 *= 1.41421f;
                }
            }

            __result = Mathf.Clamp(Mathf.RoundToInt(num3), 1, 450);

            return false;
        }
        public static bool Prefix_PawnGotoAction(IntVec3 clickCell, Pawn pawn, IntVec3 gotoLoc)
        {
            bool flag;
            if (pawn.Position == gotoLoc || (pawn.CurJobDef == JobDefOf.Goto && pawn.CurJob.targetA.Cell == gotoLoc))
            {
                flag = true;
            }
            else
            {
                Job job = JobMaker.MakeJob(JobDefOf.Goto, gotoLoc);
                if (pawn.Map.exitMapGrid.IsExitCell(clickCell))
                {
                    job.exitMapOnArrival = !pawn.IsColonyMech;
                }
                else if (!pawn.Map.IsPlayerHome && !pawn.Map.exitMapGrid.MapUsesExitGrid && CellRect.WholeMap(pawn.Map).IsOnEdge(clickCell, 3) && pawn.Map.Parent.GetComponent<FormCaravanComp>() != null && MessagesRepeatAvoider.MessageShowAllowed("MessagePlayerTriedToLeaveMapViaExitGrid-" + pawn.Map.uniqueID, 60f))
                {
                    if (pawn.Map.Parent.GetComponent<FormCaravanComp>().CanFormOrReformCaravanNow)
                    {
                        Messages.Message("MessagePlayerTriedToLeaveMapViaExitGrid_CanReform".Translate(), pawn.Map.Parent, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        Messages.Message("MessagePlayerTriedToLeaveMapViaExitGrid_CantReform".Translate(), pawn.Map.Parent, MessageTypeDefOf.RejectInput, historical: false);
                    }
                }
                if (pawn.CurJobDef == ReloadJobDefOf.R_Reload)
                {
                    if (pawn.Map.reachability.CanReach(pawn.Position, gotoLoc, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors)))
                    {
                        flag = true;
                        if (KeyBindingDefOf.QueueOrder.IsDownEvent)
                        {
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.QueueOrders, KnowledgeAmount.NoteTaught);
                            if (job.TryMakePreToilReservations(pawn, errorOnFailed: true))
                            {
                                pawn.jobs.jobQueue.EnqueueLast(job, JobTag.Misc);
                            }
                        }
                        else
                        {
                            pawn.jobs.ClearQueuedJobs();
                            pawn.pather.StartPath(gotoLoc, PathEndMode.OnCell);
                        }

                    }
                    else
                        flag = false;
                }
                else
                    flag = pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

            if (flag)
            {
                FleckMaker.Static(gotoLoc, pawn.Map, FleckDefOf.FeedbackGoto);
            }

            return false;
        }
        public static bool Prefix_PatherArrived(Pawn_PathFollower __instance, Pawn ___pawn)
        {
            if (___pawn?.jobs?.curJob?.def != ReloadJobDefOf.R_Reload)
                return true;

            var list = ___pawn.jobs.jobQueue;
            for (int i = 0; i < ___pawn.jobs.jobQueue.Count; i++)
            {
                var job = list[i].job;
                if (job.def != JobDefOf.Goto)
                    continue;
                var dest = job.targetA.Cell;
                ___pawn.jobs.EndCurrentOrQueuedJob(job, JobCondition.Succeeded);
                ___pawn.pather.StartPath(dest, PathEndMode.OnCell);
                return false;
            }

            return true;
        }
        public static bool Prefix_EndCurrentJob(Pawn_JobTracker __instance, JobCondition condition, bool startNewJob, bool canReturnToPool, Pawn ___pawn)
        {
            if (__instance?.curJob?.def != ReloadJobDefOf.R_Reload || condition != JobCondition.Succeeded)
                return true;

            var curJob = __instance.curJob;
            var curDriver = __instance.curDriver;

            JobDef jobDef = ((curJob != null) ? curJob.def : null);
            Job job = curDriver?.GetFinalizerJob(condition);
            if (job != null && curJob != null)
            {
                job.playerForced = curJob.playerForced;
            }

            bool? carryThingAfterJobOverride = null;
            if (startNewJob && job != null)
            {
                carryThingAfterJobOverride = true;
            }

            _cleanupCurrentJob.Invoke(__instance, new object[] { condition, true, true, canReturnToPool, carryThingAfterJobOverride });
            if (!startNewJob)
            {
                return false;
            }

            if (job != null && (bool)_canPawnTakeOpportunisticJob.Invoke(__instance, new object[] { ___pawn }))
            {
                __instance.StartJob(job, JobCondition.None, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, true);
                return false;
            }

            if (condition == JobCondition.Succeeded && jobDef != null && jobDef != JobDefOf.Wait_MaintainPosture && jobDef != JobDefOf.Goto)
            {
                Pawn_PathFollower pather = ___pawn.pather;
                if (pather != null && !pather.Moving)
                {
                    __instance.StartJob(JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture, 1), JobCondition.None, null, resumeCurJobAfterwards: false, cancelBusyStances: false, null, null, fromQueue: false, canReturnCurJobToPool: false, null, continueSleeping: false, addToJobsThisTick: false);
                    return false;
                }
                else if (pather != null && pather.Moving)
                {
                    __instance.StartJob(JobMaker.MakeJob(JobDefOf.Goto, pather.Destination.Cell), JobCondition.None, null, false, false);
                    return false;
                }
            }
            _tryFindAndStartJob.Invoke(__instance, new object[] { });

            return false;
        }
        public static void Postfix_ConfigurableHostilityResponse_TryGiveJob(Pawn pawn, ref Job __result)
        {
            if (__result?.def != JobDefOf.AttackStatic && __result?.def != JobDefOf.AttackMelee)
                return;
            
            var equipments = ReloadUtils.GetReloadableEquipments(pawn);
            foreach (var equipment in equipments)
            {
                CompReload comp = equipment.TryGetComp<CompReload>();
                if (!comp.CanBeUsed && comp.CanLoad())
                {
                    __result = comp.MakeReloadJob();
                    return;
                }
                else if (pawn?.jobs?.curJob?.def == ReloadJobDefOf.R_Reload)
                {
                    __result = null;
                    return;
                }
            }
        }
        public static void Postfix_AIFightEnemy_TryGiveJob(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (__result == null || __result?.def == JobDefOf.AttackMelee || __result?.def == JobDefOf.Goto)
            {
                var equipments = ReloadUtils.GetReloadableEquipments(pawn);

                foreach (var equipment in equipments)
                {
                    var comp = equipment.TryGetComp<CompReload>();
                    if (comp.CanLoad())
                    {
                        __result = comp.MakeReloadJob();
                        return;
                    }
                }
            }
        }
        public static bool Prefix_FirstUnloadableThing(Pawn_InventoryTracker __instance, ref ThingCount __result, ref List<ThingDefCount> ___tmpDrugsToKeep)
        {
            if (!Setting.EnableAmmo)
                return true;
            if (__instance.innerContainer.Count == 0)
                return true;

            Dictionary<ThingDef, int> carryQuantity = new Dictionary<ThingDef, int>(Base.Instance.StockDataStorage.GetStock(__instance.pawn).Items);
            if (carryQuantity.Sum(x => x.Value) == 0)
                return true;

            for (int i = 0; i < __instance.innerContainer.Count; i++)
            {
                var thing = __instance.innerContainer[i];
                if (!(carryQuantity.ContainsKey(thing.def)))
                {
                    if (!thing.def.IsDrug)
                    {
                        __result = new ThingCount(thing, thing.stackCount);
                        return false;
                    }
                    continue;
                }

                if (carryQuantity[thing.def] >= thing.stackCount)
                    carryQuantity[thing.def] -= thing.stackCount;
                else
                {
                    __result = new ThingCount(thing, thing.stackCount - carryQuantity[thing.def]);
                    return false;
                }
            }

            if (__instance.pawn.drugs != null && __instance.pawn.drugs.CurrentPolicy != null)
            {
                DrugPolicy currentPolicy = __instance.pawn.drugs.CurrentPolicy;
                ___tmpDrugsToKeep.Clear();
                for (int i = 0; i < currentPolicy.Count; i++)
                {
                    if (currentPolicy[i].takeToInventory > 0)
                    {
                        ___tmpDrugsToKeep.Add(new ThingDefCount(currentPolicy[i].drug, currentPolicy[i].takeToInventory));
                    }
                }

                for (int j = 0; j < __instance.innerContainer.Count; j++)
                {
                    var thing = __instance.innerContainer[j];
                    if (thing.def is AmmoDef)
                        continue;

                    if (!thing.def.IsDrug)
                    {
                        __result = new ThingCount(thing, thing.stackCount);
                        return false;
                    }

                    int num = -1;
                    for (int k = 0; k < ___tmpDrugsToKeep.Count; k++)
                    {
                        if (thing.def == ___tmpDrugsToKeep[k].ThingDef)
                        {
                            num = k;
                            break;
                        }
                    }

                    if (num < 0)
                    {
                        __result = new ThingCount(thing, thing.stackCount);
                        return false;
                    }

                    if (thing.stackCount > ___tmpDrugsToKeep[num].Count)
                    {
                        __result = new ThingCount(thing, thing.stackCount - ___tmpDrugsToKeep[num].Count);
                        return false;
                    }
                    ___tmpDrugsToKeep[num] = new ThingDefCount(___tmpDrugsToKeep[num].ThingDef, ___tmpDrugsToKeep[num].Count - thing.stackCount);
                }
                __result = default(ThingCount);
                return false;
            }
            __result = default(ThingCount);

            return false;
        }
        public static bool Prefix_WillTrade(TraderKindDef __instance, ThingDef td, ref bool __result)
        {
            if (__instance.defName == "Empire_Caravan_TributeCollector")
                return true;
            if (td is AmmoDef)
            {
                __result = true;
                return false;
            }
            return true;
        }
        public static void Postfix_GenerateGearFor(Pawn pawn)
        {
            List<Thing> weapons = ReloadUtils.GetAllReloadableEquipmentsFromPawn(pawn);

            if (weapons.NullOrEmpty())
                return;
            foreach (var weapon in weapons)
            {
                var comp = weapon.TryGetComp<CompReload>();
                if ((pawn?.RaceProps?.IsMechanoid ?? false))
                {
                    if (weapon is ThingWithComps)
                    {
                        (weapon as ThingWithComps).AllComps.Remove(comp);
                        continue;
                    }
                }

                if (Setting.EnableAmmo)
                {
                    int amount = (int)(AmmoUtils.GetAmmoCapacity(comp));
                    Utils.AddThingsToInventory(pawn, comp.Props.ammoDef, amount);
                }
                comp.remainingAmmo = comp.MagazineCapacity;
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler_Generate(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction foundInstruction = null;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                    foundInstruction = instruction;
                else
                    yield return instruction;
            }
            yield return new CodeInstruction(OpCodes.Ldloc_0, null);
            yield return new CodeInstruction(OpCodes.Ldloc_1, null);
            yield return new CodeInstruction(OpCodes.Ldloc_2, null);
            yield return new CodeInstruction(OpCodes.Ldarg_2, null);
            yield return new CodeInstruction(OpCodes.Call, typeof(HarmonyPatches).GetMethod("GenerateAmmo", BindingFlags.Static | BindingFlags.NonPublic));
            yield return foundInstruction;
        }
        static void GenerateAmmo(TraderKindDef traderKindDef, Faction makingFaction, int forTile, List<Thing> outThings)
        {
            if (!Setting.EnableAmmo)
                return;
            List<AmmoDef> ammoDefs = Setting.AmmoDefs;
            if (ammoDefs.NullOrEmpty())
                return;
            TechLevel factionTech = makingFaction?.def?.techLevel ?? TechLevel.Undefined;
            float baseValue = 50f;
            FloatRange quantityRange_traderType = new FloatRange(0.8f, 1.2f);

            string traderDefName = traderKindDef.defName.ToLower();
            if (traderDefName.Contains("visitor"))
            {
                quantityRange_traderType.min = 0f;
                quantityRange_traderType.max = 0.5f;
            }
            else if (traderDefName.Contains("caravan"))
            {
                quantityRange_traderType.min = 2f;
                quantityRange_traderType.max = 5f;
            }
            else if (traderDefName.Contains("base"))
            {
                quantityRange_traderType.min = 5f;
                quantityRange_traderType.max = 10f;
            }
            else if (traderDefName.Contains("orbital"))
            {
                quantityRange_traderType.min = 10f;
                quantityRange_traderType.max = 20f;
            }
            ammoDefs.SortBy(x => (int)x.techLevel * 1000 + x.BaseMarketValue);
            foreach (var def in ammoDefs)
            {
                var ammoTech = def.techLevel;
                if (factionTech != TechLevel.Undefined && factionTech < ammoTech)
                    continue;
                float factorFromAmmoType = def.ammoType == AmmoType.Normal ? 1 : def.ammoType == AmmoType.Explosive ? 0.75f : 0.5f;
                float factorFromTraderType = quantityRange_traderType.RandomInRange;
                float factorFromTechLevel = 0.5f;
                if(factionTech == TechLevel.Undefined)
                    factorFromTechLevel = 1f;
                else if (factionTech < TechLevel.Industrial && ammoTech < TechLevel.Industrial
                    || factionTech == TechLevel.Industrial && ammoTech == TechLevel.Industrial
                    || factionTech >= TechLevel.Spacer && ammoTech >= TechLevel.Spacer)
                    factorFromTechLevel = 1.5f;
                int quantity = (int)((baseValue * factorFromAmmoType * factorFromTechLevel * factorFromTraderType) / Math.Max(0.01f, def.BaseMarketValue));
                var generatedThings = Utils.GenerateThings(def, quantity);
                if (generatedThings.NullOrEmpty())
                    continue;
                foreach (var thing in generatedThings)
                {
                    thing.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
                    outThings.Add(thing);
                }
            }
        }
        public static void Postfix_AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            var pos = IntVec3.FromVector3(clickPos);
            using (List<Thing>.Enumerator enumerator = GridsUtility.GetThingList(pos, pawn.Map).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Thing thing = enumerator.Current;
                    if (!pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly))
                        continue;

                    if (thing.def is AmmoDef)
                    {
                        if (!pawn.IsColonist)
                            continue;

                        opts.Add(new FloatMenuOption($"{TranslatorFormattedStringExtensions.Translate("PickUpSome", thing.Label, thing)}", delegate ()
                        {
                            Action<int> pickUpAction = delegate (int quantity)
                            {
                                Utils.FetchThings(pawn, new List<Thing> { thing }, quantity);
                            };
                            Dialog_Slider slider = new Dialog_Slider(TranslatorFormattedStringExtensions.Translate("PickUpCount", thing.LabelShort, thing), 1, thing.stackCount, pickUpAction);
                            Find.WindowStack.Add(slider);
                        }));

                        if (Setting.EnableAmmo)
                        {
                            foreach (var equipment in ReloadUtils.GetAllReloadableEquipmentsFromPawn(pawn))
                            {
                                var comp = equipment.TryGetComp<CompReload>();

                                if (comp.NeedsReload())
                                {
                                    opts.Add(new FloatMenuOption($"{"ReloadCap".Translate()} {comp.parent.Label} {comp.LabelRemaining}", delegate ()
                                    {
                                        Utils.FetchThings(pawn, new List<Thing>() { thing }, Math.Min(thing.stackCount, comp.MagazineCapacity - comp.remainingAmmo));
                                        Job job = comp.MakeReloadJob();
                                        pawn.jobs.jobQueue.EnqueueLast(job);
                                    }));
                                }
                            }
                        }
                    }
                }
            }
        }
        public static IEnumerable<Gizmo> Postfix_GetGizmos(IEnumerable<Gizmo> __result, Pawn_EquipmentTracker __instance)
        {
            var equipments = ReloadUtils.GetReloadableEquipments(__instance.pawn);

            foreach (var equipment in equipments)
            {
                CompReload comp = equipment.TryGetComp<CompReload>();
                if (comp == null)
                    continue;
                if (__instance.pawn.IsColonistPlayerControlled)
                    yield return new Command_Reload(comp);

                if (DebugSettings.godMode)
                {
                    Command_Action setAmmoToMax = new Command_Action
                    {
                        groupable = false,
                        action = () => { comp.remainingAmmo = comp.MagazineCapacity; },
                        defaultLabel = "DEV: Set ammo to max",
                    };
                    yield return setAmmoToMax;
                    Command_Action setAmmoToZero = new Command_Action
                    {

                        groupable = false,
                        action = () => { comp.remainingAmmo = 0; },
                        defaultLabel = "DEV: Set ammo to 0",
                    };
                    yield return setAmmoToZero;
                }
            }
            foreach (var gizmo in __result)
                yield return gizmo;
        }
        public static bool Prefix_SetFromPreset(ThingFilter __instance, StorageSettingsPreset preset)
        {
            if (preset == StorageSettingsPreset.DefaultStockpile)
                __instance.SetAllow(ThingCategoryDef.Named("R_Category_Ammo"), allow: true);
            return true;
        }
        public static bool Prefix_PlayerAcquirable(ThingDef __instance, ref bool __result)
        {
            if (Setting.EnableAmmo)
                return true;

            if (__instance.defName.ToLower().Contains("r_") && __instance.defName.ToLower().Contains("ammo"))
            {
                __result = false;
                return false;
            }
            return true;
        }
        public static bool Prefix_AvailableNow(RecipeDef __instance, bool __result)
        {
            if (__instance.defName.Contains("R_") && __instance.defName.Contains("Ammo") && !Setting.EnableAmmo)
            {
                __result = false;
                return false;
            }
            return true;
        }
        public static bool Prefix_OrderTo(object __instance, Vector3 pos)
        {
            Colonist colonist = __instance as Colonist;
            if (colonist.pawn.CurJobDef != ReloadJobDefOf.R_Reload)
                return true;

            IntVec3 intVec = colonist.UpdateOrderPos(pos);
            if ((intVec.IsValid && !colonist.lastOrder.IsValid) || colonist.lastOrder != intVec)
            {
                colonist.lastOrder = intVec;
                OrderToSynced(colonist.pawn, intVec.x, intVec.z);
            }
            return false;
        }
        static void OrderToSynced(Pawn pawn, int x, int z)
        {
            IntVec3 intVec = new IntVec3(x, 0, z);
            Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
            job.playerForced = true;
            job.collideWithPawns = false;
            if (pawn.Map.exitMapGrid.IsExitCell(intVec))
            {
                job.exitMapOnArrival = true;
            }
            Pawn_JobTracker jobs = pawn.jobs;
            if (jobs != null && jobs.IsCurrentJobPlayerInterruptible())
            {
                if (KeyBindingDefOf.QueueOrder.IsDownEvent && pawn.pather.Moving)
                {
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.QueueOrders, KnowledgeAmount.NoteTaught);
                    if (job.TryMakePreToilReservations(pawn, errorOnFailed: true))
                    {
                        pawn.jobs.jobQueue.EnqueueLast(job, JobTag.Misc);
                    }
                }
                else
                {
                    pawn.jobs.ClearQueuedJobs();
                    pawn.pather.StartPath(intVec, PathEndMode.OnCell);
                }
            }
        }
    }
}