using RimWorld;
using Verse;
using Verse.Sound;

namespace Roos_Satyr_Xenotype
{
    public class RBSF_Deal_With_the_Devil : CompAbilityEffect_Resurrect
    {

        public new RBSF_CompProperties_Deal_With_the_Devil Props
        {
            get
            {
                return (RBSF_CompProperties_Deal_With_the_Devil)this.props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            //Log.Message("Target was " + target.ToString());
            var random = Rand.Value;
            //Log.Message("Random value was " + random.ToString());
            if (random >= Props.chance)
            {
                base.Apply(target, dest);
                //Log.Message("Success!");
                RBSF_DefOf.RBSF_MelodicElegySucceed.PlayOneShot(new TargetInfo(this.parent.pawn.Position, this.parent.pawn.Map, false));
                return;
            }

            target.Thing.Destroy();
            //Log.Message("Failure...");
            RBSF_DefOf.RBSF_MelodicElegyFail.PlayOneShot(new TargetInfo(this.parent.pawn.Position, this.parent.pawn.Map, false));

            return;
        }
    }

    public class RBSF_CompProperties_Deal_With_the_Devil : CompProperties_AbilityEffect
    {
        public RBSF_CompProperties_Deal_With_the_Devil()
        {
            this.compClass = typeof(RBSF_Deal_With_the_Devil);
        }
        public float chance = 0.5f;
    }
}
