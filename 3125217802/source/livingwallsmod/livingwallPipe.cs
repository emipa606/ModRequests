using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verse;
using RimWorld;
using UnityEngine;
using PipeSystem;

namespace valkyrim.livingwalls
{

    class LivingwallPipe : Building
    {
        public complivingwalltilePipe PipeWallComp;
        public CompResource resourceComp;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            PipeWallComp = (complivingwalltilePipe)GetComp<CompRefuelable>();
            resourceComp = GetComp<CompResource>();
        }

    public override void Tick()
        {

            base.Tick();
            complivingwalltilePipe Pipe;
            Pipe = (complivingwalltilePipe)GetComp<CompRefuelable>();
            //Log.Message("Pipe" + Pipe.livingwallfueled);
            if (Pipe.livingwallfueled)
            {
                //Log.Message("goodfuel");
                HealLivingWall(Pipe.Tier);
            }
            else
            {
                if (LoadedModManager.GetMod<LivingwallMod>().GetSettings<LivingWallSettings>().doLivingWallsDecay)
                {
                    DamageLivingWall(Pipe.Tier);
                }
            }
        }
        int healtick = 0;
        int damagetick = 0;
        void DamageLivingWall(float tier)
        {
            if (HitPoints > (int)Math.Ceiling(MaxHitPoints * 0.1))
            {
                if (damagetick < 250)
                {
                    damagetick = damagetick + 1;
                }
                if (damagetick >= 250)
                {
                    damagetick = 0;
                    HitPoints -= (int)Math.Ceiling(MaxHitPoints * 0.01 * tier);
                    if (HitPoints < (int)Math.Ceiling(MaxHitPoints * 0.1))
                        HitPoints = (int)Math.Ceiling(MaxHitPoints * 0.1);
                }
            }
        }
             void HealLivingWall(float tier)
        {

            if (HitPoints < MaxHitPoints)
            {
                if (healtick < 250)
                {
                    healtick = healtick + 1;
                }
                if (healtick >= 250)
                {
                    healtick = 0;
                    HitPoints += (int)Math.Ceiling(MaxHitPoints * 0.01 * tier);
                    if (HitPoints > MaxHitPoints)
                        HitPoints = MaxHitPoints;
                }
            }
        }
    }
    public class CompPropertieslivingwalltilePipe : CompProperties_Refuelable
    {
        public float tier;
        public CompPropertieslivingwalltilePipe() => compClass = typeof(complivingwalltilePipe);
    }
   public class complivingwalltilePipe : CompRefuelable
    {

        public CompResourceTrader compResourcebase;
        public new CompPropertieslivingwalltilePipe Props => (CompPropertieslivingwalltilePipe)props;
        public override void Initialize(CompProperties props)
        {
            //Log.Message("compexists");
            base.Initialize(props);
            compResourcebase = parent.GetComp<CompResourceTrader>();
        }
        public float Tier => Props.tier;
        public bool livingwallfueled;
        private float ConsumptionRatePerTick => Props.fuelConsumptionRate / 6000f;
        public override void CompTick()
        {
            base.CompTick();
                {
                //Log.Message("comptick");
                if (Fuel > 0f)
                {
                    //  Log.Message("goodfuel");
                    livingwallfueled = true;
                    //Log.Message("Pipe"+livingwallfueled);
                    ConsumeFuel(ConsumptionRatePerTick);
                }
                else if (Fuel <= 0f)
                {
                    livingwallfueled = false;
                    // Log.Message("nofuel");
                }
            }
        }
    }
}

