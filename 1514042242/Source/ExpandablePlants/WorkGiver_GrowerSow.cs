using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace ExpandablePlants
{
    class WorkGiver_GrowerSow : WorkGiver_Grower
    {

        protected static string CantSowCavePlantBecauseOfLightTrans;
        protected static string CantSowCavePlantBecauseUnroofedTrans;

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public static void ResetStaticData()
        {
            CantSowCavePlantBecauseOfLightTrans = "CantSowCavePlantBecauseOfLight".Translate();
            CantSowCavePlantBecauseUnroofedTrans = "CantSowCavePlantBecauseUnroofed".Translate();
        }
        
        protected override bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn)
        {
            if (!settable.CanAcceptSowNow()) return false;
            Zone_Growing zone_Growing = settable as Zone_Growing;
            if (zone_Growing != null && !zone_Growing.allowSow) return false;
            wantedPlantDef = settable.GetPlantDefToGrow();
            return wantedPlantDef != null;
        }

        // Token: 0x060006D9 RID: 1753 RVA: 0x0003F024 File Offset: 0x0003D424
        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            Map map = pawn.Map;

            // Check sowing allowed.
            if (c.IsForbidden(pawn))
                return null;

            if (wantedPlantDef == null)
            {
                wantedPlantDef = CalculateWantedPlantDef(c, map);
                if (wantedPlantDef == null)
                    return null;
            }

            // Check temperature.
            CompProperties_Plant plantProps = wantedPlantDef.GetCompProperties<CompProperties_Plant>();
            if (plantProps == null)
            {
                if (!PlantUtility.GrowthSeasonNow(c, map, true))
                    return null;
            }
            else
            {
                float temperature = 21f;
                if (GenTemperature.TryGetTemperatureForCell(c, map, out temperature))
                {
                    if (temperature < plantProps.minGrowthTemperature || temperature > plantProps.maxGrowthTemperature) return null;
                }
            }

            // Check tile entities present. Disallow growing if same plant or non-growable building planned.
            List<Thing> thingList = c.GetThingList(map);
            Zone_Growing zone_Growing = c.GetZone(map) as Zone_Growing;
            bool ownBlueprintOrFramePresent = false;
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing.def == wantedPlantDef)
                    return null;

                if ((thing is Blueprint || thing is Frame) && thing.Faction == pawn.Faction)
                    ownBlueprintOrFramePresent = true;
            }
            if (ownBlueprintOrFramePresent)
            {
                Thing edifice = c.GetEdifice(map);
                if (edifice == null || edifice.def.fertility < 0f)
                    return null;
            }

            // Check roof and light for cave plants.
            if (wantedPlantDef.plant.cavePlant)
            {
                if (!c.Roofed(map))
                {
                    JobFailReason.Is(CantSowCavePlantBecauseUnroofedTrans, null);
                    return null;
                }
                if (map.glowGrid.GameGlowAt(c, true) > 0f)
                {
                    JobFailReason.Is(CantSowCavePlantBecauseOfLightTrans, null);
                    return null;
                }
            }

            // Don't grow trees indoors.
            if (wantedPlantDef.plant.interferesWithRoof && c.Roofed(pawn.Map))
                return null;

            // Cut existing plants.
            RimWorld.Plant existingPlant = c.GetPlant(map);
            if (existingPlant != null && existingPlant.def.plant.blockAdjacentSow)
            {
                if (!pawn.CanReserve(existingPlant, 1, -1, null, forced) || existingPlant.IsForbidden(pawn))
                    return null;
                if (zone_Growing != null && !zone_Growing.allowCut)
                    return null;
                if (!PlantUtility.PawnWillingToCutPlant_Job(existingPlant, pawn))
                    return null;
                return JobMaker.MakeJob(JobDefOf.CutPlant, existingPlant);
            }

            // Check adjacent blocker.
            Thing adjacentSowBlocker = PlantUtility.AdjacentSowBlocker(wantedPlantDef, c, map);
            if (adjacentSowBlocker != null)
            {
                Plant adjacentPlant = adjacentSowBlocker as Plant;
                if (adjacentPlant != null && pawn.CanReserveAndReach(adjacentPlant, PathEndMode.Touch, Danger.Deadly, 1, -1, null, forced) && !adjacentPlant.IsForbidden(pawn))
                {
                    IPlantToGrowSettable plantToGrowSettable = adjacentPlant.Position.GetPlantToGrowSettable(adjacentPlant.Map);
                    if (plantToGrowSettable == null || plantToGrowSettable.GetPlantDefToGrow() != adjacentPlant.def)
                    {
                        Zone_Growing adjacentPlantZone = adjacentPlant.Position.GetZone(map) as Zone_Growing;
                        if ((zone_Growing != null && !zone_Growing.allowCut) || (adjacentPlantZone != null && !adjacentPlantZone.allowCut))
                            return null;
                        if (!PlantUtility.PawnWillingToCutPlant_Job(adjacentPlant, pawn))
                            return null;
                        return JobMaker.MakeJob(JobDefOf.CutPlant, adjacentPlant);
                    }
                }
                return null;
            }

            // Check skill level.
            if (wantedPlantDef.plant.sowMinSkill > 0 && pawn.skills != null && pawn.skills.GetSkill(SkillDefOf.Plants).Level < wantedPlantDef.plant.sowMinSkill)
            {
                JobFailReason.Is("UnderAllowedSkill".Translate(wantedPlantDef.plant.sowMinSkill), def.label);
                return null;
            }

            // Check things at site that could block planting. Remove if possible.
            int j = 0;
            while (j < thingList.Count)
            {
                Thing thing = thingList[j];
                if (thing.def.BlocksPlanting(false))
                {
                    if (!pawn.CanReserve(thing, 1, -1, null, forced))
                        return null;

                    if (thing.def.category == ThingCategory.Plant)
                    {
                        if (thing.IsForbidden(pawn) || (zone_Growing != null && !zone_Growing.allowCut))
                            return null;
                        if (!PlantUtility.PawnWillingToCutPlant_Job(thing, pawn))
                            return null;
                        return JobMaker.MakeJob(JobDefOf.CutPlant, thing);
                    }

                    if (thing.def.EverHaulable)
                        return HaulAIUtility.HaulAsideJobFor(pawn, thing);

                    return null;
                }
                j++;
            }

            // Check if location still blocked or fertility insufficient.
            if (!wantedPlantDef.CanNowPlantAt(c, map) || !pawn.CanReserve(c, 1, -1, null, forced))
                return null;

            // Return sowing job.
            Job job = JobMaker.MakeJob(JobDefOf.Sow, c);
            job.plantDefToSow = wantedPlantDef;
            return job;
        }

    }
}
