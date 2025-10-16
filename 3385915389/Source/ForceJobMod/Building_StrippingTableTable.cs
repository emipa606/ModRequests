using Verse;
using RimWorld;

namespace ForceJobMod
{
    public class Building_StrippingTableTable : Building
    {
        public bool isInUse;
        private int ticksUntilSpawn;
        
        
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isInUse, "isInUse", false);
            Scribe_Values.Look(ref ticksUntilSpawn, "ticksUntilSpawn", 5); // Default 600 ticks (10 seconds at normal speed)
        }

        public override void TickRare()
        {
            base.TickRare();

            if (isInUse)
            {
                if (ticksUntilSpawn <= 0)
                {
                    if (HasLinkedColonyCam())
                    {
                        SpawnSilver();
                    }
                    ResetSpawnCountdown();

                    // Check if table is still in use after spawning silver
                    if (!isInUse)
                    {
                        Log.Message("[DEBUG] Table is no longer in use after silver spawn. Stopping spawn countdown.");
                        ticksUntilSpawn = 5; // Reset spawn countdown if table is no longer in use
                    }
                }
                else
                {
                    ticksUntilSpawn -= 50; // Decrement by rare tick interval
                }
            }
            else
            {
                // Reset countdown when no longer in use
                ticksUntilSpawn = 50;
            }
        }

        private bool HasLinkedColonyCam()
        {
            CompAffectedByFacilities comp = GetComp<CompAffectedByFacilities>();
            if (comp != null && comp.LinkedFacilitiesListForReading != null)
            {
                foreach (var facility in comp.LinkedFacilitiesListForReading)
                {
                    if (facility.def.defName == "ColonyCam")
                    {
                        // Cast facility to ThingWithComps to access GetComp
                        var facilityWithComps = facility as ThingWithComps;
                        if (facilityWithComps != null)
                        {
                            var powerComp = facilityWithComps.GetComp<CompPowerTrader>();
                            if (powerComp != null && powerComp.PowerOn)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


        private void SpawnSilver()
        {
            // Get the room the table is in
            Room room = this.GetRoom();

            if (room != null)
            {
                // Get the beauty stat of the room
                float beauty = room.GetStat(RoomStatDefOf.Beauty);

                // Determine silver amount based on beauty
                int silverAmount = 0; // Default to 0 silver if conditions are not met

                if (beauty < -3.5)
                {
                    silverAmount = Rand.RangeInclusive(0, 1); // Hideous
                }
                else if (beauty >= 0 && beauty < 2.4)
                {
                    silverAmount = Rand.RangeInclusive(2, 4); // Neutral
                }
                else if (beauty >= 2.4 && beauty < 5)
                {
                    silverAmount = Rand.RangeInclusive(5, 10); // Pretty
                }
                else if (beauty >= 5 && beauty < 15)
                {
                    silverAmount = Rand.RangeInclusive(11, 18); // Beautiful
                }
                else if (beauty >= 15 && beauty < 50)
                {
                    silverAmount = Rand.RangeInclusive(19, 28); // Very Beautiful
                }
                else if (beauty >= 50 && beauty < 100)
                {
                    silverAmount = Rand.RangeInclusive(29, 33); // Extremely Beautiful
                }
                else if (beauty >= 100)
                {
                    silverAmount = Rand.RangeInclusive(34, 38); // Unbelievably Beautiful
                }

                // Create the silver thing with the calculated stack count
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = silverAmount;

                // Place the silver at the interaction cell or nearby
                GenPlace.TryPlaceThing(silver, InteractionCell, Map, ThingPlaceMode.Near);
            }
        }



        private void ResetSpawnCountdown()
        {
            ticksUntilSpawn = 50; // Reset to 10 seconds
        }
    }
}