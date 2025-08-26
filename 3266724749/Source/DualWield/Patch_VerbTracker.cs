using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl.DualWield
{
    [HarmonyPatch(typeof(VerbTracker), nameof(VerbTracker.CreateVerbTargetCommand))]
    class Patch_VerbTracker_CreateVerbTargetCommand
    {
        static bool Prepare()
        {
            return Settings.dualWieldEnabled;
        }

        static Command_VerbTarget Postfix(Command_VerbTarget __result, VerbTracker __instance, Thing ownerThing, Verb verb)
        {
            if (ownerThing is ThingWithComps twc && twc.ParentHolder is Pawn_EquipmentTracker peqt && peqt.pawn.GetOffHander(out ThingWithComps offHandEquip) && offHandEquip != twc)
                return CreateDualWieldCommand(ownerThing, offHandEquip, verb);

            return __result;
        }

        static Command_VerbTarget CreateVerbTargetCommand(VerbTracker __instance, Thing ownerThing, Verb verb)
        {
            Command_VerbTarget command_VerbTarget = new Command_VerbTarget();
            ThingStyleDef styleDef = ownerThing.StyleDef;
            command_VerbTarget.defaultDesc = ownerThing.LabelCap + ": " + ownerThing.def.description.CapitalizeFirst();
            command_VerbTarget.icon = ((styleDef != null && styleDef.UIIcon != null) ? styleDef.UIIcon : ownerThing.def.uiIcon);
            command_VerbTarget.iconAngle = ownerThing.def.uiIconAngle;
            command_VerbTarget.iconOffset = ownerThing.def.uiIconOffset;
            command_VerbTarget.tutorTag = "VerbTarget";
            command_VerbTarget.verb = verb;
            if (verb.caster.Faction != Faction.OfPlayer && !DebugSettings.ShowDevGizmos)
            {
                command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
            }
            else if (verb.CasterIsPawn)
            {
                if (verb.CasterPawn.RaceProps.IsMechanoid && !MechanitorUtility.EverControllable(verb.CasterPawn) && !DebugSettings.ShowDevGizmos)
                {
                    command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
                }
                else if (verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                {
                    command_VerbTarget.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (!verb.CasterPawn.Drafted && !DebugSettings.ShowDevGizmos)
                {
                    command_VerbTarget.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (verb is Verb_LaunchProjectile)
                {
                    Apparel apparel = verb.FirstApparelPreventingShooting();
                    if (apparel != null)
                    {
                        command_VerbTarget.Disable("ApparelPreventsShooting".Translate(verb.CasterPawn.Named("PAWN"), apparel.Named("APPAREL")).CapitalizeFirst());
                    }
                }
                else if (EquipmentUtility.RolePreventsFromUsing(verb.CasterPawn, verb.EquipmentSource, out var reason))
                {
                    command_VerbTarget.Disable(reason);
                }
            }

            return command_VerbTarget;
        }

        static Command_VerbTarget CreateDualWieldCommand(Thing ownerThing, Thing offHandThing, Verb verb)
        {
            Command_DualWield command_VerbTarget = new Command_DualWield(offHandThing)
            {
                defaultDesc = ownerThing.LabelCap + ": " + ownerThing.def.description.CapitalizeFirst(),
                icon = ownerThing.def.uiIcon,
                iconAngle = ownerThing.def.uiIconAngle,
                iconOffset = ownerThing.def.uiIconOffset,
                tutorTag = "VerbTarget",
                verb = verb
            };
            if (verb.caster.Faction != Faction.OfPlayer && !DebugSettings.ShowDevGizmos)
            {
                command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
            }
            else if (verb.CasterIsPawn)
            {
                if (verb.CasterPawn.RaceProps.IsMechanoid && !MechanitorUtility.EverControllable(verb.CasterPawn) && !DebugSettings.ShowDevGizmos)
                {
                    command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
                }
                else if (verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
                {
                    command_VerbTarget.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (!verb.CasterPawn.Drafted && !DebugSettings.ShowDevGizmos)
                {
                    command_VerbTarget.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (verb is Verb_LaunchProjectile)
                {
                    Apparel apparel = verb.FirstApparelPreventingShooting();
                    if (apparel != null)
                    {
                        command_VerbTarget.Disable("ApparelPreventsShooting".Translate(verb.CasterPawn.Named("PAWN"), apparel.Named("APPAREL")).CapitalizeFirst());
                    }
                }
                else if (EquipmentUtility.RolePreventsFromUsing(verb.CasterPawn, verb.EquipmentSource, out var reason))
                {
                    command_VerbTarget.Disable(reason);
                }
            }

            return command_VerbTarget;
        }
    }

    [HarmonyPatch(typeof(VerbTracker), nameof(VerbTracker.GetVerbsCommands))]
    class Patch_VerbTracker_GetVerbsCommands_Postfix
    {
        static IEnumerable<Command> Postfix(IEnumerable<Command> values, VerbTracker __instance)
        {
            foreach (Command command in values)
            {
                if (command is Command_VerbTarget cVerbTarget)
                {
                    Verb verb = cVerbTarget.verb;

                    if (verb.EquipmentSource is ThingWithComps twc && twc.ParentHolder is Pawn_EquipmentTracker peqt)
                    {
                        //Remove offHand gizmo when dual wielding
                        //Don't remove offHand gizmo when offHand weapon is the only weapon being carried by the pawn
                        //TODO: look at this closer
                        if (peqt.pawn.GetOffHander(out ThingWithComps offHandEquip) && offHandEquip == twc && offHandEquip != peqt.Primary)
                            continue;
                    }
                }

                yield return command;
            }
        }
    }
}
