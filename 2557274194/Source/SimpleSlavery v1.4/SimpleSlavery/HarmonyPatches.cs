using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SimpleSlaveryCollars
{
    // Mood buff when releasing slaves, the player's faction.
    [HarmonyPatch(typeof(GenGuest), "SlaveRelease")]
    public static class GenGuest_SlaveRelease_Patch
    {
        [HarmonyPostfix]
        public static void SlaveRelease_Patch(Pawn p)
        {
            if (SlaveUtility.TimeAsSlave(p) >= SlaveUtility.SlaveStage4 && !SlaveUtility.IsSteadfast(p) && p.Faction == Faction.OfPlayer && p.needs.mood != null)
            {
                p.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.WasEnslaved);
                p.needs.mood.thoughts.memories.TryGainMemory(SS_ThoughtDefOf.WasEnslaved_Assimilation);
            }
        }
    }
    // Change the slave's faction to the player's faction when the slave stage reaches 5.
    [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    public static class Pawn_GuestTracker_SetGuestStatus_Patch
    {
        [HarmonyPostfix]
        public static void SetGuestStatus_Patch(Pawn_GuestTracker __instance, ref Faction ___slaveFactionInt, Pawn ___pawn)
        {
            if (SimpleSlaveryCollarsSetting.SlavestageEnable == false || SimpleSlaveryCollarsSetting.AssimilationSlaveEnable == false)
                return;
            if (SlaveUtility.TimeAsSlave(___pawn) >= SlaveUtility.SlaveStage4 && !SlaveUtility.IsSteadfast(___pawn) && ___pawn.IsSlaveOfColony && __instance.SlaveFaction != Faction.OfPlayer)
            {
                ___slaveFactionInt = Faction.OfPlayer;
                Messages.Message((string)"MessageAssimilationSlave".Translate().AdjustedFor(___pawn), (LookTargets)(Thing)___pawn, MessageTypeDefOf.NeutralEvent);
            }
        }
    }
    // Remove global work speed debuff from slave of Slave Stage 5.
    [HarmonyPatch(typeof(StatPart_Slave), "ActiveFor")]
    public static class StatPartSlave_ActiveFor_Patch
    {
        [HarmonyPostfix]
        public static void ActiveFor_Patch(Thing t, ref bool __result)
        {
            if (SimpleSlaveryCollarsSetting.SlavestageEnable == false || SimpleSlaveryCollarsSetting.RebelCycleChangeEnable == false || SimpleSlaveryCollarsSetting.RemoveWorkspeedDebuffEnable == false)
                return;
            if (t is Pawn pawn && SlaveUtility.TimeAsSlave(pawn) >= SlaveUtility.SlaveStage4 && !SlaveUtility.IsSteadfast(pawn))
                __result = false;
        }
    }
    // Add role assignment button for slave pawns.
    [HarmonyPatch(typeof(SocialCardUtility), "DrawPawnRole")]

    public static class SocialCardUtility_DrawPawnRole_Patch
    {
        [HarmonyPostfix]
        public static void DrawPawnRole_Patch(Pawn pawn, Rect rect, List<Precept_Role> ___cachedRoles)
        {
            if (SimpleSlaveryCollarsSetting.SlavestageEnable == false || SimpleSlaveryCollarsSetting.RebelCycleChangeEnable == false || SimpleSlaveryCollarsSetting.AssignSlaveEnable == false)
                return;
            Precept_Role currentRole = pawn.Ideo?.GetRole(pawn);
            Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
            Rect rect2 = rect;
            if (pawn.IsFreeColonist && pawn.IsSlave)
            {
                bool active = ___cachedRoles.Any<Precept_Role>();
                if (!active)
                    GUI.color = Color.gray;
                float y = (float)((double)rect2.y + (double)rect2.height / 2.0 - 14.0);
                if (Widgets.ButtonText(new Rect(rect.width - 150f, y, 115f, 28f)
                {
                    xMax = (float)((double)rect.width - 26.0 - 4.0)
                }, (string)("ChooseRole".Translate() + "..."), active: active))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    if (currentRole != null)
                        options.Add(new FloatMenuOption((string)"RemoveCurrentRole".Translate(), (Action)(() => Find.WindowStack.Add((Window)Dialog_MessageBox.CreateConfirmation("ChooseRoleConfirmUnassign".Translate(currentRole.Named("ROLE"), pawn.Named("PAWN")) + "\n\n" + "ChooseRoleConfirmAssignPostfix".Translate(), (Action)(() => currentRole.Unassign(pawn, true))))), Widgets.PlaceholderIconTex, Color.white));
                    if (pawn.Ideo != null)
                    {
                        foreach (Precept_Role cachedRole in ___cachedRoles)
                        {
                            Precept_Role newRole = cachedRole;
                            if (newRole != currentRole && newRole.Active && newRole.RequirementsMet(pawn) && (!newRole.def.leaderRole || pawn.Ideo == primaryIdeo))
                            {
                                bool flag1 = false;
                                TaggedString confirmText = "ChooseRoleConfirmAssign".Translate(newRole.Named("ROLE"), pawn.Named("PAWN"));
                                string label2 = newRole.LabelForPawn(pawn) + " (" + newRole.def.label + ")";
                                if (currentRole != null)
                                {
                                    confirmText += " " + "ChooseRoleConfirmAssignHasOtherRole".Translate(pawn.Named("PAWN"), currentRole.Named("ROLE"));
                                    flag1 = true;
                                }
                                Pawn pawn1 = newRole.ChosenPawns().FirstOrDefault<Pawn>();
                                if (pawn1 != null && newRole is Precept_RoleSingle)
                                {
                                    label2 = label2 + ": " + pawn1.LabelShort;
                                    confirmText += " " + "ChooseRoleConfirmAssignReplace".Translate(pawn1.Named("PAWN"));
                                    flag1 = true;
                                }
                                else if (newRole.def.leaderRole)
                                {
                                    foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
                                    {
                                        foreach (Precept precept in allIdeo.PreceptsListForReading)
                                        {
                                            if (precept != currentRole && precept is Precept_Role preceptRole14)
                                            {
                                                Pawn pawn2 = preceptRole14.ChosenPawnSingle();
                                                if (preceptRole14.def.leaderRole && pawn2 != null && pawn2 != pawn && pawn2.IsFreeColonist)
                                                {
                                                    confirmText += " " + "ChooseRoleConfirmAssignReplaceLeader".Translate(pawn2.Named("PAWN"), newRole.Named("ROLE"), preceptRole14.Named("OTHERROLE"));
                                                    flag1 = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                IEnumerable<WorkTypeDef> disabledWorkTypes = newRole.DisabledWorkTypes;
                                if (disabledWorkTypes.Any<WorkTypeDef>())
                                {
                                    flag1 = true;
                                    if (!confirmText.NullOrEmpty())
                                        confirmText += "\n\n";
                                    confirmText += "ChooseRoleListWorkTypeRestrictions".Translate(pawn.Named("PAWN")) + ": \n" + disabledWorkTypes.Select<WorkTypeDef, string>((Func<WorkTypeDef, string>)(x => x.labelShort.ToString())).ToLineList("  - ");
                                }
                                if (!newRole.def.grantedAbilities.NullOrEmpty<AbilityDef>())
                                {
                                    flag1 = true;
                                    if (!confirmText.NullOrEmpty())
                                        confirmText += "\n\n";
                                    confirmText += "ChooseRoleListAbilities".Translate(pawn.Named("PAWN")) + ": \n" + newRole.def.grantedAbilities.Select<AbilityDef, string>((Func<AbilityDef, string>)(x => x.LabelCap.ToString())).ToLineList("  - ");
                                }
                                if (!newRole.apparelRequirements.NullOrEmpty<PreceptApparelRequirement>())
                                {
                                    flag1 = true;
                                    if (!confirmText.NullOrEmpty())
                                        confirmText += "\n\n";
                                    confirmText += "ChooseRoleListApparelDemands".Translate(newRole.Named("ROLE")) + ": \n" + newRole.AllApparelRequirementLabels(pawn.gender, pawn).ToLineList("  - ");
                                }
                                if (newRole.def.roleRequiredWorkTags != WorkTags.None)
                                {
                                    List<string> stringList = new List<string>();
                                    foreach (WorkTags allSelectedItem in newRole.def.roleRequiredWorkTags.GetAllSelectedItems<WorkTags>())
                                    {
                                        if (pawn.WorkTagIsDisabled(allSelectedItem))
                                            stringList.Add(allSelectedItem.LabelTranslated().CapitalizeFirst());
                                    }
                                    if (stringList.Any<string>())
                                    {
                                        flag1 = true;
                                        if (!confirmText.NullOrEmpty())
                                            confirmText += "\n\n";
                                        confirmText += "ChooseRoleRequiredWorkTagsDisabled".Translate(pawn.Named("PAWN"), newRole.Named("ROLE")) + ": \n" + stringList.ToLineList("  - ");
                                    }
                                }
                                else if (newRole.def.roleRequiredWorkTagAny != WorkTags.None)
                                {
                                    bool flag2 = false;
                                    List<string> entries = new List<string>();
                                    foreach (WorkTags allSelectedItem in newRole.def.roleRequiredWorkTagAny.GetAllSelectedItems<WorkTags>())
                                    {
                                        if (!pawn.WorkTagIsDisabled(allSelectedItem))
                                        {
                                            flag2 = true;
                                            break;
                                        }
                                        entries.Add(allSelectedItem.LabelTranslated().CapitalizeFirst());
                                    }
                                    if (!flag2)
                                    {
                                        flag1 = true;
                                        if (!confirmText.NullOrEmpty())
                                            confirmText += "\n\n";
                                        confirmText += "ChooseRoleRequiredWorkTagsDisabled".Translate(pawn.Named("PAWN"), newRole.Named("ROLE")) + ": \n" + entries.ToLineList("  - ");
                                    }
                                }
                                if (newRole.ChosenPawnSingle() == null && newRole is Precept_RoleSingle)
                                {
                                    flag1 = true;
                                    if (!confirmText.NullOrEmpty())
                                        confirmText += "\n\n";
                                    confirmText += "ChooseRoleHint".Translate();
                                }
                                if (flag1)
                                    options.Add(new FloatMenuOption(label2, (Action)(() =>
                                    {
                                        confirmText += "\n\n" + "ChooseRoleConfirmAssignPostfix".Translate();
                                        Find.WindowStack.Add((Window)Dialog_MessageBox.CreateConfirmation(confirmText, (Action)(() =>
                                        {
                                            currentRole?.Unassign(pawn, true);
                                            newRole.Assign(pawn, true);
                                        })));
                                    }), newRole.Icon, newRole.ideo.Color, mouseoverGuiAction: new Action<Rect>(DrawTooltip))
                                    {
                                        orderInPriority = newRole.def.displayOrderInImpact
                                    });
                                else
                                    options.Add(new FloatMenuOption(label2, (Action)(() => newRole.Assign(pawn, true)), newRole.Icon, newRole.ideo.Color, mouseoverGuiAction: new Action<Rect>(DrawTooltip))
                                    {
                                        orderInPriority = newRole.def.displayOrderInImpact
                                    });
                            }

                            void DrawTooltip(Rect r)
                            {
                                TipSignal tip = new TipSignal((Func<string>)(() => newRole.GetTip()), pawn.thingIDNumber * 39);
                                TooltipHandler.TipRegion(r, tip);
                            }
                        }
                        foreach (Precept_Role cachedRole in ___cachedRoles)
                        {
                            if (cachedRole != currentRole && !cachedRole.RequirementsMet(pawn) || !cachedRole.Active)
                            {
                                string label3 = cachedRole.LabelForPawn(pawn) + " (" + cachedRole.def.label + ")";
                                if (cachedRole.ChosenPawnSingle() != null)
                                    label3 = label3 + ": " + cachedRole.ChosenPawnSingle().LabelShort;
                                else if (!cachedRole.RequirementsMet(pawn))
                                    label3 = label3 + ": " + cachedRole.GetFirstUnmetRequirement(pawn).GetLabel(cachedRole).CapitalizeFirst();
                                else if (!cachedRole.Active && cachedRole.def.activationBelieverCount > cachedRole.ideo.ColonistBelieverCountCached)
                                    label3 = (string)(label3 + (": " + "InactiveRoleRequiresMoreBelievers".Translate((NamedArgument)cachedRole.def.activationBelieverCount, (NamedArgument)cachedRole.ideo.memberName, (NamedArgument)cachedRole.ideo.ColonistBelieverCountCached).CapitalizeFirst()));
                                options.Add(new FloatMenuOption(label3, (Action)null, cachedRole.Icon, cachedRole.ideo.Color)
                                {
                                    orderInPriority = cachedRole.def.displayOrderInImpact
                                });
                            }
                        }
                    }
                    Find.WindowStack.Add((Window)new FloatMenu(options));
                }
                GUI.color = Color.white;
            }
        }
    }
    // Allow role assignment of a slave pawn.
    [HarmonyPatch(typeof(Precept_Role), "ValidatePawn")]
    public static class PreceptRole_ValidatePawn_Patch
    {
        [HarmonyPostfix]
        public static void ValidatePawn_Patch(ref bool __result, Pawn p, Precept_Role __instance)
        {
            if (SimpleSlaveryCollarsSetting.SlavestageEnable == false || SimpleSlaveryCollarsSetting.RebelCycleChangeEnable == false || SimpleSlaveryCollarsSetting.AssignSlaveEnable == false)
                return;
            if (__result == false && (p.Faction != null && (!p.Faction.IsPlayer || p.IsFreeColonist) && !p.Destroyed && !p.Dead && __instance.RequirementsMet(p)))
                __result = true;
        }
    }
    // Remove Rebellion Cycle according to Slave Stage 5.
    [HarmonyPatch(typeof(SlaveRebellionUtility), "CanParticipateInSlaveRebellion")]
    public static class SlaveRebellionUtility_CanParticipateInSlaveRebellion_Patch
    {
        [HarmonyPostfix]
        public static void CanParticipateInSlaveRebellion_Patch(ref Pawn pawn, ref bool __result)
        {
            if (SimpleSlaveryCollarsSetting.SlavestageEnable == false || SimpleSlaveryCollarsSetting.RebelCycleChangeEnable == false)
                return;
            if (SlaveUtility.TimeAsSlave(pawn) >= SlaveUtility.SlaveStage4 && !SlaveUtility.IsSteadfast(pawn))
                __result = false;
        }
    }
    // Added Rebellion Cycle Effect according to Slave Stage.
    [HarmonyPatch(typeof(SlaveRebellionUtility), "InitiateSlaveRebellionMtbDays")]
    public static class SlaveRebellionUtility_InitiateSlaveRebellionMtbDays_Patch
    {
        [HarmonyPostfix]
        public static void InitiateSlaveRebellionMtbDays_Patch(ref Pawn pawn, ref float __result)
        {
            if (SimpleSlaveryCollarsSetting.SlavestageEnable == false || SimpleSlaveryCollarsSetting.RebelCycleChangeEnable == false || __result == -1f)
                return;
            else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage1)
                __result *= 1f;
            else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage2)
                __result *= 1.5f;
            else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage3)
                __result *= 1.75f;
            else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage4 || (SlaveUtility.TimeAsSlave(pawn) >= SlaveUtility.SlaveStage3 && SlaveUtility.IsSteadfast(pawn)))
                __result *= 2f;
        }
    }
    // Added Rebellion Cycle Description according to Slave Stage.
    [HarmonyPatch(typeof(SlaveRebellionUtility), "GetSlaveRebellionMtbCalculationExplanation")]
    public static class SlaveRebellionUtility_GetSlaveRebellionMtbCalculationExplanation_Patch
    {
        [HarmonyPostfix]
        public static void GetSlaveRebellionMtbCalculationExplanation_Patch(ref Pawn pawn, ref string __result)
        {
            Need_Suppression need = pawn.needs.TryGetNeed<Need_Suppression>();
            if (SimpleSlaveryCollarsSetting.SlavestageEnable == false || SimpleSlaveryCollarsSetting.RebelCycleChangeEnable == false || need == null || !SlaveRebellionUtility.CanParticipateInSlaveRebellion(pawn))
                return;
            StringBuilder stringBuilder = new StringBuilder();
            if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage1)
            {
                float f4 = 1f;
                stringBuilder.AppendLine(string.Format("\n{0}: x{1}", (object)"SuppressionSlavestageFactor".Translate(), (object)f4.ToStringPercent()));
            }
            else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage2)
            {
                float f4 = 1.5f;
                stringBuilder.AppendLine(string.Format("\n{0}: x{1}", (object)"SuppressionSlavestageFactor".Translate(), (object)f4.ToStringPercent()));
            }
            else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage3)
            {
                float f4 = 1.75f;
                stringBuilder.AppendLine(string.Format("\n{0}: x{1}", (object)"SuppressionSlavestageFactor".Translate(), (object)f4.ToStringPercent()));
            }
            else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage4 || (SlaveUtility.TimeAsSlave(pawn) >= SlaveUtility.SlaveStage3 && SlaveUtility.IsSteadfast(pawn)))
            {
                float f4 = 2f;
                stringBuilder.AppendLine(string.Format("\n{0}: x{1}", (object)"SuppressionSlavestageFactor".Translate(), (object)f4.ToStringPercent()));
            }
            stringBuilder.AppendLine(string.Format("{0}: {1}", (object)"SuppressionFinalInterval".Translate(), (object)((int)((double)SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(pawn) * 60000.0)).ToStringTicksToPeriod()));
            __result = __result.Remove(__result.LastIndexOf(Environment.NewLine));
            __result += stringBuilder.ToString();
        }
    }
    // Turn off pawn's collar when stripping pawns.
    [HarmonyPatch(typeof(Pawn), "Strip")]
    public static class Pawn_Strip_Patch
    {
        [HarmonyPrefix]
        public static void Strip_Patch(ref Pawn __instance)
        {
            if (SlaveUtility.HasSlaveCollar(__instance) && SlaveUtility.GetSlaveCollar(__instance).def.thingClass == typeof(SlaveCollar_Crypto))
            {
                (SlaveUtility.GetSlaveCollar(__instance) as SlaveCollar_Crypto).armed = false;
                if (!__instance.Dead)
                {
                    (SlaveUtility.GetSlaveCollar(__instance) as SlaveCollar_Crypto).RevertMentalState();
                }
            }
            if (SlaveUtility.HasSlaveCollar(__instance) && SlaveUtility.GetSlaveCollar(__instance).def.thingClass == typeof(SlaveCollar_Electric))
            {
                (SlaveUtility.GetSlaveCollar(__instance) as SlaveCollar_Electric).armed = false;
            }
        }
    }
    // Remove enslave hediff when slaves are freed.
    [HarmonyPatch(typeof(GenGuest), "EmancipateSlave")]
    public static class GenGuest_EmancipateSlave_Patch
    {
        [HarmonyPostfix]
        public static void EmancipateSlave_Patch(ref Pawn slave)
        {
            Hediff enslaved = slave.health.hediffSet.GetFirstHediffOfDef(SS_HediffDefOf.Enslaved);
            if (slave.health.hediffSet.HasHediff(SS_HediffDefOf.Enslaved))
                slave.health.RemoveHediff(enslaved);
        }
    }
    // Add enslave hediff when pawn become a slave.
    [HarmonyPatch(typeof(GenGuest), "EnslavePrisoner")]
    public static class GenGuest_EnslavePrisoner_Patch
    {
        [HarmonyPostfix]
        public static void EnslavePrisoner_Patch(ref Pawn prisoner)
        {
            if (!prisoner.health.hediffSet.HasHediff(SS_HediffDefOf.Enslaved))
                prisoner.health.AddHediff(SS_HediffDefOf.Enslaved);
            if (SimpleSlaveryCollarsSetting.ShacklesDefault == false)
                SlaveUtility.GetEnslavedHediff(prisoner).shackledGoal = false;
        }
    }
    // Patch for when a slave is sold
    [HarmonyPatch(typeof(Pawn), "PreTraded")]
    public static class Pawn_PreTraded_Patch
    {
        [HarmonyPostfix]
        public static void PreTraded_Patch(ref Pawn __instance, ref TradeAction action)
        {
            Hediff enslaved = __instance.health.hediffSet.GetFirstHediffOfDef(SS_HediffDefOf.Enslaved);
            if (action == TradeAction.PlayerBuys && __instance.RaceProps.Humanlike && !__instance.health.hediffSet.HasHediff(SS_HediffDefOf.Enslaved) && __instance.IsSlaveOfColony)
            {
                __instance.health.AddHediff(SS_HediffDefOf.Enslaved);
            }
            else if (action == TradeAction.PlayerSells && __instance.health.hediffSet.HasHediff(SS_HediffDefOf.Enslaved))
            {
                __instance.health.RemoveHediff(enslaved);
            }
        }
    }
    // Changes the behaviour of restraints to acknowledge shackled slaves
    [HarmonyPatch(typeof(RestraintsUtility), "InRestraints")]
    public static class RestraintUtility_Patch
    {
        [HarmonyPostfix]
        public static void InRestraints_Patch(ref Pawn pawn, ref bool __result)
        {
            // Pawn is a shackled slave
            if (pawn.IsSlaveOfColony && pawn.health.hediffSet.HasHediff(SS_HediffDefOf.Enslaved))
                __result = SlaveUtility.GetEnslavedHediff(pawn).shackled;
        }
    }
    [HarmonyPatch(typeof(RestraintsUtility), "ShouldShowRestraintsInfo")]
    public static class RestraintUtility_Show_Patch
    {
        [HarmonyPostfix]
        public static void ShouldShowRestraintsInfo_Patch(ref Pawn pawn, ref bool __result)
        {
            if (RestraintsUtility.InRestraints(pawn) && pawn.IsSlaveOfColony)
            {
                __result = true;
            }
        }
    }
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_Patch
    {
        static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = __result.Concat(SlaveGizmos(__instance));
        }

        internal static IEnumerable<Gizmo> SlaveGizmos(Pawn pawn)
        {
            if (!SlaveUtility.IsColonyMember(pawn))
            { // Only display the apparel gizmos if the pawn isn't a slave, not a colonist, not a prisoner.
                yield break;
            }
            if (pawn.apparel != null)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    var slaveApparel = apparel as SlaveApparel;
                    if (slaveApparel != null)
                    {
                        foreach (var g in slaveApparel.SlaveGizmos()) yield return g;
                    }
                }
            }
        }
    }
}

