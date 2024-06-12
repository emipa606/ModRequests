using RimWorld;
using System.Linq;
using Verse;

namespace NightVision
{
    public class SolarRaid_StoryWorker
    {
        public static int MinTicksLeftToFireInc = 2500;
        public static int MinTicksPassedToFire = 2500;
        public static IncidentDef FlareRaidDef = IncidentDef.Named("FlareRaid");
        public int GlobalLastFireTick = -1;
        public int LastFlareStartTick = -1;
        public int QueuedFiringTick = -1;
        public int QueuedMapID = -1;
        public static readonly double MaxSunGlowForRaid = 0.2;

        public SolarRaid_StoryWorker() { }

        public void TickCheckForFlareRaid()
        {

            if (!NVGameComponent.FlareRaidIsEnabled
                || Find.Scenario.AllParts.Any(
                    scenPart => scenPart is ScenPart_DisableIncident scenPartDisable && scenPartDisable.Incident == FlareRaidDef
                ))
            {
                return;
            }

            if (QueuedMapID > 0 && QueuedFiringTick <= Find.TickManager.TicksAbs)
            {
                if (!Find.World.GameConditionManager.ConditionIsActive(Defs_Rimworld.SolarFlare))
                {
                    Log.Message($"NVGameComp: found queued map but solar flare no longer active. Miscalculated?");
                    QueuedFiringTick = -1;
                    QueuedMapID = -1;
                    return;
                }

                if (QueuedFiringTick < 0)
                {
                    Log.Message($"NVGameComp: had queued map index, and solar flare is active but queued tick was -1. Error?");
                    QueuedFiringTick = -1;
                    QueuedMapID = -1;
                    return;
                }

                Map map = Find.Maps.Find(mp => mp.uniqueID == QueuedMapID);

                QueuedFiringTick = -1;
                QueuedMapID = -1;

                if (map == null)
                {
                    Log.Message($"NVGameComp: MapID and Tick were both valid but map was not found. Map destroyed?");
                    return;
                }

                TryFireFlareRaid(map);
            }
            else if (QueuedFiringTick > 0 && QueuedMapID < 0)
            {
                Log.Message($"NVGameComp: found queued firing tick but queued mapID was -1.");
                QueuedFiringTick = -1;
                QueuedMapID = -1;
            }

            if (Find.World.GameConditionManager.GetActiveCondition(def: Defs_Rimworld.SolarFlare) is GameCondition solarFlare
                && !solarFlare.Permanent && solarFlare.startTick != LastFlareStartTick)
            {
                LastFlareStartTick = solarFlare.startTick;

                if (solarFlare.TicksLeft > MinTicksLeftToFireInc)
                {

                    var difficultyDef = Find.Storyteller.difficulty;

                    if (!difficultyDef.allowBigThreats)
                    {
                        return;
                    }


                    var potentialTargets = Find.Maps.FindAll(map => map.IsPlayerHome);

                    if (potentialTargets.Count == 0)
                    {
                        return;
                    }

                    int hourCount = 0;
                    // use tolist to force eval
                    var anony = potentialTargets.Select(target => new { mapID = target.uniqueID, hours = CalcPotentialHoursToFire(target, solarFlare.TicksLeft, ref hourCount) }).Where(anon => anon.hours != null).ToList();


                    if (
                        hourCount == 0)
                    {
                        return;
                    }

                    int ticksTillFireInHourTerms = -1250 + Rand.Range(0, hourCount * 2500);

                    int firingTick = -1;
                    int mapId = -1;

                    foreach (var mapHours in anony)
                    {
                        foreach (int hour in mapHours.hours)
                        {
                            if (ticksTillFireInHourTerms < 1250)
                            {
                                firingTick = (hour * 2500) + ticksTillFireInHourTerms;

                                break;
                            }
                            else
                            {
                                ticksTillFireInHourTerms -= 2500;
                            }
                        }

                        if (firingTick > 0)
                        {
                            mapId = mapHours.mapID;

                            break;
                        }
                    }

                    if (firingTick < 0 || mapId < 0)
                    {

                        Log.Message(new string('-', 20));
                        Log.Message($"NVGameComponent");
                        Log.Message($"TickCheckForFlareRaid");
                        Log.Message($"No tick or map index found when there should be one.");
                        Log.Message(new string('-', 20));

                        return;
                    }



                    QueuedFiringTick = firingTick + Find.TickManager.TicksAbs;
                    QueuedMapID = mapId;
                }
            }

        }

        public void TryFireFlareRaid(Map map)
        {
            //int ticksGame = Find.TickManager.TicksGame;

            //if (GlobalLastFireTick > 0 && (ticksGame - GlobalLastFireTick) / 60000f < FlareRaidDef.minRefireDays)
            //{
            //    return false;
            //}
            if (!Rand.Chance(SolarRaid_IncidentWorker.ChanceForFlareRaid(map)))
            {
                return;
            }

            IncidentParms newParms = StorytellerUtility.DefaultParmsNow(FlareRaidDef.category, map);


            if (!FlareRaidDef.Worker.CanFireNow(newParms))
            {
                return;
            }

            Find.Storyteller.TryFire(new FiringIncident(FlareRaidDef, null, newParms));
        }

        public int[] CalcPotentialHoursToFire(Map map, int flareTicks, ref int hourCount)
        {
            int currentTick = Find.TickManager.TicksAbs;

            int numHoursFromNow = (flareTicks / 2500);

            if (numHoursFromNow == 0)
            {
                return null;
            }

            hourCount += numHoursFromNow;
            var result = new int[numHoursFromNow];

            for (int hoursFromNow = 1; hoursFromNow <= numHoursFromNow; hoursFromNow++)
            {
                if (GenCelestial.CelestialSunGlow(map, currentTick + (2500 * hoursFromNow)) < MaxSunGlowForRaid)
                {
                    result[hoursFromNow - 1] = hoursFromNow;
                }
            }

            return result;


        }

        public void ExposeData()
        {

            Scribe_Values.Look(ref this.LastFlareStartTick, "lastFlareStartTick", -1);
            Scribe_Values.Look(ref this.QueuedFiringTick, "queuedFiringTick", -1);
            Scribe_Values.Look(ref this.QueuedMapID, "queuedMapID", -1);
        }
    }
}