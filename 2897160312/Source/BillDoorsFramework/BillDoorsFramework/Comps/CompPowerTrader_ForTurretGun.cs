using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompPowerTrader_ForTurretGun : ThingComp
    {

        public CompProperties_PowerTurretGun Props
        {
            get
            {
                return (CompProperties_PowerTurretGun)this.props;
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            compPower = parent.GetComp<CompPowerTrader>();
            compMannable = parent.GetComp<CompMannable>();
            BurstCooldownTicksLeft = AccessTools.FieldRefAccess<int>(parent.GetType(), "burstCooldownTicksLeft");
        }

        protected AccessTools.FieldRef<object, int> BurstCooldownTicksLeft;

        public override void CompTick()
        {
            base.CompTick();
            if (BurstCooldownTicksLeft != null)
            {
                int tick = BurstCooldownTicksLeft(parent);
                if (tick > 0 && (compMannable == null || compMannable.MannedNow))
                {
                    compPower.PowerOutput = -1f * Props.rechargePowerConsumption * shotCount;
                }
                else
                {
                    compPower.PowerOutput = -1f * compPower.Props.PowerConsumption;
                    if (Find.TickManager.TicksGame > shotTime)
                    {
                        shotCount = 0;
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            float powerTotal = Props.rechargePowerConsumption * Props.cooldownTime * 0.001f;
            return "BD_RechargePower".Translate() + " : " + powerTotal.ToString("0.#");
        }
        public int shotCount = 0;
        public int shotTime = 0;
        private CompPowerTrader compPower;
        private CompMannable compMannable;
    }

    public class CompProperties_PowerTurretGun : CompProperties
    {
        public CompProperties_PowerTurretGun()
        {
            this.compClass = typeof(CompPowerTrader_ForTurretGun);
        }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry statDrawEntry in base.SpecialDisplayStats(req))
            {
                yield return statDrawEntry;
            }
            if (rechargePowerConsumption > 0f)
            {
                float powerTotal = rechargePowerConsumption * cooldownTime * 0.001f;
                yield return new StatDrawEntry(StatCategoryDefOf.Building, rechargePower, powerTotal.ToString("0.#") + " Wd", string.Format(rechargePower_Stat, rechargePowerConsumption.ToString("0.#"), cooldownTime.ToString("0.#"), powerTotal.ToString("0.#")), 5000, null, null, false);
            }
            yield break;
        }
        public float rechargePowerConsumption;
        public float cooldownTime;
        [MustTranslate]
        public string rechargePower = "";
        [MustTranslate]
        public string rechargePower_Stat = "{0},{1},{2}";
    }

    //obsolete
    public class Verb_ShootWithPower : Verb_Shoot
    {
        protected override bool TryCastShot()
        {
            bool result = base.TryCastShot();
            if (result == true)
            {
                if ((caster as ThingWithComps).GetComp<CompPowerTrader_ForTurretGun>() is CompPowerTrader_ForTurretGun comp)
                {
                    comp.shotCount += 1;
                    comp.shotTime = Find.TickManager.TicksGame + verbProps.ticksBetweenBurstShots + 1;
                }
            }
            return result;
        }
    }
}
