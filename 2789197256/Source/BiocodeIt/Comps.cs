using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using Verse.AI;

namespace Krelinos_BiocodeIt
{
    // Then next two classes used the Infused mod as a base and guide
    // https://steamcommunity.com/sharedfiles/filedetails/?id=731287727
    public class CompTargetable_Equipment : CompTargetable
    {
        //public new CompProperties_Equipment Props => (CompProperties_Equipment)this.props;

        protected override bool PlayerChoosesTarget => true;
        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            return base.CompFloatMenuOptions(selPawn);
        }

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetAnimals = false,
                canTargetHumans = false,
                canTargetMechs = false,
                mapObjectTargetsMustBeAutoAttackable = false,

                canTargetItems = true,
                mustBeSelectable = true,

                validator = EquipmentValidator
            };
        }

        bool EquipmentValidator(TargetInfo tInfo)
        {
            Thing targetedThing = tInfo.Thing;
            
            if (targetedThing == null) { return false; }
            if (!targetedThing.def.IsWithinCategory(ThingCategoryDefOf.Weapons) && !targetedThing.def.IsWithinCategory(ThingCategoryDefOf.Apparel) ) { return false; }

            //if ( targetedThing.def.IsMeleeWeapon
            //    || targetedThing.def.IsApparel
            //    || targetedThing.def.techLevel < TechLevel.Industrial
            //    || CompBiocodable.IsBiocoded(targetedThing) )
            //    return false;

//            if (!BiocodeIt_Settings.allowRanged && targetedThing.def.IsRangedWeapon) { return false; }
//            if (!BiocodeIt_Settings.allowMelee && targetedThing.def.IsMeleeWeapon) { return false; }
//            if (!BiocodeIt_Settings.allowApparel && targetedThing.def.IsApparel) { return false; }
//            if (!BiocodeIt_Settings.ignoreTechLevel && targetedThing.def.techLevel < TechLevel.Industrial) { return false; }
//            if (BiocodeIt_Settings.onlyNonBiocoded && CompBiocodable.IsBiocoded(targetedThing)) { return false; }

            return true;
        }
    }

    /*
        So this is now obsolete since I replaced comp properties with mod settings.
        Keeping it here as legacy code for those curious.
    
    public class CompProperties_Equipment : CompProperties_Targetable
    {
        public bool includeMeleeWeapons;        // These bools are okay to be left uninitialized.
        public bool includeRangedWeapons;       // When CompTargetable_Equipment is defined in an XML,
        public bool includeApparel;             // the relevant ones will be set to true.
        public bool industrialTierAndUpOnly;    // It will probably look something like this in some
        public bool spacerTierAndUpOnly;        // XML somewhere:
        public bool nonBiocodedEquipmentOnly;
    }                                           
//                                                <ThingDef>
//                                                    ...
//                                                    <comps>
//                                                        <li Class="Krelinos_BiocodeIt.CompProperties_Equipment">
//                                                            <compClass>Krelinos_BiocodeIt.CompTargetable_Equipment</compClass>
//                                                            <includeMeleeWeapons>true</includeMeleeWeapons>
//                                                            <spacerTierAndUpOnly>true</spacerTierAndUpOnly>
//                                                        </li>
//                                                    </comps>
//                                                </ThingDef>
                                                
    */

    public class CompTargetEffect_Biocode : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (user == null || target == null) { return; }
            if( !user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false)) { return; }

            Job job = JobMaker.MakeJob(BiocodeIt_JobDefOf.Biocode, target, this.parent);
            job.count = 1;
            user.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
        }
    }
}
