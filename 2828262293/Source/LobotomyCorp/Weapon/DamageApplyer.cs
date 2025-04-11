using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace LobotomyCorp.Weapon
{

    [StaticConstructorOnStartup]
    public static class DamageApplyer
    {

        public static Rot4 RotationOf(this Thing caster, Thing target)
        {
            return Rot4.FromAngleFlat((target.Position - caster.Position).AngleFlat);
        }

        public static void TakeDamage(this Verb_LCSPecialAttack verb){
            /*Log.Message("Affect:"+verb.verbProperties.aoeType.AffectCells.Count);
            Rot4 rot = Rot4.FromAngleFlat(
                (verb.CurrentTarget.Thing.Position - verb.CasterPawn.Position).AngleFlat
                );
            */

            Rot4 rot = RotationOf(verb.CasterPawn, verb.CurrentTarget.Thing);

            CacheThingLooper.Loop(
                //verb.verbProperties.aoeType.Rotate(rot).Select((x)=>x + verb.CurrentTarget.Cell),
                verb.verbProperties.aoeType.AffectCellsOf(verb.CurrentTarget.Cell, verb.Caster.RotationOf(verb.CurrentTarget.Thing)),
                verb.Caster.Map,
                (Thing thing)=>{
                if (thing is Pawn)
                {
                    if ((!Setting.LCModSetting.canDamageColonist ||
                         !verb.verbProperties.affectColonist) &&
                        ((Pawn)thing).IsColonist) return;
                    if (thing == verb.Caster) return;
                }
                if (thing == verb.CurrentTarget.Thing) return;

                for (int c = 0; c < verb.verbProperties.hitCount; c++)
                {
                    if (thing == null) continue;
                    if (thing.Destroyed) continue;
                    DamageWorker.DamageResult res = thing.TakeDamage(DamageInfo(verb, thing));
                }
            });

            /*
            //thingID
            HashSet<string> cachedThing = new HashSet<string>();
            foreach (IntVec3 vec in verb.verbProperties.aoeType.Rotate(rot))
            {
                List<Thing> things = new List<Thing>(verb.CasterPawn.Map
                    .thingGrid.ThingsListAtFast(vec + verb.CurrentTarget.Cell));
                for(int i = 0; i < things.Count; i++)
                {
                    if (!Setting.LCModSetting.canDamageSameThingsInDifferentCell)
                    {
                        if (cachedThing.Contains(things[i].ThingID)) continue;
                        else cachedThing.Add(things[i].ThingID);
                    }

                    if (things[i] is Pawn) {
                        if ( ( !Setting.LCModSetting.canDamageColonist ||
                             !verb.verbProperties.affectColonist  )&&
                            ((Pawn)things[i]).IsColonist) continue;
                        if (things[i] == verb.Caster) continue;
                    }
                    if (things[i] == verb.CurrentTarget.Thing) continue;

                    for (int c = 0; c < verb.verbProperties.hitCount; c++){
                        if (things[i] == null) continue;
                        if (things[i].Destroyed) continue;
                        DamageWorker.DamageResult res = things[i].TakeDamage(DamageInfo(verb, things[i]));
                    }

                }
                things = null;
            }
            */
        }

        private static float RandDamount(float num)
        {
            return Rand.Range(num * 0.9f, num * 1.1f);
        }

        private static float MeleeDamage(Verb_LCSPecialAttack verb)
        {
            float f = verb.verbProperties.AdjustedMeleeDamageAmount(verb, verb.CasterPawn);
            f /= verb.verbProperties.hitCount;
            
            return f;
        }

        private static DamageInfo DamageInfo(Verb_LCSPecialAttack verb, Thing victim)
        {
            VerbProperties_LCSpecialAttack vp = verb.verbProperties;
            DamageDef def = Setting.LCModSetting.isLCDamageAvailable ?
                vp.meleeDamageDef : (vp.noLCDamage?? DamageDefOf.Blunt);

            float amount = RandDamount(MeleeDamage(verb));
            float ap = vp.AdjustedArmorPenetration(verb, verb.CasterPawn);

            ThingDef source = verb.EquipmentSource != null ?
                verb.EquipmentSource.def : verb.CasterPawn.def;

            BodyPartGroupDef part = null;
            HediffDef buff = null;

            if (verb.CasterIsPawn)
            {
                part = verb.verbProps.AdjustedLinkedBodyPartsGroup(verb.tool);
                if (amount >= 1f)
                {
                    if (verb.HediffCompSource != null) buff = verb.HediffCompSource.Def;

                }
                else amount = 1f;
            }

            DamageInfo info = new DamageInfo(def, amount, ap, -1, verb.CasterPawn, null, source);
            info.SetWeaponBodyPartGroup(part);
            info.SetWeaponHediff(buff);
            info.SetAngle((victim.Position - verb.CasterPawn.Position).ToVector3());
            info.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            return info;
        }

        public static IEnumerable<DamageInfo> DamageInfoIEnumerable(Verb_LCSPecialAttack verb, Thing victim)
        {
            VerbProperties_LCSpecialAttack vp = verb.verbProperties;

            for (int i = 0; i < vp.hitCount; i++)
            {
                yield return DamageInfo(verb, victim);
            }
        }


        public static class CacheThingLooper{
            private static HashSet<string> cachedThing = new HashSet<string>();

            public static void Loop(IEnumerable<IntVec3> affectCells, Map map, Action<Thing> damageFunc)
            {
                cachedThing = new HashSet<string>();

                foreach(IntVec3 vec in affectCells)
                {
                    if (!vec.InBounds(map)) continue;
                    List<Thing> things = new List<Thing>(vec.GetThingList(map));
                    for (int i = 0; i < things.Count; i++)
                    {
                        if (!Setting.LCModSetting.canDamageSameThingsInDifferentCell)
                        {
                            if (cachedThing.Contains(things[i].ThingID)) continue;
                            else cachedThing.Add(things[i].ThingID);
                        }
                        damageFunc.Invoke(things[i]);

                    }
                }

            }

        }

    }
}
