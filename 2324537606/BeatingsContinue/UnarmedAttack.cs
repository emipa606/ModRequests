using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace BeatingsContinue
{
    class UnarmedAttack
    {
        static string defaultLabel { get { return "Unarmed Attack"; } }
        static string defaultDesc { get { return "Punch Them!"; } }
        static JobDef jobDef { get { return BeatingsDefsOf.jobDefUnarmed; } }

        public static Gizmo GetGizmo(Pawn pawn)
        {
            Command_Target command_Target = new Command_Target();
            command_Target.defaultLabel = defaultLabel;
            //command_Target.defaultLabel = "CommandMeleeAttack".Translate();
            command_Target.defaultDesc = defaultDesc;
            //command_Target.defaultDesc = "CommandMeleeAttackDesc".Translate();
            command_Target.targetingParams = TargetingParameters.ForAttackAny();
            //command_Target.hotKey = KeyBindingDefOf.Misc2;
            command_Target.icon = TexCommand.AttackMelee;
            if (GetAttackAction(pawn, LocalTargetInfo.Invalid, out string failStr) == null)
            {
                command_Target.Disable(failStr.CapitalizeFirst() + ".");
            }
            command_Target.action = delegate (Thing target)
            {
                foreach (Pawn item in Find.Selector.SelectedObjects.Where(delegate (object x)
                {
                    Pawn pawn2 = x as Pawn;
                    return pawn2 != null && pawn2.IsColonistPlayerControlled;
                    //if (pawn2 != null && pawn2.IsColonistPlayerControlled)
                    //{
                    //    return pawn2.Drafted;
                    //}
                    //return false;
                }).Cast<Pawn>())
                {
                    string failStr2;
                    Action unarmedAttackAction = GetAttackAction(item, target, out failStr2);
                    if (unarmedAttackAction != null)
                    {
                        unarmedAttackAction();
                    }
                    else if (!failStr2.NullOrEmpty())
                    {
                        Messages.Message(failStr2, target, MessageTypeDefOf.RejectInput, historical: false);
                    }
                }
            };
            return command_Target;
        }

        public static Action GetAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
        {
            failStr = "";
            if (!pawn.IsColonistPlayerControlled)
            {
                failStr = "CannotOrderNonControlledLower".Translate();
            }
            else if (target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
            {
                failStr = "NoPath".Translate();
            }
            else if (pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                failStr = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
            }
            else if (pawn.meleeVerbs.TryGetMeleeVerb(target.Thing) == null)
            {
                failStr = "Incapable".Translate();
            }
            else if (pawn == target.Thing)
            {
                failStr = "CannotAttackSelf".Translate();
            }
            else
            {
                Pawn target2;
                if ((target2 = (target.Thing as Pawn)) == null || (!pawn.InSameExtraFaction(target2, ExtraFactionType.HomeFaction) && !pawn.InSameExtraFaction(target2, ExtraFactionType.MiniFaction)))
                {
                    return delegate
                    {
                        if (InteractionUtility.TryGetRandomVerbForSocialFight(pawn, out Verb verb))
                        {
                            Job job = JobMaker.MakeJob(jobDef, target.Thing as Pawn);
                            //job.maxNumMeleeAttacks = 1;
                            //job.killIncappedTarget = false;
                            Pawn pawn2 = target.Thing as Pawn;
                            if (pawn2 != null)
                            {
                                job.killIncappedTarget = pawn2.Downed;
                            }
                            //job.verbToUse = verb;
                            pawn.jobs.TryTakeOrderedJob(job);
                        }
                        //Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                        //pawn.jobs.TryTakeOrderedJob(job);
                    };
                }
                failStr = "CannotAttackSameFactionMember".Translate();
            }
            failStr = failStr.CapitalizeFirst();
            return null;
        }
    }
}
