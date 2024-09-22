using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace System.RandomFactions
{
    /*
    Harmony patch example:

    NOTE: arguments prefixed with __... are keywords in Harmony and cannot be renamed!
    All other args much have the same name as in the original code ytou are patching 
    (unlike the Java patchers which do not know/care about the original variable names)
    see https://harmony.pardeike.net/articles/patching-injections.html

    public class OriginalCode
    {
        public string GetName(int i)
        {
            return name; // ...
        }
    }

    [HarmonyPatch(typeof(OriginalCode), nameof(OriginalCode.GetName))]
    class Patch
    {
        static bool Prefix(ref string __result, int i)
        {
            __result = "test";
            return true; // make sure you only skip if really necessary
        }

        static void Postfix(OriginalCode __instance, ref string __result, int i)
        {
            if (__result == "foo")
                __result = "bar";
        }
    }
     */


    //[HarmonyPatch(typeof(RimWorld.Page_CreateWorldParams), "Reset")]
    [HarmonyPatch(typeof(RimWorld.Page_CreateWorldParams), "DoWindowContents")]
    public class NewGameUIPatch
    {
        // planet global temp settings enum: RimWorld.Planet.OverallTemperature
        // planet global rain settings enum: RimWorld.Planet.OverallRainfall
        // planet global pop. settings enum: RimWorld.Planet.OverallPopulation
        // planet global pol. settings variable: RimWorld.Page_CreateWorldParams.pollution

        static string lastSeed = string.Empty;
        static bool Prefix(Rect rect)
        {
            // return false to cancel event
            return true;
        }

        static void Postfix(RimWorld.Page_CreateWorldParams __instance, Rect rect)
        {
            string newSeed = getByReflection<string>(__instance, "seedString");
            if (!lastSeed.Equals(newSeed))
            {
                lastSeed = newSeed;
                randomizePlanetSettings(__instance, newSeed);
            }
        }

        private static void randomizePlanetSettings(Page_CreateWorldParams createWorldPage, string newSeed)
        {
            System.Random prng = prngFromSeed(newSeed);
            var rainfallArray = new OverallRainfall[]
            {
                OverallRainfall.AlmostNone, OverallRainfall.Little, OverallRainfall.LittleBitLess,
                OverallRainfall.Normal,
                OverallRainfall.LittleBitMore, OverallRainfall.High, OverallRainfall.VeryHigh
            };
            var tempArray = new OverallTemperature[]
            {
                OverallTemperature.VeryCold, OverallTemperature.Cold, OverallTemperature.LittleBitColder,
                OverallTemperature.Normal,
                OverallTemperature.LittleBitWarmer, OverallTemperature.Hot, OverallTemperature.VeryHot
            };
            var popDensityArray = new OverallPopulation[]
            {
                OverallPopulation.AlmostNone, OverallPopulation.Little, OverallPopulation.LittleBitLess,
                OverallPopulation.Normal,
                OverallPopulation.LittleBitMore, OverallPopulation.High, OverallPopulation.VeryHigh
            };
            setByReflection(createWorldPage, "rainfall", rainfallArray[rollDice(2, 4, -2, prng)]);
            setByReflection(createWorldPage, "temperature", tempArray[rollDice(2, 4, -2, prng)]);
            setByReflection(createWorldPage, "population", popDensityArray[rollDice(2, 4, -2, prng)]);
            if (ModsConfig.BiotechActive)
            {
                // only mess with pollution if Biotech DLC is active (default is 0.05f)
                // roll 8d6-28 and take abs value for a nice gaussian
                setByReflection(createWorldPage, "pollution", (float)(0.05f * Math.Abs(rollDice(8, 6, -28, prng))));
            }
        }

        private static int rollDice(int n, int d, int mod, System.Random prng)
        {
            int total = mod;
            for(int i = 0; i < n; i++)
            {
                total += prng.Next(d)+1;
            }
            return total;
        }

        private static System.Random prngFromSeed(string s)
        {
            // ensure sufficient entropy in rangom number generator (this is overkill :P )
            int seederSeed  = s.GetHashCode() * 7727 + 2999;
            System.Random seeder = new System.Random(seederSeed);
            byte[] seed_buffer = new byte[4];
            seeder.NextBytes(seed_buffer);
            int seed = BitConverter.ToInt32(seed_buffer, 0);
            var seeder2 = new System.Random(seed);
            byte[] seed_buffer2 = new byte[4];
            seeder2.NextBytes(seed_buffer2);
            int seed2 = BitConverter.ToInt32(seed_buffer2, 0);
            var prng = new System.Random(seed2);
            return prng;
        }

        private static T getByReflection<T>(object obj, string varName)
        {
            var fieldHandle = obj.GetType().GetField(varName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fieldHandle == null) throw new FieldAccessException(string.Format("Exception in System.RandomFactions.NewGameUIPatch.getByReflection({1}, {0}): Field {0} does not exist in class {1}", varName, obj.GetType().Name));

            return (T) fieldHandle.GetValue(obj);
        }
        private static void setByReflection(object obj, string varName, object value)
        {
            var fieldHandle = obj.GetType().GetField(varName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fieldHandle == null) throw new FieldAccessException(string.Format("Exception in System.RandomFactions.NewGameUIPatch.setByReflection({1}, {0}, {2}): Field {0} does not exist in class {1}", varName, obj.GetType().Name, value));

            fieldHandle.SetValue(obj, value);
        }
    }
}
