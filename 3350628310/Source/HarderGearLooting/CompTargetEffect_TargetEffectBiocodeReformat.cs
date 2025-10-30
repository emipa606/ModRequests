using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace HarderGearLooting
{
    public class CompProperties_BiocodeReformat : CompProperties
    {
        public CompProperties_BiocodeReformat()
        {
            this.compClass = typeof(CompTargetEffect_TargetEffectBiocodeReformat);
        }
    }

    public class CompTargetEffect_TargetEffectBiocodeReformat: CompTargetEffect
    {
        public CompProperties_BiocodeReformat Props
        {
            get => (CompProperties_BiocodeReformat)this.props;
        }
        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (! (target is ThingWithComps targetWithComps))
            {
                Log.Error($"Trying to apply biorecode on a Thing without components.");
                return;
            }
            Job job = JobMaker.MakeJob(HardGearLooting_DefOf.ApplyBiocodeReformatter, (LocalTargetInfo)target, (LocalTargetInfo)(Thing)this.parent);
            job.count = 1;
            job.playerForced = true;
            user.jobs.TryTakeOrderedJob(job);
        }
    }
}
