using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using RimWorld.Planet;
using Random = System.Random;

namespace RimCrypto
{
    [StaticConstructorOnStartup]
    public static class RimCryptoInit
    {
        static RimCryptoInit()
        {
            HarmonyLib.Harmony harmony = new Harmony("jechto.rimcrypto.1");
            harmony.PatchAll();
            Log.Message("RimCrypto Loaded");
        }
    }

    public class RimCryptoBtc : ThingWithComps
    {
        private const int Cooldown = 2500;
        public static float Price = 2.5f;
        public static float TrendPrice = 2.5f;

        private int cd = 0;

        public RimCryptoBtc()
        {
            cd = 10;
        }

        public override void Tick()
        {
            base.Tick();
            if (cd < 0)
            {
                ChangePrice();
                cd = Cooldown;
            }

            cd--;
        }

        public void ChangePrice()
        {
            List<StatModifier> myThingStatBases = this.def.statBases;
            StatModifier marketValue = myThingStatBases.Find((StatModifier mod) => mod.stat == StatDefOf.MarketValue);
            marketValue.value = Price;
            //Log.Message(String.Format("BTC price set to to {0}", Price));
            this.def.description =
                String.Format(
                    "Milli Bitcoins (mBTC) denotes 1/1000th of a whole Bitcoin, which can be traded and exchanged for goods.\nOne bitcoin is worth {0}$",
                    Math.Floor(Price * 1000));
        }
    }

    public class RimCryptoMiner : Building
    {
        private const int Cooldown = 250;
        public static float BlockEstReward = 1000f;

        public static float CurBlockReward = 37.5f;

        public int cd = 0;

        public RimCryptoMiner()
        {
            cd = 0;
        }

        public override void Tick()
        {
            base.Tick();
            if (cd < 0)
            {
                SetBlockReward();
                cd = Cooldown;
            }

            cd--;
        }

        public void SetBlockReward()
        {
            CurBlockReward = (BlockEstReward / RimCryptoBtc.Price);

            CompProperties Comp = GetComp<CompSpawner>().props;
            CompProperties_Spawner SpawnerComp = (CompProperties_Spawner) Comp;
            SpawnerComp.spawnCount = (int) Math.Ceiling(CurBlockReward);
           //Log.Message(String.Format("Setting Spawn Count to {0}", CurBlockReward));
        }
    }
    
    public class BTCPriceCollecter : HistoryAutoRecorderWorker
    {
        public override float PullRecord()
        {
            return RimCryptoBtc.Price*1000;
        }
    }


    [HarmonyPatch(typeof(TickManager), "DoSingleTick")]
    public class TickManager_DoSingleTick
    {
        private static Random rng = new Random();
        private static int Cooldown = 5000;
        private static int cd;

        [HarmonyPostfix]
        public static void PostFix()
        {
            cd--;
            if (cd > 0)
            {
                return;
            }

            cd = Cooldown;

            // Do Gaussian Random
            double u1 = 1d - rng.NextDouble();
            double u2 = 1d - rng.NextDouble();
            float randStdNormal = (float) (Math.Sqrt(-2d * Math.Log(u1)) * Math.Sin(2d * Math.PI * u2));
            
            if (randStdNormal > 0)
            {
                RimCryptoBtc.Price *= (Math.Abs(randStdNormal) / 10 + 1);
            }
            else if (randStdNormal < 0)
            {
                RimCryptoBtc.Price /= (Math.Abs(randStdNormal) / 10 + 1);
            }
            if (RimCryptoBtc.Price < 0.25f)
            {
                RimCryptoBtc.Price = 0.25f;
            }

            // Add nudge based on over/undervaluation
            float valuationDiff = RimCryptoBtc.Price - RimCryptoBtc.TrendPrice;
            RimCryptoBtc.Price -= valuationDiff/50f;
            
            RimCryptoBtc.TrendPrice += (2.5f/1440f)*2f;
            
            //Log.Message(String.Format("Changed Price To {0}, Price change was {1} std", RimCryptoBtc.Price, randStdNormal));
        }
    }

    [HarmonyPatch(typeof(Tradeable), "InitPriceDataIfNeeded")]
    internal class Patch_Tradeable_InitPriceDataIfNeeded
    {
        [HarmonyPostfix]
        static void Postfix(Tradeable __instance)
        {
            if ( __instance.ThingDef != null && __instance.ThingDef.tradeTags != null && __instance.ThingDef.tradeTags.Contains("CryptoCoin"))
            {
                Traverse t = Traverse.Create(__instance);
                t.Field("pricePlayerBuy").SetValue(RimCryptoBtc.Price);
                t.Field("pricePlayerSell").SetValue(RimCryptoBtc.Price * 0.9f);
            }
        }
    }
}