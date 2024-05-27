﻿///Always required
///Possible libraries

using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI.Group;

//using RimWorld.Planet;
//using Verse.AI;
//using Verse.AI.Group;


namespace CosmicHorror
{
    public class IncidentWorker_StarVampireAttack_Depricated : IncidentWorker
    {
        private CosmicHorrorPawn iwVampire; //The Star Vampire Pawn
        private ThingDef iwDef; //For the custom Spawner from JecsTools
        private Faction iwFac; //The Star Vampire Faction
        private IntVec3 iwLoc; //The Star Vampire location
        private SoundDef iwWarn; //The Star Vampire Warning Noise
        private Lord lord; //The Star Vampire AI manager

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (GenDate.DaysPassed < (ModInfo.cosmicHorrorRaidDelay + def.earliestDay))
            {
                Log.Message(text: "Cosmic Horrors :: CantFireDueTo Time :: " + GenDate.DaysPassed +
                                  " days passed, but we need " + ModInfo.cosmicHorrorRaidDelay.ToString() + " days + " +
                                  def.earliestDay);
                return false;
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //Resolve parameters.
            ResolveSpawnCenter(parms: parms);

            //Initialize variables.
            iwDef = ResolveSpawner(parms: parms);
            iwWarn = MonsterDefOf.Pawn_ROM_StarVampire_Warning;
            iwVampire = null; //iwPawn as CosmicHorrorPawn;
            iwLoc = CellFinder.RandomClosewalkCellNear(root: parms.spawnCenter, map: (Map)parms.target, radius: 8);

            //In-case there's something silly happening...
            if (iwFac == null)
            {
                iwFac = Find.FactionManager.FirstFactionOfDef(facDef: FactionDefOf.AncientsHostile);
            }

            //Slow down time
            Find.TickManager.slower.SignalForceNormalSpeed();
            //Play a sound.
            iwWarn.PlayOneShotOnCamera();
            //Show the warning message.
            Messages.Message(text: "StarVampireIncidentMessage".Translate(),
                lookTargets: new RimWorld.Planet.GlobalTargetInfo(cell: IntVec3.Invalid, map: (Map)parms.target),
                def: MessageTypeDefOf.SituationResolved);
            //Spawn the Star Vampire.
            SpawnStarVampires(parms: parms);
            return true;
        }

        private static ThingDef ResolveSpawner(IncidentParms parms)
        {
            Map iwMap = (Map)parms.target;
            var abberation =
                (GenCelestial.IsDaytime(
                    glow: GenCelestial.CelestialSunGlow(map: iwMap, ticksAbs: Find.TickManager.TicksAbs)))
                    ? MonsterDefOf.ROM_StarVampireSpawnerAbberation
                    : MonsterDefOf.ROM_StarVampireSpawnerNight;

            var chance = Rand.Value;
            if (chance > 0.3)
                return MonsterDefOf.ROM_StarVampireSpawner;
            else if (chance > 0.05)
                return abberation;
            else
                return MonsterDefOf.ROM_StarVampireSpawnerAlbino;
        }

        /// <summary>
        /// Where to, Cthulhu?
        /// </summary>
        /// <param name="parms"></param>
        protected void ResolveSpawnCenter(IncidentParms parms)
        {
            Map iwMap = (Map)parms.target;
            if (parms.spawnCenter.IsValid)
            {
                return;
            }

            if (Rand.Value < 0.4f &&
                iwMap.listerBuildings.ColonistsHaveBuildingWithPowerOn(def: ThingDefOf.OrbitalTradeBeacon))
            {
                parms.spawnCenter = DropCellFinder.TradeDropSpot(map: iwMap);
            }
            else
            {
                RCellFinder.TryFindRandomPawnEntryCell(result: out parms.spawnCenter, map: iwMap, roadChance: 0.0f);
            }
        }

        /// <summary>
        /// Finds the best number of Star Vampires
        /// to spawn. Then spawns them according
        /// to a previously resolved location.
        /// </summary>
        /// <param name="parms"></param>
        protected void SpawnStarVampires(IncidentParms parms)
        {
            Map iwMap = (Map)parms.target;

            int iwCount = 1;

            if (parms.points <= 2000f)
            {
                iwCount = 1;
            }
            else if (parms.points <= 3000f)
            {
                iwCount = 2;
            }
            else if (parms.points <= 5000f)
            {
                iwCount = 3;
            }

            for (int i = 0; i < iwCount; i++)
            {
                Thing iwSpawner = ThingMaker.MakeThing(def: iwDef, stuff: null);
                GenPlace.TryPlaceThing(thing: iwSpawner, center: iwLoc, map: iwMap, mode: ThingPlaceMode.Near);
                //    CosmicHorrorPawn temp = null;
                //    this.iwVampire = null;
                //    this.iwSpawner = ThingMaker.MakeThing(this.iwKind, this.iwFac);
                //    this.iwVampire = this.iwPawn as CosmicHorrorPawn;

                //if (this.lord == null)
                //{
                //    LordJob_StarVampire lordJob = new LordJob_StarVampire(this.iwFac, this.iwLoc);
                //    this.lord = LordMaker.MakeNewLord(this.iwFac, lordJob, iwMap, null);
                //}
                //temp = (CosmicHorrorPawn)GenSpawn.Spawn(this.iwVampire, this.iwLoc, iwMap);
                //this.lord.AddPawn(temp);
            }
        }
    }
}