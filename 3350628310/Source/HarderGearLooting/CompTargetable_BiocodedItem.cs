using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HarderGearLooting
{

    public class CompTargetable_BiocodedItem : CompTargetable
    {
        protected override bool PlayerChoosesTarget => true;

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetItems = true,
                canTargetCorpses = false,
                mapObjectTargetsMustBeAutoAttackable = false
            };
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Thing is not ThingWithComps)
            {
                return false;
            }
            CompBiocodable comp = target.Thing.TryGetComp<CompBiocodable>();
            if (comp == null)
            {
                return false;
            }

            if (!comp.Biocoded)
            {
                return false;
            }

            return true;
        }
    }
}
