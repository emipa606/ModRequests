using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace NoobertNebulous
{
    public abstract class JobDriver_UseStreamingConsole : JobDriver
    {
        public CompStreamingConsole CompStreamingConsole => this.TargetA.Thing.TryGetComp<CompStreamingConsole>();
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell)
                .FailOn((Toil to) =>  !CompStreamingConsole.CanUseConsoleNow);
            if (WaitPeriod() > 0)
            {
                var wait = Toils_General.Wait(WaitPeriod(), TargetIndex.A)
                    .WithProgressBarToilDelay(TargetIndex.A);
                if (Sustainer() != null)
                {
                    wait = wait.PlaySustainerOrSound(Sustainer());
                }
                if (Effecter() != null)
                {
                    wait = wait.WithEffect(() => Effecter(), () => CompStreamingConsole.parent.OccupiedRect().ClosestCellTo(pawn.Position));
                }
                wait.AddPreTickAction(PretickWaitAction);
                yield return wait;
            }
            yield return Toils_General.Do(DoAction);
        }

        public virtual void PretickWaitAction()
        {
            var xpToLearn = 120f / GenDate.TicksPerHour;
            var skill = pawn.skills.GetSkill(SkillDefOf.Social);
            if (skill.passion == Passion.None)
            {
                xpToLearn *= 0.35f;
            }
            else if (skill.passion == Passion.Major)
            {
                xpToLearn *= 1.5f;
            }
            xpToLearn *= pawn.GetStatValue(StatDefOf.GlobalLearningFactor);
            skill.Learn(xpToLearn);
        }

        protected virtual int WaitPeriod() => 0;
        protected abstract void DoAction();
        protected virtual SoundDef Sustainer() => null;
        protected virtual EffecterDef Effecter() => null;
    }

    public class JobDriver_SetupChannel : JobDriver_UseStreamingConsole
    {
        protected override void DoAction()
        {
            pawn.CreateVidtubeChannel();
            Find.WindowStack.Add(new Window_Vidtube(pawn, CompStreamingConsole));
        }
    }

    public class JobDriver_OpenVidtube : JobDriver_UseStreamingConsole
    {
        protected override void DoAction()
        {
            Find.WindowStack.Add(new Window_Vidtube(pawn, CompStreamingConsole));
        }
    }

    public class JobDriver_DeleteChannel : JobDriver_UseStreamingConsole
    {
        protected override void DoAction()
        {
            Find.WindowStack.Add(new Dialog_MessageBox("NN.DeleteChannelConfirmation".Translate(pawn.Named("PAWN")), "Confirm".Translate(), delegate
            {
                NoobertNebulousStoryteller.Instance.vidtubers.Remove(pawn);
            }, "Cancel".Translate()));
        }
    }

    public class JobDriver_RecordVideo : JobDriver_UseStreamingConsole
    {
        protected override SoundDef Sustainer()
        {
            return NN_DefOf.NN_RecordingVidtube;
        }

        protected override EffecterDef Effecter()
        {
            return NN_DefOf.NN_StreamingConsoleWorking;
        }
        protected override int WaitPeriod()
        {
            return CompStreamingConsole.videoToRecord.WorkToMakeVideo;
        }
        protected override void DoAction()
        {
            pawn.GetVidtubeChannel().AddVideo(CompStreamingConsole.videoToRecord);
            CompStreamingConsole.videoToRecord = null;
            NN_DefOf.NN_VidtubeUpload.PlayOneShot(CompStreamingConsole.parent);
        }
    }
}
