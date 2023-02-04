using UnityEngine;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;
using System.Linq;


namespace customYear
{

    public class modBase : ModBase
    {
        public override string ModIdentifier => "customYear";

        private SettingHandle<int> startYear_s;
        public static int startYear = 1;

        private SettingHandle<int> unitYear_s;
        public static int unitYear = 1;

        private SettingHandle<int> humanAging_s;
        public static int humanAging = 1;

        private SettingHandle<int> lifeExpectancy_s;
        public static int lifeExpectancy = 1;

        private SettingHandle<int> animalAging_s;
        public static int animalAging = 1;


        private SettingHandle<bool> worldPawn_s;
        public static bool worldPawn = false;


        // -----------------------------------------

        public override void DefsLoaded()
        {
            startYear_s = Settings.GetHandle<int>("startYear", "startYear".Translate(), "", 2100);
            unitYear_s = Settings.GetHandle<int>("unitYear", "unitYear".Translate(), "", 10);

            humanAging_s = Settings.GetHandle<int>("humanAging", "humanAging".Translate(), "", 10);
            animalAging_s = Settings.GetHandle<int>("animalAging", "animalAging".Translate(), "", 3);

            lifeExpectancy_s = Settings.GetHandle<int>("humanLifeExpectancy", "humanLifeExpectancy".Translate(), "", 80);

            worldPawn_s = Settings.GetHandle<bool>("worldPawn", "worldPawn".Translate(), "", false);

            SettingsChanged();
        }

        public override void SettingsChanged()
        {

            startYear = startYear_s.Value;
            unitYear = unitYear_s.Value;
            humanAging = Mathf.Clamp(humanAging_s.Value, 1, 10000);
            lifeExpectancy = lifeExpectancy_s.Value;

            animalAging = Mathf.Clamp(animalAging_s.Value, 1, 10000);

            worldPawn = worldPawn_s.Value;

            patchDef();
        }

        public static void patchDef()
        {
            /*
            foreach (ThingDef t in from thing in DefDatabase<ThingDef>.AllDefs
                                     where
                                    thing.defName == "Human"
                                   select thing)
            {
                t.race.lifeExpectancy =
            }
            */
            ThingDefOf.Human.race.lifeExpectancy = lifeExpectancy;


        }

        
        public override void Tick(int currentTick)
        {
            foreach(Pawn p in Find.WorldPawns.AllPawnsAlive)
            {
                if (p.IsColonist)
                {
                    customAging(p);
                }
            }
            foreach (Map m in Find.Maps)
            {
                foreach (Pawn p in m.mapPawns.PawnsInFaction(Faction.OfPlayer))
                {
                    customAging(p);
                }
            }

            // world pawn
            if (worldPawn)
            {
                int checkTick = GenDate.TicksPerDay * 5;
                bool flag = Find.TickManager.TicksAbs % checkTick == 0;
                if (flag)
                {

                    RimWorld.Planet.WorldPawns worldPawns = Find.WorldPawns;
                    Pawn[] ar_pawn = worldPawns.AllPawnsAlive.ToArray();

                    foreach (Pawn p in ar_pawn)
                    {
                        if (!p.IsColonist)
                        {
                            int multiplier = 0;

                            if (p.RaceProps.Humanlike)
                            {
                                multiplier = modBase.humanAging;
                            }
                            else
                            {
                                multiplier = modBase.animalAging;
                            }

                            int totalTick = checkTick * multiplier;

                            p.ageTracker.AgeTickMothballed(totalTick);
                        }
                        

                    }

                }
            }



        }

        void customAging(Pawn p)
        {
            int multiplier = 1;



            if (p.RaceProps.Humanlike)
            {
                multiplier = modBase.humanAging;
            }
            else
            {
                multiplier = modBase.animalAging;
            }

            int additionalTick = multiplier - 1;

            if (additionalTick <= 0) return;

            //__instance.ageTracker.AgeTickMothballed(additionalTick);
            p.TickMothballed(additionalTick);

            //Log.Message($"--- {Find.TickManager.TicksAbs.ToString()}");

            //Log.Message(p.ageTracker.AgeBiologicalTicks.ToString());
        }




    }











}