using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace Bombastic
{
    [DefOf]
    public class BaroDefOf
    {
        public static HediffDef BaroTrauma;
    }


    [StaticConstructorOnStartup]
    public class Patcher
    {
        static Patcher()
        {
            //DamageDefOf.Blunt.workerClass = typeof(BetterBlunt);
            DamageDefOf.Bomb.workerClass = typeof(BetterExplosion);
        }
    }

    public class DamageDefExtension : DefModExtension
    {
        public List<BodyPartDef> partsOnWhichToBuff;

        public float damageMult;
    }

    public class BetterExplosion : DamageWorker_Blunt
    {
        BodyPartRecord record = null;
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            if (thing is Pawn p)
            {
                if (p.def == ThingDefOf.Human)
                {
                    record = ChooseHitPart(dinfo, p);
                    var thingy = dinfo.Def.GetModExtension<DamageDefExtension>();
                    if (thingy.partsOnWhichToBuff.Contains(record.def))
                    {
                        dinfo.SetAmount(dinfo.Amount * thingy.damageMult);
                    }
                }

                if(record == null)
                {
                    record = ChooseHitPart(dinfo, p);
                }

                if (record.GetDirectChildParts().Any())
                {
                    foreach (var x in record.GetDirectChildParts().InRandomOrder())
                    {
                        float chance = 1f;
                        if (Rand.Chance(chance) && x.depth == BodyPartDepth.Inside)
                        {
                            //Log.Message(x.def.label);

                            var dinef = new DamageInfo(DamageDefOf.Crush, (dinfo.Amount / 2f), 0, -1, dinfo.Instigator, x, dinfo.Weapon);

                            dinef.Def = DamageDefOf.Crush;

                            //Log.Message((dinfo.Amount / 2f).ToString());

                            dinef.SetAmount(dinfo.Amount / 2f);

                            dinef.SetHitPart(x);

                            p.TakeDamage(dinef);

                            chance -= -0.15f;

                            if (Rand.Chance(0.5f))
                            {
                                foreach (var child in x.GetDirectChildParts().Where(xy => xy.def.delicate && xy.def.IsSolid(xy, p.health.hediffSet.hediffs.Where(xz => xz.Part == xy).ToList())))
                                {

                                    var dinef2 = new DamageInfo(DamageDefOf.Crush, (dinfo.Amount / 2f), 0, -1, dinfo.Instigator, x, dinfo.Weapon);

                                    dinef2.Def = DamageDefOf.Crush;

                                    ////Log.Message((dinfo.Amount / 2f).ToString());

                                    dinef2.SetAmount(dinfo.Amount / 4f);

                                    dinef2.SetHitPart(x);

                                    p.TakeDamage(dinef);
                                }
                            }
                        }

                    }
                }
            }
            return base.Apply(dinfo, thing);
        }

    }
}