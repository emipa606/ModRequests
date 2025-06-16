using System.Collections.Generic;
using Verse;

namespace SWSaber
{
    public static class Utility
    {
        private static bool modCheck;
        private static bool loadedForcePowers;
        private static bool loadedFactions;

        public static bool AreForcePowersLoaded()
        {
            if (!modCheck)
            {
                ModCheck();
            }

            return loadedForcePowers;
        }

        public static bool AreFactionsLoaded()
        {
            if (!modCheck)
            {
                ModCheck();
            }

            return loadedFactions;
        }

        private static void ModCheck()
        {
            Log.Message("Mod Check Called");
            loadedForcePowers = false;
            loadedFactions = false;
            foreach (var ResolvedMod in LoadedModManager.RunningMods)
            {
                if (loadedForcePowers && loadedFactions)
                {
                    break; //Save some loading
                }

                if (ResolvedMod.Name.Contains("Star Wars - The Force"))
                {
                    Log.Message("Lightsabers :: Star Wars - The Force Detected.");
                    loadedForcePowers = true;
                }

                if (!ResolvedMod.Name.Contains("Star Wars - Factions"))
                {
                    continue;
                }

                Log.Message("Lightsabers :: Star Wars - Factions Detected.");
                loadedFactions = true;
            }

            modCheck = true;
        }



        public static Thing GenerateCrystal(ThingDef crystalDef, float chance = 1.0f)
        {
            if (Rand.Value > chance)
            {
                return null;
            }

            if (crystalDef == null)
            {
                return null;
            }

            var thing = ThingMaker.MakeThing(crystalDef);
            thing.stackCount = 1;
            return thing;
        }

        public static Thing GenerateCrystalFor(Pawn p)
        {
            if (p.inventory == null)
            {
                return null;
            }

            if (p.inventory.innerContainer == null)
            {
                return null;
            }

            var legendaryCrystals = new List<ThingDef>
            {
                ThingDef.Named("PJ_UltimaPearl"),
                ThingDef.Named("PJ_BlackPearl"),
                ThingDef.Named("PJ_KaiburrCrystal"),
                ThingDef.Named("PJ_UltimaPearl"),
                ThingDef.Named("PJ_AnkSapphire")
            };
            var rareCrystals = new List<ThingDef>
            {
                ThingDef.Named("PJ_BarabIngot"),
                ThingDef.Named("PJ_PontiteCrystal"),
                ThingDef.Named("PJ_FirkrannCrystal"),
                ThingDef.Named("PJ_RubatCrystal"),
                ThingDef.Named("PJ_HurCrystal"),
                ThingDef.Named("PJ_DragiteCrystal"),
                ThingDef.Named("PJ_DamindCrystal"),
                ThingDef.Named("PJ_AdeganCrystal"),
                ThingDef.Named("PJ_EralamCrystal"),
                ThingDef.Named("PJ_PontiteCrystal")
            };
            var result = Utility.GenerateCrystal(legendaryCrystals.RandomElement(), 0.7f);
            if (result != null)
            {
                //Log.Message("5a");

                p.inventory.innerContainer.TryAdd(result);
                return result;
            }
            else
            {
                //Log.Message("5b");

                result = Utility.GenerateCrystal(rareCrystals.RandomElement());
                p.inventory.innerContainer.TryAdd(result);
                return result;
            }
        }

        public static void CrystalSlotter(CompCrystalSlotLoadable crystalSlot,
            CompLightsaberActivatableEffect lightsaberEffect, ThingWithComps crystal=null)
        {
            crystalSlot.Initialize();
            var randomCrystals = new List<string>
            {
                "PJ_KyberCrystal",
                "PJ_KyberCrystalBlue",
                "PJ_KyberCrystalCyan",
                "PJ_KyberCrystalAzure",
                "PJ_KyberCrystalRed",
                "PJ_KyberCrystalPurple"
            };
            if (crystal != null)
            {
                foreach (var slot in crystalSlot.Slots)
                {
                    slot.TryLoadSlot(crystal);
                }

            }
            else
            {
                var thingWithComps = (ThingWithComps)ThingMaker.MakeThing(ThingDef.Named(randomCrystals.RandomElement()));
                foreach (var slot in crystalSlot.Slots)
                {
                    slot.TryLoadSlot(thingWithComps);
                }
            }

            lightsaberEffect.TryActivate();
        }
    }
}