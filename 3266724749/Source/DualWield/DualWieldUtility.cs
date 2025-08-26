using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Tacticowl.Compatability.DualWield.LTSAmmo;
using Verse;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl.DualWield
{
    public static class DualWieldUtility
    {
        public static bool TryStartOffHandAttack(Pawn pawn, LocalTargetInfo targ, bool __result)
        {
            if (pawn.equipment == null || !pawn.GetOffHander(out ThingWithComps offHandEquip))
                return __result;


            var offHandStance = pawn.GetOffHandStance();
            if (offHandStance is Stance_Warmup_DW || offHandStance is Stance_Cooldown || pawn.WorkTagIsDisabled(WorkTags.Violent))
                return __result;

            //Support for JecsTools
            //TODO: look into making making this XML-exposed via mod extensions?
            if (Settings.usingJecsTools && pawn.CurJobDef != null && (pawn.CurJobDef.driverClass == ResourceBank.CastAbilitySelf.driverClass || pawn.CurJobDef.driverClass == ResourceBank.CastAbilityVerb.driverClass))
                return __result;

            if (TryGetOffHandAttackVerb(pawn, targ.Thing, out Verb verb, true))
            {
                // Support for Lts Ammo,
                if (Settings.usingLTSAmmo && !verb.IsMeleeAttack && verb.EquipmentSource != null && !LTSAmmo_Compatability.IsAvailable(pawn, verb.EquipmentSource))
                    return __result;
                
                return verb.TryStartCastOn(targ) || __result;
            }

            return __result;
        }

        static bool TryGetOffHandAttackVerb(Pawn instance, Thing target, out Verb verb, bool allowManualCastWeapons = false)
        {
            verb = null;
            if (instance.GetOffHander(out ThingWithComps offHandEquip))
            {
                CompEquippable compEquippable = offHandEquip.GetComp<CompEquippable>();
                if (compEquippable != null && compEquippable.PrimaryVerb.Available() && (!compEquippable.PrimaryVerb.verbProps.onlyManualCast || instance.CurJobDef != JobDefOf.Wait_Combat || allowManualCastWeapons))
                    verb = compEquippable.PrimaryVerb;
            }
            else TryGetMeleeVerbOffHand(instance, target, out verb);

            return verb != null;
        }

        public static bool TryGetMeleeVerbOffHand(Pawn instance, Thing target, out Verb verb)
        {
            verb = null;
            if (instance.GetOffHander(out ThingWithComps offHandEquip))
            {
                List<Verb> allVerbs = offHandEquip.GetComp<CompEquippable>()?.AllVerbs;
                if (allVerbs != null)
                {
                    List<Verb> usableVerbs = new List<Verb>();
                    float weight = 0;
                    for (int k = allVerbs.Count; k-- > 0;)
                    {
                        Verb v = allVerbs[k];
                        if (v.IsStillUsableBy(instance))
                        {
                            usableVerbs.Add(v);
                            var verbWeight = VerbUtility.InitialVerbWeight(v, instance);
                            if (verbWeight > weight)
                                weight = verbWeight;
                        }
                    }

                    if (!allVerbs.NullOrEmpty() && usableVerbs.Select(x => new VerbEntry(x, instance, usableVerbs, weight)).ToList().TryRandomElementByWeight(ve => ve.GetSelectionWeight(target), out var result))
                        verb = result.verb;
                }
            }

            return verb != null;
        }
    }
}
