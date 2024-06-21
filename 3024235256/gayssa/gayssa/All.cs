using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace GayAss
{
    [DefOf]
    public static class Slurs
    {
        public static HediffDef Crippled;

        public static RecipeDef FixCripple;
    }

   [StaticConstructorOnStartup]
    public class Patcher
    {
        static Patcher()
        {
            //DamageDefOf.Blunt.workerClass = typeof(BetterBlunt);

            foreach (var def in DefDatabase<DamageDef>.AllDefs)
            {
                if (def.workerClass == typeof(Verse.DamageWorker_Blunt))
                {
                    def.workerClass = typeof(BetterBlunt);
                }
            }
            ThingDefOf.Human.recipes.Add(Slurs.FixCripple);
        }
    }

    public class BetterBlunt : DamageWorker_Blunt
    {
        BodyPartRecord record = null;

        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            DamageResult result = null;
            if (thing is Pawn p)
            {
                if (p.def == ThingDefOf.Human)
                {
                    record = ChooseHitPart(dinfo, p);

                    if (record != null)
                    {
                        if (record?.def == BodyPartDefOf.Arm || record?.def == BodyPartDefOf.Leg)
                        {
                            var hp = p.health.hediffSet.GetPartHealth(record);
                            bool flag = dinfo.Amount > hp;
                            if (flag)
                            {
                                dinfo.SetAmount(Math.Max(hp - 1f, 0f));
                                p.health.AddHediff(HediffMaker.MakeHediff(Slurs.Crippled, p, record));

                            }
                        }
                    }
                }
            }

            if (result == null)
            {
                result = base.Apply(dinfo, thing);
            }

            return result;
        }

        protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
        {
            if (record == null || (pawn?.health?.hediffSet?.HasHediff(Slurs.Crippled, record) ?? false))
            {
                record = base.ChooseHitPart(dinfo, pawn);
            }
            return record;
        }
    }
}