
using System.Collections.Generic;
using RimWorld;

using HarmonyLib;
using Verse;
using UnityEngine;
using System.Linq;
using Verse.AI;
using System;
using System.Reflection;
using RimWorld.Planet;
using Verse.AI.Group;


namespace forcedOrder
{
    public class harmonyPatch : Mod
    {
        public harmonyPatch(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("forcedOrder");
            harmony.PatchAll();
        }
    }




    /*

    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    internal class patch_FloatMenuMakerMap_ChoicesAtFor
    {
        private static MethodInfo m_CanTakeOrder = AccessTools.Method(typeof(FloatMenuMakerMap), "CanTakeOrder", new Type[] { typeof(Pawn) });
        
        [HarmonyPostfix]
        static bool Prefix(List<FloatMenuOption> __result, Vector3 clickPos, Pawn pawn)
        {

            IntVec3 intVec3 = IntVec3.FromVector3(clickPos);
            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            if (!intVec3.InBounds(pawn.Map) || !(bool)m_CanTakeOrder.Invoke(null, new object[] { pawn }) || pawn.Map != Find.CurrentMap)
            {
                __result = opts;
                return false;
            }
                
            FloatMenuMakerMap.makingFor = pawn;
            try
            {
                if (intVec3.Fogged(pawn.Map))
                {
                    if (pawn.Drafted)
                    {
                        FloatMenuOption floatMenuOption = FloatMenuMakerMap.GotoLocationOption(intVec3, pawn);
                        if (floatMenuOption != null)
                        {
                            if (!floatMenuOption.Disabled)
                                opts.Add(floatMenuOption);
                        }
                    }
                }
                else
                {
                    if (pawn.Drafted)
                        FloatMenuMakerMap.AddDraftedOrders(clickPos, pawn, opts);
                    if (pawn.RaceProps.Humanlike)
                        FloatMenuMakerMap.AddHumanlikeOrders(clickPos, pawn, opts);
                    if (!pawn.Drafted)
                        FloatMenuMakerMap.AddUndraftedOrders(clickPos, pawn, opts);
                    foreach (FloatMenuOption floatMenuOption in pawn.GetExtraFloatMenuOptionsFor(intVec3))
                        opts.Add(floatMenuOption);
                }
            }
            finally
            {
                FloatMenuMakerMap.makingFor = (Pawn)null;
            }
            __result = opts;
            return false;
        }
    }
    */




    /*
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    internal class patch_FloatMenuMakerMap_AddHumanlikeOrders
    {
        private static FieldInfo f_equivalenceGroupTempStorage = AccessTools.Field(typeof(FloatMenuMakerMap), "equivalenceGroupTempStorage");
        private static AccessTools.FieldRef<FloatMenuOption[]> s_equivalenceGroupTempStorage = AccessTools.StaticFieldRefAccess<FloatMenuOption[]>(f_equivalenceGroupTempStorage);

        [HarmonyPostfix]
        static bool Prefix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            foreach (Thing thing in c.GetThingList(pawn.Map))
            {
                if (thing is Pawn pawn1)
                {
                    Lord lord = pawn1.GetLord();
                    if (lord != null && lord.CurLordToil != null)
                    {
                        IEnumerable<FloatMenuOption> floatMenuOptions = lord.CurLordToil.ExtraFloatMenuOptions(pawn1, pawn);
                        if (floatMenuOptions != null)
                        {
                            foreach (FloatMenuOption floatMenuOption in floatMenuOptions)
                                opts.Add(floatMenuOption);
                        }
                    }
                }
            }
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (LocalTargetInfo dest in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForArrest(pawn), true))
                {
                    bool flag = dest.HasThing && dest.Thing is Pawn && ((Pawn)dest.Thing).IsWildMan();
                    if (pawn.Drafted || flag)
                    {
                        if (dest.Thing is Pawn && (pawn.InSameExtraFaction((Pawn)dest.Thing, ExtraFactionType.HomeFaction) || pawn.InSameExtraFaction((Pawn)dest.Thing, ExtraFactionType.MiniFaction)))
                            opts.Add(new FloatMenuOption((string)("CannotArrest".Translate() + ": " + "SameFaction".Translate((NamedArgument)dest.Thing)), (Action)null));
                        else if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly))
                        {
                            opts.Add(new FloatMenuOption((string)("CannotArrest".Translate() + ": " + "NoPath".Translate().CapitalizeFirst()), (Action)null));
                        }
                        else
                        {
                            Pawn pTarg = (Pawn)dest.Thing;
                            Action action = (Action)(() =>
                            {
                                Building_Bed buildingBed = RestUtility.FindBedFor(pTarg, pawn, true, false) ?? RestUtility.FindBedFor(pTarg, pawn, true, false, true);
                                if (buildingBed == null)
                                {
                                    Messages.Message((string)("CannotArrest".Translate() + ": " + "NoPrisonerBed".Translate()), (LookTargets)(Thing)pTarg, MessageTypeDefOf.RejectInput, false);
                                }
                                else
                                {
                                    Job job = JobMaker.MakeJob(JobDefOf.Arrest, (LocalTargetInfo)(Thing)pTarg, (LocalTargetInfo)(Thing)buildingBed);
                                    job.count = 1;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                    if (pTarg.Faction == null || (pTarg.Faction == Faction.OfPlayer || pTarg.Faction.Hidden) && !pTarg.IsQuestLodger())
                                        return;
                                    TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.ArrestingCreatesEnemies, pTarg.GetAcceptArrestChance(pawn).ToStringPercent());
                                }
                            });
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"TryToArrest".Translate((NamedArgument)dest.Thing.LabelCap, (NamedArgument)dest.Thing, (NamedArgument)pTarg.GetAcceptArrestChance(pawn).ToStringPercent()), action, MenuOptionPriority.High, revalidateClickTarget: dest.Thing), pawn, (LocalTargetInfo)(Thing)pTarg));
                        }
                    }
                }
            }
            foreach (Thing thing in c.GetThingList(pawn.Map))
            {
                Thing t = thing;
                if (t.def.ingestible != null && pawn.RaceProps.CanEverEat(t) && t.IngestibleNow)
                {
                    string label = !t.def.ingestible.ingestCommandString.NullOrEmpty() ? string.Format(t.def.ingestible.ingestCommandString, (object)t.LabelShort) : (string)"ConsumeThing".Translate((NamedArgument)t.LabelShort, (NamedArgument)t);
                    if (!t.IsSociallyProper(pawn))
                        label = (string)(label + ": " + "ReservedForPrisoners".Translate().CapitalizeFirst());
                    FloatMenuOption floatMenuOption;
                    if (t.def.IsNonMedicalDrug && pawn.IsTeetotaler())
                        floatMenuOption = new FloatMenuOption(label + ": " + TraitDefOf.DrugDesire.DataAtDegree(-1).GetLabelCapFor(pawn), (Action)null);
                    else if (FoodUtility.InappropriateForTitle(t.def, pawn, true))
                        floatMenuOption = new FloatMenuOption((string)(label + ": " + "FoodBelowTitleRequirements".Translate((NamedArgument)pawn.royalty.MostSeniorTitle.def.GetLabelFor(pawn))), (Action)null);
                    else if (!pawn.CanReach((LocalTargetInfo)t, PathEndMode.OnCell, Danger.Deadly))
                    {
                        floatMenuOption = new FloatMenuOption((string)(label + ": " + "NoPath".Translate().CapitalizeFirst()), (Action)null);
                    }
                    else
                    {
                        MenuOptionPriority priority = t is Corpse ? MenuOptionPriority.Low : MenuOptionPriority.Default;
                        int maxAmountToPickup1 = FoodUtility.GetMaxAmountToPickup(t, pawn, FoodUtility.WillIngestStackCountOf(pawn, t.def, t.GetStatValue(StatDefOf.Nutrition)));
                        floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, (Action)(() =>
                        {
                            int maxAmountToPickup = FoodUtility.GetMaxAmountToPickup(t, pawn, FoodUtility.WillIngestStackCountOf(pawn, t.def, t.GetStatValue(StatDefOf.Nutrition)));
                            if (maxAmountToPickup == 0)
                                return;
                            t.SetForbidden(false);
                            Job job = JobMaker.MakeJob(JobDefOf.Ingest, (LocalTargetInfo)t);
                            job.count = maxAmountToPickup;
                            pawn.jobs.TryTakeOrderedJob(job);
                        }), priority), pawn, (LocalTargetInfo)t);
                        if (maxAmountToPickup1 == 0)
                            floatMenuOption.action = (Action)null;
                    }
                    opts.Add(floatMenuOption);
                }
            }
            foreach (LocalTargetInfo dest in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForQuestPawnsWhoWillJoinColony(pawn), true))
            {
                Pawn toHelpPawn = (Pawn)dest.Thing;
                FloatMenuOption floatMenuOption = pawn.CanReach(dest, PathEndMode.Touch, Danger.Deadly) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)(toHelpPawn.IsPrisoner ? "FreePrisoner".Translate() : "OfferHelp".Translate()), (Action)(() => pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.OfferHelp, (LocalTargetInfo)(Thing)toHelpPawn))), MenuOptionPriority.RescueOrCapture, revalidateClickTarget: ((Thing)toHelpPawn)), pawn, (LocalTargetInfo)(Thing)toHelpPawn) : new FloatMenuOption((string)"CannotGoNoPath".Translate(), (Action)null);
                opts.Add(floatMenuOption);
            }
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (Thing thing in c.GetThingList(pawn.Map))
                {
                    Corpse corpse = thing as Corpse;
                    if (corpse != null && corpse.IsInValidStorage())
                    {
                        StoragePriority priority = StoreUtility.CurrentHaulDestinationOf((Thing)corpse).GetStoreSettings().Priority;
                        IHaulDestination haulDestination;
                        if (StoreUtility.TryFindBestBetterNonSlotGroupStorageFor((Thing)corpse, pawn, pawn.Map, priority, Faction.OfPlayer, out haulDestination, true) && haulDestination.GetStoreSettings().Priority == priority && haulDestination is Building_Grave)
                        {
                            Building_Grave grave = haulDestination as Building_Grave;
                            string label = (string)"PrioritizeGeneric".Translate((NamedArgument)"Burying".Translate(), (NamedArgument)corpse.Label);
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, (Action)(() => pawn.jobs.TryTakeOrderedJob(HaulAIUtility.HaulToContainerJob(pawn, (Thing)corpse, (Thing)grave)))), pawn, new LocalTargetInfo((Thing)corpse)));
                        }
                    }
                }
                foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    Pawn victim = (Pawn)localTargetInfo.Thing;
                    if (!victim.InBed() && pawn.CanReserveAndReach((LocalTargetInfo)(Thing)victim, PathEndMode.OnCell, Danger.Deadly, ignoreOtherReservations: true) && !victim.mindState.WillJoinColonyIfRescued)
                    {
                        if (!victim.IsPrisonerOfColony && (!victim.InMentalState || victim.health.hediffSet.HasHediff(HediffDefOf.Scaria)) && (victim.Faction == Faction.OfPlayer || victim.Faction == null || !victim.Faction.HostileTo(Faction.OfPlayer)))
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"Rescue".Translate((NamedArgument)victim.LabelCap, (NamedArgument)(Thing)victim), (Action)(() =>
                            {
                                Building_Bed buildingBed = RestUtility.FindBedFor(victim, pawn, false, false) ?? RestUtility.FindBedFor(victim, pawn, false, false, true);
                                if (buildingBed == null)
                                {
                                    string str = !victim.RaceProps.Animal ? (string)"NoNonPrisonerBed".Translate() : (string)"NoAnimalBed".Translate();
                                    Messages.Message((string)("CannotRescue".Translate() + ": " + str), (LookTargets)(Thing)victim, MessageTypeDefOf.RejectInput, false);
                                }
                                else
                                {
                                    Job job = JobMaker.MakeJob(JobDefOf.Rescue, (LocalTargetInfo)(Thing)victim, (LocalTargetInfo)(Thing)buildingBed);
                                    job.count = 1;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
                                }
                            }), MenuOptionPriority.RescueOrCapture, revalidateClickTarget: ((Thing)victim)), pawn, (LocalTargetInfo)(Thing)victim));
                        if (victim.RaceProps.Humanlike && (victim.InMentalState || victim.Faction != Faction.OfPlayer || victim.Downed && (victim.guilt.IsGuilty || victim.IsPrisonerOfColony)))
                        {
                            TaggedString taggedString = "Capture".Translate((NamedArgument)victim.LabelCap, (NamedArgument)(Thing)victim);
                            if (victim.Faction != null && victim.Faction != Faction.OfPlayer && (!victim.Faction.Hidden && !victim.Faction.HostileTo(Faction.OfPlayer)) && !victim.IsPrisonerOfColony)
                                taggedString += ": " + "AngersFaction".Translate().CapitalizeFirst();
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)taggedString, (Action)(() =>
                            {
                                Building_Bed buildingBed = RestUtility.FindBedFor(victim, pawn, true, false) ?? RestUtility.FindBedFor(victim, pawn, true, false, true);
                                if (buildingBed == null)
                                {
                                    Messages.Message((string)("CannotCapture".Translate() + ": " + "NoPrisonerBed".Translate()), (LookTargets)(Thing)victim, MessageTypeDefOf.RejectInput, false);
                                }
                                else
                                {
                                    Job job = JobMaker.MakeJob(JobDefOf.Capture, (LocalTargetInfo)(Thing)victim, (LocalTargetInfo)(Thing)buildingBed);
                                    job.count = 1;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Capturing, KnowledgeAmount.Total);
                                    if (victim.Faction == null || victim.Faction == Faction.OfPlayer || (victim.Faction.Hidden || victim.Faction.HostileTo(Faction.OfPlayer)) || victim.IsPrisonerOfColony)
                                        return;
                                    Messages.Message((string)"MessageCapturingWillAngerFaction".Translate(victim.Named("PAWN")).AdjustedFor(victim), (LookTargets)(Thing)victim, MessageTypeDefOf.CautionInput, false);
                                }
                            }), MenuOptionPriority.RescueOrCapture, revalidateClickTarget: ((Thing)victim)), pawn, (LocalTargetInfo)(Thing)victim));
                        }
                    }
                }
                foreach (LocalTargetInfo localTargetInfo1 in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    LocalTargetInfo localTargetInfo2 = localTargetInfo1;
                    Pawn victim = (Pawn)localTargetInfo2.Thing;
                    if (victim.Downed && pawn.CanReserveAndReach((LocalTargetInfo)(Thing)victim, PathEndMode.OnCell, Danger.Deadly, ignoreOtherReservations: true) && Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn, true) != null)
                    {
                        string label1 = (string)"CarryToCryptosleepCasket".Translate((NamedArgument)localTargetInfo2.Thing.LabelCap, (NamedArgument)localTargetInfo2.Thing);
                        JobDef jDef = JobDefOf.CarryToCryptosleepCasket;
                        Action action = (Action)(() =>
                        {
                            Building_CryptosleepCasket cryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn) ?? Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn, true);
                            if (cryptosleepCasket == null)
                            {
                                Messages.Message((string)("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate()), (LookTargets)(Thing)victim, MessageTypeDefOf.RejectInput, false);
                            }
                            else
                            {
                                Job job = JobMaker.MakeJob(jDef, (LocalTargetInfo)(Thing)victim, (LocalTargetInfo)(Thing)cryptosleepCasket);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }
                        });
                        if (victim.IsQuestLodger())
                        {
                            string label2 = (string)(label1 + (" (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")"));
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label2, (Action)null, revalidateClickTarget: ((Thing)victim)), pawn, (LocalTargetInfo)(Thing)victim));
                        }
                        else if (victim.GetExtraHostFaction() != null)
                        {
                            string label2 = (string)(label1 + (" (" + "CryptosleepCasketGuestPrisonersNotAllowed".Translate() + ")"));
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label2, (Action)null, revalidateClickTarget: ((Thing)victim)), pawn, (LocalTargetInfo)(Thing)victim));
                        }
                        else
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label1, action, revalidateClickTarget: ((Thing)victim)), pawn, (LocalTargetInfo)(Thing)victim));
                    }
                }
                if (ModsConfig.RoyaltyActive)
                {
                    foreach (LocalTargetInfo localTargetInfo1 in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForShuttle(pawn), true))
                    {
                        LocalTargetInfo localTargetInfo2 = localTargetInfo1;
                        Pawn victim = (Pawn)localTargetInfo2.Thing;
                        Predicate<Thing> validator = (Predicate<Thing>)(thing =>
                        {
                            CompShuttle comp = thing.TryGetComp<CompShuttle>();
                            return comp != null && comp.IsAllowedNow((Thing)victim);
                        });
                        Thing shuttleThing = GenClosest.ClosestThingReachable(victim.Position, victim.Map, ThingRequest.ForDef(ThingDefOf.Shuttle), PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: validator);
                        if (shuttleThing != null && pawn.CanReserveAndReach((LocalTargetInfo)(Thing)victim, PathEndMode.OnCell, Danger.Deadly, ignoreOtherReservations: true) && !pawn.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
                        {
                            string label = (string)"CarryToShuttle".Translate((NamedArgument)localTargetInfo2.Thing);
                            Action action = (Action)(() =>
                            {
                                CompShuttle comp = shuttleThing.TryGetComp<CompShuttle>();
                                if (!comp.LoadingInProgressOrReadyToLaunch)
                                    TransporterUtility.InitiateLoading(Gen.YieldSingle<CompTransporter>(comp.Transporter));
                                Job job = JobMaker.MakeJob(JobDefOf.HaulToTransporter, (LocalTargetInfo)(Thing)victim, (LocalTargetInfo)shuttleThing);
                                job.ignoreForbidden = true;
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                            });
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), pawn, (LocalTargetInfo)(Thing)victim));
                        }
                    }
                }
            }
            foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForStrip(pawn), true))
            {
                LocalTargetInfo stripTarg = localTargetInfo;
                FloatMenuOption floatMenuOption = pawn.CanReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly) ? (stripTarg.Pawn == null || !stripTarg.Pawn.HasExtraHomeFaction() ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"Strip".Translate((NamedArgument)stripTarg.Thing.LabelCap, (NamedArgument)stripTarg.Thing), (Action)(() =>
                {
                    stripTarg.Thing.SetForbidden(false, false);
                    pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Strip, stripTarg));
                    StrippableUtility.CheckSendStrippingImpactsGoodwillMessage(stripTarg.Thing);
                })), pawn, stripTarg) : new FloatMenuOption((string)("CannotStrip".Translate((NamedArgument)stripTarg.Thing.LabelCap, (NamedArgument)stripTarg.Thing) + ": " + "QuestRelated".Translate().CapitalizeFirst()), (Action)null)) : new FloatMenuOption((string)("CannotStrip".Translate((NamedArgument)stripTarg.Thing.LabelCap, (NamedArgument)stripTarg.Thing) + ": " + "NoPath".Translate().CapitalizeFirst()), (Action)null);
                opts.Add(floatMenuOption);
            }
            if (pawn.equipment != null)
            {
                ThingWithComps equipment = (ThingWithComps)null;
                List<Thing> thingList = c.GetThingList(pawn.Map);
                for (int index = 0; index < thingList.Count; ++index)
                {
                    if (thingList[index].TryGetComp<CompEquippable>() != null)
                    {
                        equipment = (ThingWithComps)thingList[index];
                        break;
                    }
                }
                if (equipment != null)
                {
                    string labelShort = equipment.LabelShort;
                    FloatMenuOption floatMenuOption;
                    if (equipment.def.IsWeapon && pawn.WorkTagIsDisabled(WorkTags.Violent))
                        floatMenuOption = new FloatMenuOption((string)("CannotEquip".Translate((NamedArgument)labelShort) + ": " + "IsIncapableOfViolenceLower".Translate((NamedArgument)pawn.LabelShort, (NamedArgument)(Thing)pawn)), (Action)null);
                    else if (!pawn.CanReach((LocalTargetInfo)(Thing)equipment, PathEndMode.ClosestTouch, Danger.Deadly))
                        floatMenuOption = new FloatMenuOption((string)("CannotEquip".Translate((NamedArgument)labelShort) + ": " + "NoPath".Translate().CapitalizeFirst()), (Action)null);
                    else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                        floatMenuOption = new FloatMenuOption((string)("CannotEquip".Translate((NamedArgument)labelShort) + ": " + "Incapable".Translate()), (Action)null);
                    else if (equipment.IsBurning())
                        floatMenuOption = new FloatMenuOption((string)("CannotEquip".Translate((NamedArgument)labelShort) + ": " + "BurningLower".Translate()), (Action)null);
                    else if (pawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip((Thing)equipment, pawn))
                    {
                        floatMenuOption = new FloatMenuOption((string)("CannotEquip".Translate((NamedArgument)labelShort) + ": " + "QuestRelated".Translate().CapitalizeFirst()), (Action)null);
                    }
                    else
                    {
                        string cantReason;
                        if (!EquipmentUtility.CanEquip_NewTmp((Thing)equipment, pawn, out cantReason, false))
                        {
                            floatMenuOption = new FloatMenuOption((string)("CannotEquip".Translate((NamedArgument)labelShort) + ": " + cantReason.CapitalizeFirst()), (Action)null);
                        }
                        else
                        {
                            string label1 = (string)"Equip".Translate((NamedArgument)labelShort);
                            if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                                label1 = (string)(label1 + (" " + "EquipWarningBrawler".Translate()));
                            if (EquipmentUtility.AlreadyBondedToWeapon((Thing)equipment, pawn))
                            {
                                string label2 = (string)(label1 + (" " + "BladelinkAlreadyBonded".Translate()));
                                TaggedString dialogText = "BladelinkAlreadyBondedDialog".Translate(pawn.Named("PAWN"), equipment.Named("WEAPON"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
                                floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label2, (Action)(() => Find.WindowStack.Add((Window)new Dialog_MessageBox(dialogText))), MenuOptionPriority.High), pawn, (LocalTargetInfo)(Thing)equipment);
                            }
                            else
                                floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label1, (Action)(() =>
                                {
                                    string confirmationText = EquipmentUtility.GetPersonaWeaponConfirmationText((Thing)equipment, pawn);
                                    if (!confirmationText.NullOrEmpty())
                                        Find.WindowStack.Add((Window)new Dialog_MessageBox((TaggedString)confirmationText, (string)"Yes".Translate(), (Action)(() => Equip()), (string)"No".Translate()));
                                    else
                                        Equip();
                                }), MenuOptionPriority.High), pawn, (LocalTargetInfo)(Thing)equipment);
                        }
                    }
                    opts.Add(floatMenuOption);
                }

                void Equip()
                {
                    equipment.SetForbidden(false);
                    pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, (LocalTargetInfo)(Thing)equipment));
                    MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                }
            }
            foreach (Pair<CompReloadable, Thing> pair in ReloadableUtility.FindPotentiallyReloadableGear(pawn, c.GetThingList(pawn.Map)))
            {
                CompReloadable comp = pair.First;
                Thing second = pair.Second;
                string label = (string)("Reload".Translate(comp.parent.Named("GEAR"), NamedArgumentUtility.Named(comp.AmmoDef, "AMMO")) + " (" + comp.LabelRemaining + ")");
                if (!pawn.CanReach((LocalTargetInfo)second, PathEndMode.ClosestTouch, Danger.Deadly))
                    opts.Add(new FloatMenuOption((string)(label + ": " + "NoPath".Translate().CapitalizeFirst()), (Action)null));
                else if (!comp.NeedsReload(true))
                {
                    opts.Add(new FloatMenuOption((string)(label + ": " + "ReloadFull".Translate()), (Action)null));
                }
                else
                {
                    List<Thing> chosenAmmo;
                    if ((chosenAmmo = ReloadableUtility.FindEnoughAmmo(pawn, second.Position, comp, true)) == null)
                    {
                        opts.Add(new FloatMenuOption((string)(label + ": " + "ReloadNotEnough".Translate()), (Action)null));
                    }
                    else
                    {
                        Action action = (Action)(() => pawn.jobs.TryTakeOrderedJob(JobGiver_Reload.MakeReloadJob(comp, chosenAmmo)));
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), pawn, (LocalTargetInfo)second));
                    }
                }
            }
            TaggedString taggedString1;
            if (pawn.apparel != null)
            {
                Apparel apparel = pawn.Map.thingGrid.ThingAt<Apparel>(c);
                if (apparel != null)
                {
                    string key1 = "CannotWear";
                    string key2 = "ForceWear";
                    if (apparel.def.apparel.LastLayer.IsUtilityLayer)
                    {
                        key1 = "CannotEquipApparel";
                        key2 = "ForceEquipApparel";
                    }
                    FloatMenuOption floatMenuOption;
                    if (!pawn.CanReach((LocalTargetInfo)(Thing)apparel, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        TaggedString taggedString2 = key1.Translate((NamedArgument)apparel.Label, (NamedArgument)(Thing)apparel) + ": ";
                        taggedString1 = "NoPath".Translate();
                        TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                        floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                    }
                    else if (apparel.IsBurning())
                        floatMenuOption = new FloatMenuOption((string)(key1.Translate((NamedArgument)apparel.Label, (NamedArgument)(Thing)apparel) + ": " + "Burning".Translate()), (Action)null);
                    else if (pawn.apparel.WouldReplaceLockedApparel(apparel))
                    {
                        TaggedString taggedString2 = key1.Translate((NamedArgument)apparel.Label, (NamedArgument)(Thing)apparel) + ": ";
                        taggedString1 = "WouldReplaceLockedApparel".Translate();
                        TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                        floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                    }
                    else
                    {
                        string cantReason;
                        floatMenuOption = ApparelUtility.HasPartsToWear(pawn, apparel.def) ? (EquipmentUtility.CanEquip_NewTmp((Thing)apparel, pawn, out cantReason) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)key2.Translate((NamedArgument)apparel.LabelShort, (NamedArgument)(Thing)apparel), (Action)(() =>
                        {
                            apparel.SetForbidden(false);
                            pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Wear, (LocalTargetInfo)(Thing)apparel));
                        }), MenuOptionPriority.High), pawn, (LocalTargetInfo)(Thing)apparel) : new FloatMenuOption((string)(key1.Translate((NamedArgument)apparel.Label, (NamedArgument)(Thing)apparel) + ": " + cantReason), (Action)null)) : new FloatMenuOption((string)(key1.Translate((NamedArgument)apparel.Label, (NamedArgument)(Thing)apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate()), (Action)null);
                    }
                    opts.Add(floatMenuOption);
                }
            }
            if (pawn.IsFormingCaravan())
            {
                Thing item = c.GetFirstItem(pawn.Map);
                if (item != null && item.def.EverHaulable && item.def.canLoadIntoCaravan)
                {
                    Pawn packTarget = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(pawn) ?? pawn;
                    JobDef jobDef = packTarget == pawn ? JobDefOf.TakeInventory : JobDefOf.GiveToPackAnimal;
                    if (!pawn.CanReach((LocalTargetInfo)item, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        List<FloatMenuOption> floatMenuOptionList = opts;
                        TaggedString taggedString2 = "CannotLoadIntoCaravan".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": ";
                        taggedString1 = "NoPath".Translate();
                        TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                        FloatMenuOption floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                        floatMenuOptionList.Add(floatMenuOption);
                    }
                    else if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, item, 1))
                    {
                        opts.Add(new FloatMenuOption((string)("CannotLoadIntoCaravan".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": " + "TooHeavy".Translate()), (Action)null));
                    }
                    else
                    {
                        float capacityLeft = CaravanFormingUtility.CapacityLeft((LordJob_FormAndSendCaravan)pawn.GetLord().LordJob);
                        if (item.stackCount == 1)
                        {
                            float capacityLeft4 = capacityLeft - item.GetStatValue(StatDefOf.Mass);
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CaravanFormingUtility.AppendOverweightInfo((string)"LoadIntoCaravan".Translate((NamedArgument)item.Label, (NamedArgument)item), capacityLeft4), (Action)(() =>
                            {
                                item.SetForbidden(false, false);
                                Job job = JobMaker.MakeJob(jobDef, (LocalTargetInfo)item);
                                job.count = 1;
                                job.checkEncumbrance = packTarget == pawn;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                        }
                        else
                        {
                            if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, item, item.stackCount))
                            {
                                opts.Add(new FloatMenuOption((string)("CannotLoadIntoCaravanAll".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": " + "TooHeavy".Translate()), (Action)null));
                            }
                            else
                            {
                                float capacityLeft2 = capacityLeft - (float)item.stackCount * item.GetStatValue(StatDefOf.Mass);
                                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CaravanFormingUtility.AppendOverweightInfo((string)"LoadIntoCaravanAll".Translate((NamedArgument)item.Label, (NamedArgument)item), capacityLeft2), (Action)(() =>
                                {
                                    item.SetForbidden(false, false);
                                    Job job = JobMaker.MakeJob(jobDef, (LocalTargetInfo)item);
                                    job.count = item.stackCount;
                                    job.checkEncumbrance = packTarget == pawn;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                            }
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"LoadIntoCaravanSome".Translate((NamedArgument)item.LabelNoCount, (NamedArgument)item), (Action)(() => Find.WindowStack.Add((Window)new Dialog_Slider((Func<int, string>)(val =>
                            {
                                float capacityLeft3 = capacityLeft - (float)val * item.GetStatValue(StatDefOf.Mass);
                                return CaravanFormingUtility.AppendOverweightInfo(string.Format((string)"LoadIntoCaravanCount".Translate((NamedArgument)item.LabelNoCount, (NamedArgument)item), (object)val), capacityLeft3);
                            }), 1, Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(packTarget, item), item.stackCount), closure_1 ?? (closure_1 = (Action<int>)(count =>
                            {
                                item.SetForbidden(false, false);
                                Job job = JobMaker.MakeJob(jobDef, (LocalTargetInfo)item);
                                job.count = count;
                                job.checkEncumbrance = packTarget == pawn;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }))))), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                        }
                    }
                }
            }
            if (!pawn.Map.IsPlayerHome && !pawn.IsFormingCaravan())
            {
                Thing item = c.GetFirstItem(pawn.Map);
                if (item != null && item.def.EverHaulable)
                {
                    if (!pawn.CanReach((LocalTargetInfo)item, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        List<FloatMenuOption> floatMenuOptionList = opts;
                        TaggedString taggedString2 = "CannotPickUp".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": ";
                        taggedString1 = "NoPath".Translate();
                        TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                        FloatMenuOption floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                        floatMenuOptionList.Add(floatMenuOption);
                    }
                    else if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item, 1))
                        opts.Add(new FloatMenuOption((string)("CannotPickUp".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": " + "TooHeavy".Translate()), (Action)null));
                    else if (item.stackCount == 1)
                    {
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"PickUp".Translate((NamedArgument)item.Label, (NamedArgument)item), (Action)(() =>
                        {
                            item.SetForbidden(false, false);
                            Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, (LocalTargetInfo)item);
                            job.count = 1;
                            job.checkEncumbrance = true;
                            pawn.jobs.TryTakeOrderedJob(job);
                        }), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                    }
                    else
                    {
                        if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item, item.stackCount))
                            opts.Add(new FloatMenuOption((string)("CannotPickUpAll".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": " + "TooHeavy".Translate()), (Action)null));
                        else
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"PickUpAll".Translate((NamedArgument)item.Label, (NamedArgument)item), (Action)(() =>
                            {
                                item.SetForbidden(false, false);
                                Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, (LocalTargetInfo)item);
                                job.count = item.stackCount;
                                job.checkEncumbrance = true;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"PickUpSome".Translate((NamedArgument)item.LabelNoCount, (NamedArgument)item), (Action)(() =>
                        {
                            int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(pawn, item), item.stackCount);
                            Find.WindowStack.Add((Window)new Dialog_Slider((string)"PickUpCount".Translate((NamedArgument)item.LabelNoCount, (NamedArgument)item), 1, to, (Action<int>)(count =>
                            {
                                item.SetForbidden(false, false);
                                Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, (LocalTargetInfo)item);
                                job.count = count;
                                job.checkEncumbrance = true;
                                pawn.jobs.TryTakeOrderedJob(job);
                            })));
                        }), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                    }
                }
            }
            if (!pawn.Map.IsPlayerHome && !pawn.IsFormingCaravan())
            {
                Thing item = c.GetFirstItem(pawn.Map);
                if (item != null && item.def.EverHaulable)
                {
                    Pawn bestPackAnimal = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(pawn);
                    if (bestPackAnimal != null)
                    {
                        if (!pawn.CanReach((LocalTargetInfo)item, PathEndMode.ClosestTouch, Danger.Deadly))
                        {
                            List<FloatMenuOption> floatMenuOptionList = opts;
                            TaggedString taggedString2 = "CannotGiveToPackAnimal".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": ";
                            taggedString1 = "NoPath".Translate();
                            TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                            FloatMenuOption floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                            floatMenuOptionList.Add(floatMenuOption);
                        }
                        else if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item, 1))
                            opts.Add(new FloatMenuOption((string)("CannotGiveToPackAnimal".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": " + "TooHeavy".Translate()), (Action)null));
                        else if (item.stackCount == 1)
                        {
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"GiveToPackAnimal".Translate((NamedArgument)item.Label, (NamedArgument)item), (Action)(() =>
                            {
                                item.SetForbidden(false, false);
                                Job job = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, (LocalTargetInfo)item);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                        }
                        else
                        {
                            if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item, item.stackCount))
                                opts.Add(new FloatMenuOption((string)("CannotGiveToPackAnimalAll".Translate((NamedArgument)item.Label, (NamedArgument)item) + ": " + "TooHeavy".Translate()), (Action)null));
                            else
                                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"GiveToPackAnimalAll".Translate((NamedArgument)item.Label, (NamedArgument)item), (Action)(() =>
                                {
                                    item.SetForbidden(false, false);
                                    Job job = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, (LocalTargetInfo)item);
                                    job.count = item.stackCount;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"GiveToPackAnimalSome".Translate((NamedArgument)item.LabelNoCount, (NamedArgument)item), (Action)(() =>
                            {
                                int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(bestPackAnimal, item), item.stackCount);
                                Find.WindowStack.Add((Window)new Dialog_Slider((string)"GiveToPackAnimalCount".Translate((NamedArgument)item.LabelNoCount, (NamedArgument)item), 1, to, closure_4 ?? (closure_4 = (Action<int>)(count =>
                                {
                                    item.SetForbidden(false, false);
                                    Job job = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, (LocalTargetInfo)item);
                                    job.count = count;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }))));
                            }), MenuOptionPriority.High), pawn, (LocalTargetInfo)item));
                        }
                    }
                }
            }
            if (!pawn.Map.IsPlayerHome && pawn.Map.exitMapGrid.MapUsesExitGrid)
            {
                foreach (LocalTargetInfo target in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    Pawn p = (Pawn)target.Thing;
                    if (p.Faction == Faction.OfPlayer || p.IsPrisonerOfColony || CaravanUtility.ShouldAutoCapture(p, Faction.OfPlayer))
                    {
                        if (!pawn.CanReach((LocalTargetInfo)(Thing)p, PathEndMode.ClosestTouch, Danger.Deadly))
                        {
                            List<FloatMenuOption> floatMenuOptionList = opts;
                            TaggedString taggedString2 = "CannotCarryToExit".Translate((NamedArgument)p.Label, (NamedArgument)(Thing)p) + ": ";
                            taggedString1 = "NoPath".Translate();
                            TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                            FloatMenuOption floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                            floatMenuOptionList.Add(floatMenuOption);
                        }
                        else
                        {
                            IntVec3 exitSpot;
                            if (!RCellFinder.TryFindBestExitSpot(pawn, out exitSpot))
                            {
                                List<FloatMenuOption> floatMenuOptionList = opts;
                                TaggedString taggedString2 = "CannotCarryToExit".Translate((NamedArgument)p.Label, (NamedArgument)(Thing)p) + ": ";
                                taggedString1 = "NoPath".Translate();
                                TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                                FloatMenuOption floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                                floatMenuOptionList.Add(floatMenuOption);
                            }
                            else
                            {
                                TaggedString taggedString2 = p.Faction == Faction.OfPlayer || p.IsPrisonerOfColony ? "CarryToExit".Translate((NamedArgument)p.Label, (NamedArgument)(Thing)p) : "CarryToExitAndCapture".Translate((NamedArgument)p.Label, (NamedArgument)(Thing)p);
                                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)taggedString2, (Action)(() =>
                                {
                                    Job job = JobMaker.MakeJob(JobDefOf.CarryDownedPawnToExit, (LocalTargetInfo)(Thing)p, (LocalTargetInfo)exitSpot);
                                    job.count = 1;
                                    job.failIfCantJoinOrCreateCaravan = true;
                                    pawn.jobs.TryTakeOrderedJob(job);
                                }), MenuOptionPriority.High), pawn, target));
                            }
                        }
                    }
                }
            }
            if (pawn.equipment != null && pawn.equipment.Primary != null && GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForSelf(pawn), true).Any<LocalTargetInfo>())
            {
                if (pawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanUnequip((Thing)pawn.equipment.Primary, pawn))
                {
                    List<FloatMenuOption> floatMenuOptionList = opts;
                    TaggedString taggedString2 = "CannotDrop".Translate((NamedArgument)pawn.equipment.Primary.Label, (NamedArgument)(Thing)pawn.equipment.Primary) + ": ";
                    taggedString1 = "QuestRelated".Translate();
                    TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                    FloatMenuOption floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                    floatMenuOptionList.Add(floatMenuOption);
                }
                else
                {
                    Action action = (Action)(() => pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, (LocalTargetInfo)(Thing)pawn.equipment.Primary)));
                    opts.Add(new FloatMenuOption((string)"Drop".Translate((NamedArgument)pawn.equipment.Primary.Label, (NamedArgument)(Thing)pawn.equipment.Primary), action, revalidateClickTarget: ((Thing)pawn)));
                }
            }
            foreach (LocalTargetInfo dest in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForTrade(), true))
            {
                if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly))
                {
                    List<FloatMenuOption> floatMenuOptionList = opts;
                    TaggedString taggedString2 = "CannotTrade".Translate() + ": ";
                    taggedString1 = "NoPath".Translate();
                    TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                    FloatMenuOption floatMenuOption = new FloatMenuOption((string)(taggedString2 + taggedString3), (Action)null);
                    floatMenuOptionList.Add(floatMenuOption);
                }
                else if (pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
                    opts.Add(new FloatMenuOption((string)"CannotPrioritizeWorkTypeDisabled".Translate((NamedArgument)SkillDefOf.Social.LabelCap), (Action)null));
                else if (!pawn.CanTradeWith(dest.Thing.Faction, ((Pawn)dest.Thing).TraderKind))
                {
                    opts.Add(new FloatMenuOption((string)"CannotTradeMissingTitleAbility".Translate(), (Action)null));
                }
                else
                {
                    Pawn pTarg = (Pawn)dest.Thing;
                    Action action = (Action)(() =>
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.TradeWithPawn, (LocalTargetInfo)(Thing)pTarg);
                        job.playerForced = true;
                        pawn.jobs.TryTakeOrderedJob(job);
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InteractingWithTraders, KnowledgeAmount.Total);
                    });
                    string str = "";
                    if (pTarg.Faction != null)
                        str = " (" + pTarg.Faction.Name + ")";
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)("TradeWith".Translate((NamedArgument)(pTarg.LabelShort + ", " + pTarg.TraderKind.label)) + str), action, MenuOptionPriority.InitiateSocial, revalidateClickTarget: dest.Thing), pawn, (LocalTargetInfo)(Thing)pTarg));
                }
            }
            foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForOpen(pawn), true))
            {
                LocalTargetInfo casket = localTargetInfo;
                if (!pawn.CanReach(casket, PathEndMode.OnCell, Danger.Deadly))
                    opts.Add(new FloatMenuOption((string)("CannotOpen".Translate((NamedArgument)casket.Thing) + ": " + "NoPath".Translate().CapitalizeFirst()), (Action)null));
                else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    opts.Add(new FloatMenuOption((string)("CannotOpen".Translate((NamedArgument)casket.Thing) + ": " + "Incapable".Translate()), (Action)null));
                else if (casket.Thing.Map.designationManager.DesignationOn(casket.Thing, DesignationDefOf.Open) == null)
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption((string)"Open".Translate((NamedArgument)casket.Thing), (Action)(() =>
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.Open, (LocalTargetInfo)casket.Thing);
                        job.ignoreDesignations = true;
                        pawn.jobs.TryTakeOrderedJob(job);
                    }), MenuOptionPriority.High), pawn, (LocalTargetInfo)casket.Thing));
            }
            foreach (Thing thing in pawn.Map.thingGrid.ThingsAt(c))
            {
                foreach (FloatMenuOption floatMenuOption in thing.GetFloatMenuOptions(pawn))
                    opts.Add(floatMenuOption);
            }
            return false;
        }
    }
    */

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddJobGiverWorkOrders_NewTmp")]
    internal class patch_FloatMenuMakerMap_AddJobGiverWorkOrders_NewTmp
    {
        private static FieldInfo f_equivalenceGroupTempStorage = AccessTools.Field(typeof(FloatMenuMakerMap), "equivalenceGroupTempStorage");
        private static AccessTools.FieldRef<FloatMenuOption[]> s_equivalenceGroupTempStorage = AccessTools.StaticFieldRefAccess<FloatMenuOption[]>(f_equivalenceGroupTempStorage);

        [HarmonyPostfix]
        static bool Prefix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts, bool drafted)
        {

            if (pawn.thinker.TryGetMainTreeThinkNode<JobGiver_Work>() == null)
                return false;
            IntVec3 clickCell = IntVec3.FromVector3(clickPos);
            foreach (Thing t in GenUI.ThingsUnderMouse(clickPos, 1f, new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetBuildings = true,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false
            }))
            {
                bool flag = false;
                foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                {
                    for (int index = 0; index < workTypeDef.workGiversByPriority.Count; ++index)
                    {
                        WorkGiverDef workGiver = workTypeDef.workGiversByPriority[index];
                        if ((!drafted || workGiver.canBeDoneWhileDrafted) && (workGiver.Worker is WorkGiver_Scanner worker && worker.def.directOrderable))
                        {
                            JobFailReason.Clear();
                            if ((worker.PotentialWorkThingRequest.Accepts(t) || worker.PotentialWorkThingsGlobal(pawn) != null && worker.PotentialWorkThingsGlobal(pawn).Contains<Thing>(t)) && !worker.ShouldSkip(pawn, true))
                            {
                                Action action = (Action)null;
                                PawnCapacityDef pawnCapacityDef = worker.MissingRequiredCapacity(pawn);
                                string label;
                                if (pawnCapacityDef != null)
                                {
                                    label = (string)"CannotMissingHealthActivities".Translate((NamedArgument)pawnCapacityDef.label);
                                }
                                else
                                {
                                    Job job = worker.HasJobOnThing(pawn, t, true) ? worker.JobOnThing(pawn, t, true) : (Job)null;
                                    if (job == null)
                                    {
                                        if (JobFailReason.HaveReason)
                                            label = (JobFailReason.CustomJobString.NullOrEmpty() ? (string)"CannotGenericWork".Translate((NamedArgument)worker.def.verb, (NamedArgument)t.LabelShort, (NamedArgument)t) : (string)"CannotGenericWorkCustom".Translate((NamedArgument)JobFailReason.CustomJobString)) + ": " + JobFailReason.Reason.CapitalizeFirst();
                                        else if (t.IsForbidden(pawn))
                                        {
                                            // here
                                            //label = t.Position.InAllowedArea(pawn) ? (string)"CannotPrioritizeForbidden".Translate((NamedArgument)t.Label, (NamedArgument)t) : (string)("CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + pawn.playerSettings.EffectiveAreaRestriction.Label);
                                            continue;
                                        }

                                        else
                                            continue;
                                    }
                                    else
                                    {
                                        WorkTypeDef workType = worker.def.workType;
                                        if (pawn.WorkTagIsDisabled(worker.def.workTags))
                                            label = (string)"CannotPrioritizeWorkGiverDisabled".Translate((NamedArgument)worker.def.label);
                                        else if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job))
                                            label = (string)"CannotGenericAlreadyAm".Translate((NamedArgument)worker.PostProcessedGerund(job), (NamedArgument)t.LabelShort, (NamedArgument)t);
                                        else if (pawn.workSettings.GetPriority(workType) == 0)
                                            label = !pawn.WorkTypeIsDisabled(workType) ? (!"CannotPrioritizeNotAssignedToWorkType".CanTranslate() ? (string)"CannotPrioritizeWorkTypeDisabled".Translate((NamedArgument)workType.pawnLabel) : (string)"CannotPrioritizeNotAssignedToWorkType".Translate((NamedArgument)workType.gerundLabel)) : (string)"CannotPrioritizeWorkTypeDisabled".Translate((NamedArgument)workType.gerundLabel);
                                        else if (job.def == JobDefOf.Research && t is Building_ResearchBench)
                                            label = (string)"CannotPrioritizeResearch".Translate();
                                        else if (t.IsForbidden(pawn))
                                        {
                                            // here
                                            //label = t.Position.InAllowedArea(pawn) ? (string)"CannotPrioritizeForbidden".Translate((NamedArgument)t.Label, (NamedArgument)t) : (string)("CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + pawn.playerSettings.EffectiveAreaRestriction.Label);
                                            label = (string)"PrioritizeGeneric".Translate((NamedArgument)worker.PostProcessedGerund(job), (NamedArgument)t.Label);
                                            Job localJob = job;
                                            WorkGiver_Scanner localScanner = worker;
                                            job.workGiverDef = worker.def;
                                            action = (Action)(() =>
                                            {
                                                if (!pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, (WorkGiver)localScanner, clickCell) || workGiver.forceMote == null)
                                                    return;
                                                MoteMaker.MakeStaticMote(clickCell, pawn.Map, workGiver.forceMote);
                                            });
                                        }

                                        else if (!pawn.CanReach((LocalTargetInfo)t, worker.PathEndMode, Danger.Deadly))
                                        {
                                            label = (string)(t.Label + ": " + "NoPath".Translate().CapitalizeFirst()).CapitalizeFirst();
                                        }
                                        else
                                        {
                                            label = (string)"PrioritizeGeneric".Translate((NamedArgument)worker.PostProcessedGerund(job), (NamedArgument)t.Label);
                                            Job localJob = job;
                                            WorkGiver_Scanner localScanner = worker;
                                            job.workGiverDef = worker.def;
                                            action = (Action)(() =>
                                            {
                                                if (!pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, (WorkGiver)localScanner, clickCell) || workGiver.forceMote == null)
                                                    return;
                                                MoteMaker.MakeStaticMote(clickCell, pawn.Map, workGiver.forceMote);
                                            });
                                        }
                                    }
                                }
                                if (DebugViewSettings.showFloatMenuWorkGivers)
                                    label += string.Format(" (from {0})", (object)workGiver.defName);
                                FloatMenuOption menuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), pawn, (LocalTargetInfo)t);
                                if (drafted && workGiver.autoTakeablePriorityDrafted != -1)
                                {
                                    menuOption.autoTakeable = true;
                                    menuOption.autoTakeablePriority = (float)workGiver.autoTakeablePriorityDrafted;
                                }
                                if (!opts.Any<FloatMenuOption>((Predicate<FloatMenuOption>)(op => op.Label == menuOption.Label)))
                                {
                                    if (workGiver.equivalenceGroup != null)
                                    {

                                        if (s_equivalenceGroupTempStorage.Invoke()[(int)workGiver.equivalenceGroup.index] == null || s_equivalenceGroupTempStorage.Invoke()[(int)workGiver.equivalenceGroup.index].Disabled && !menuOption.Disabled)
                                        {
                                            s_equivalenceGroupTempStorage.Invoke()[(int)workGiver.equivalenceGroup.index] = menuOption;
                                            flag = true;
                                        }
                                    }
                                    else
                                        opts.Add(menuOption);
                                }
                            }
                        }
                    }
                }
                if (flag)
                {
                    for (int index = 0; index < s_equivalenceGroupTempStorage.Invoke().Length; ++index)
                    {
                        if (s_equivalenceGroupTempStorage.Invoke()[index] != null)
                        {
                            opts.Add(s_equivalenceGroupTempStorage.Invoke()[index]);
                            s_equivalenceGroupTempStorage.Invoke()[index] = (FloatMenuOption)null;
                        }
                    }
                }
            }
            foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                for (int index = 0; index < workTypeDef.workGiversByPriority.Count; ++index)
                {
                    WorkGiverDef workGiver = workTypeDef.workGiversByPriority[index];
                    if ((!drafted || workGiver.canBeDoneWhileDrafted) && (workGiver.Worker is WorkGiver_Scanner worker && worker.def.directOrderable))
                    {
                        JobFailReason.Clear();
                        if (worker.PotentialWorkCellsGlobal(pawn).Contains<IntVec3>(clickCell) && !worker.ShouldSkip(pawn, true))
                        {
                            Action action = (Action)null;
                            string label = (string)null;
                            PawnCapacityDef pawnCapacityDef = worker.MissingRequiredCapacity(pawn);
                            if (pawnCapacityDef != null)
                            {
                                label = (string)"CannotMissingHealthActivities".Translate((NamedArgument)pawnCapacityDef.label);
                            }
                            else
                            {
                                Job job = worker.HasJobOnCell(pawn, clickCell, true) ? worker.JobOnCell(pawn, clickCell, true) : (Job)null;
                                if (job == null)
                                {
                                    if (JobFailReason.HaveReason)
                                    {
                                        label = JobFailReason.CustomJobString.NullOrEmpty() ? (string)"CannotGenericWork".Translate((NamedArgument)worker.def.verb, (NamedArgument)"AreaLower".Translate()) : (string)"CannotGenericWorkCustom".Translate((NamedArgument)JobFailReason.CustomJobString);
                                        label = label + ": " + JobFailReason.Reason.CapitalizeFirst();
                                    }
                                    else if (clickCell.IsForbidden(pawn))
                                    {
                                        // here
                                        label = clickCell.InAllowedArea(pawn) ? (string)"CannotPrioritizeCellForbidden".Translate() : (string)("CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + pawn.playerSettings.EffectiveAreaRestriction.Label);
                                    }

                                    else
                                        continue;
                                }
                                else
                                {
                                    WorkTypeDef workType = worker.def.workType;
                                    if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job))
                                        label = (string)"CannotGenericAlreadyAmCustom".Translate((NamedArgument)worker.PostProcessedGerund(job));
                                    else if (pawn.workSettings.GetPriority(workType) == 0)
                                        label = !pawn.WorkTypeIsDisabled(workType) ? (!"CannotPrioritizeNotAssignedToWorkType".CanTranslate() ? (string)"CannotPrioritizeWorkTypeDisabled".Translate((NamedArgument)workType.pawnLabel) : (string)"CannotPrioritizeNotAssignedToWorkType".Translate((NamedArgument)workType.gerundLabel)) : (string)"CannotPrioritizeWorkTypeDisabled".Translate((NamedArgument)workType.gerundLabel);
                                    else if (clickCell.IsForbidden(pawn))
                                    {
                                        // here
                                        label = clickCell.InAllowedArea(pawn) ? (string)"CannotPrioritizeCellForbidden".Translate() : (string)("CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + pawn.playerSettings.EffectiveAreaRestriction.Label);
                                    }

                                    else if (!pawn.CanReach((LocalTargetInfo)clickCell, PathEndMode.Touch, Danger.Deadly))
                                    {
                                        TaggedString taggedString1 = "AreaLower".Translate();
                                        TaggedString taggedString2 = taggedString1.CapitalizeFirst() + ": ";
                                        taggedString1 = "NoPath".Translate();
                                        TaggedString taggedString3 = taggedString1.CapitalizeFirst();
                                        label = (string)(taggedString2 + taggedString3);
                                    }
                                    else
                                    {
                                        label = (string)"PrioritizeGeneric".Translate((NamedArgument)worker.PostProcessedGerund(job), (NamedArgument)"AreaLower".Translate());
                                        Job localJob = job;
                                        WorkGiver_Scanner localScanner = worker;
                                        job.workGiverDef = worker.def;
                                        action = (Action)(() =>
                                        {
                                            if (!pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, (WorkGiver)localScanner, clickCell) || workGiver.forceMote == null)
                                                return;
                                            MoteMaker.MakeStaticMote(clickCell, pawn.Map, workGiver.forceMote);
                                        });
                                    }
                                }
                            }
                            if (!opts.Any<FloatMenuOption>((Predicate<FloatMenuOption>)(op => op.Label == label.TrimEnd((char[])Array.Empty<char>()))))
                            {
                                FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), pawn, (LocalTargetInfo)clickCell);
                                if (drafted && workGiver.autoTakeablePriorityDrafted != -1)
                                {
                                    floatMenuOption.autoTakeable = true;
                                    floatMenuOption.autoTakeablePriority = (float)workGiver.autoTakeablePriorityDrafted;
                                }
                                opts.Add(floatMenuOption);
                            }
                        }
                    }
                }
            }
            return false;
        }
    }

}