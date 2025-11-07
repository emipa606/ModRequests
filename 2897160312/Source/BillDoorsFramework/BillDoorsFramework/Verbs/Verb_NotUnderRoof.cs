using RimWorld;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    public class Verb_ShootNotUnderRoof : Verb_Shoot
    {
        ModExtension_VerbNotUnderRoof extension => EquipmentSource.def.GetModExtension<ModExtension_VerbNotUnderRoof>();

        CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();
        public override bool Available()
        {
            if (Caster.Position.Roofed(Caster.Map)
                && (compSecondaryVerb == null || extension == null || (compSecondaryVerb.IsSecondaryVerbSelected && extension.appliesInSecondaryMode) || (!compSecondaryVerb.IsSecondaryVerbSelected && extension.appliesInPrimaryMode)))
            {
                return false;
            }
            return base.Available();
        }
    }

    public class Verb_ShootNotUnderRoofOneUse : Verb_ShootOneUse
    {
        ModExtension_VerbNotUnderRoof extension => EquipmentSource.def.GetModExtension<ModExtension_VerbNotUnderRoof>();

        CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();
        public override bool Available()
        {
            if (Caster.Position.Roofed(Caster.Map)
                && (compSecondaryVerb == null || extension == null || (compSecondaryVerb.IsSecondaryVerbSelected && extension.appliesInSecondaryMode) || (!compSecondaryVerb.IsSecondaryVerbSelected && extension.appliesInPrimaryMode)))
            {
                return false;
            }
            return base.Available();
        }

        protected override bool TryCastShot()
        {
            bool b = base.TryCastShot();
            if (extension != null && CasterIsPawn && !CasterPawn.IsPlayerControlled)
            {
                bool proceed = true;
                if (extension.generateSidearm)
                {
                    PawnKindModExtension_Sidearm extSidearm = CasterPawn.kindDef?.GetModExtension<PawnKindModExtension_Sidearm>();
                    if (extSidearm != null)
                    {
                        Thing sidearm = ThingMaker.MakeThing(extSidearm.weaponDef, extSidearm.weaponDef.defaultStuff);
                        if (sidearm.TryGetComp<CompQuality>() is CompQuality q)
                        {
                            q.SetQuality(QualityCategory.Normal, null);
                        }

                        CasterPawn.equipment.AddEquipment((ThingWithComps)sidearm);
                        proceed = false;
                    }
                }
                if (proceed && extension.oneUseTryFindNewWeapon)
                {
                    Thing thing = GenClosest.ClosestThingReachable(CasterPawn.Position, CasterPawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon), PathEndMode.OnCell, TraverseParms.For(CasterPawn), 8f, (Thing x) => CasterPawn.CanReserve(x) && !x.IsBurning() && !(x.def.IsRangedWeapon && CasterPawn.WorkTagIsDisabled(WorkTags.Shooting)), null, 0, 15);
                    if (thing != null)
                    {
                        CasterPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RimWorld.JobDefOf.Equip, thing));
                    }
                }
            }
            return b;
        }
    }

    public class ModExtension_VerbNotUnderRoof : DefModExtension
    {
        public bool appliesInPrimaryMode = true;
        public bool appliesInSecondaryMode = true;

        public bool oneUseTryFindNewWeapon;
        public bool generateSidearm;
    }
}
