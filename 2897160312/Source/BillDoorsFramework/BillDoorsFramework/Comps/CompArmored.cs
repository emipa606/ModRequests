using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class CompArmored : ThingComp
    {
        float damageReduction;

        public CompProperties_Armored Props => (CompProperties_Armored)props;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            CompProperties_Armored Props = props as CompProperties_Armored;
            damageReduction = Props.damageReduction;
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            dinfo.SetAmount(dinfo.Amount - Props.damageReduction);
            if (dinfo.Amount <= 0)
            {
                absorbed = true;
            }
            else
            {
                base.PostPreApplyDamage(ref dinfo, out absorbed);
            }
        }

        private static void ApplyArmor(ref float damAmount, float armorPenetration, float armorRating)
        {
            float num = Mathf.Max(armorRating - armorPenetration, 0f);
            float value = Rand.Value;
            float num2 = num * 0.5f;
            float num3 = num;
            if (value < num2)
            {
                damAmount = 0f;
            }
            else if (value < num3)
            {
                damAmount = GenMath.RoundRandom(damAmount / 2f);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            return base.SpecialDisplayStats();
        }
    }

    public class CompProperties_Armored : CompProperties
    {
        public CompProperties_Armored()
        {
            compClass = typeof(CompArmored);
        }

        public float damageReduction = 0;
    }
}
