using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;


namespace BorgAssimilate
{
    class SpawnUtilityBorg
    {
        public SpawnUtilityBorg()
        {

        }

        public void spawnBorgPawn(Pawn deadPawn, String factionName,Map map)
        {
            bool flag = factionName.Equals(Faction.OfPlayer.def.defName);
            //ModCore.Log(flag.ToString());
            Faction faction = FactionUtility.DefaultFactionFrom(FactionDef.Named(factionName));
           // Map map = deadPawn.Map;
            Pawn spawn = PawnGenerator.GeneratePawn(flag ? this.playerPawnKind : this.hostilePawnKind, faction);
            spawn.Position = deadPawn.Position;
            if (map != null)
            {
                //ModCore.Log("valid map");
                spawn.SpawnSetup(map, false);
            }

            if (flag)
            {
                //ModCore.Log("setting faction to player");
                spawn.SetFaction(Faction.OfPlayer, null);
            }
            else
            {

                //ModCore.Log("applying duty");
                IntVec3 intVec = new IntVec3(map.Size.x / 2, 0, map.Size.z / 2);// ModCore.Log(faction.ToString()); ModCore.Log(intVec.ToString());
                LordJob_DefendShip defendShipJob = new LordJob_DefendShip(faction, intVec);
                Lord defendShip = LordMaker.MakeNewLord(faction, defendShipJob, map, null);
                defendShip.AddPawn(spawn);
            }
            if (!deadPawn.Destroyed)
            {
                deadPawn.Destroy(DestroyMode.Vanish);
            }
            Corpse corpse = deadPawn.Corpse;
            bool flag2 = corpse != null;
            if (flag2)
            {
                corpse.Destroy(DestroyMode.Vanish);
            }


        }
        public void spawnBorgPawn2(Pawn deadPawn, String factionName)
        {
            bool flag = factionName.Equals(Faction.OfPlayer.def.defName);
            ModCore.Log(flag.ToString());
            Faction faction = FactionUtility.DefaultFactionFrom(FactionDef.Named(factionName));
            Map map = deadPawn.Map;
            Pawn spawn = PawnGenerator.GeneratePawn(flag ? this.playerPawnKind : this.hostilePawnKind, faction);
            spawn.Position = deadPawn.Position;
            if (map != null) 
            {
                ModCore.Log("valid map");
                spawn.SpawnSetup(map, false);
            }
            
            if (flag)
            {
                ModCore.Log("setting faction to player");
                spawn.SetFaction(Faction.OfPlayer, null);
            }
            else
            {
                
                ModCore.Log("applying duty");
                IntVec3 intVec = new IntVec3(map.Size.x / 2, 0, map.Size.z / 2); ModCore.Log(faction.ToString()); ModCore.Log(intVec.ToString());
                LordJob_DefendShip defendShipJob = new LordJob_DefendShip(faction, intVec); 
                Lord defendShip = LordMaker.MakeNewLord(faction, defendShipJob, map, null);
                defendShip.AddPawn(spawn);
            }
            Corpse corpse = deadPawn.Corpse;
            bool flag2 = corpse != null;
            if (flag2)
            {
                deadPawn.Destroy(DestroyMode.Vanish);
            }

        }


        public void spawnBorgPawn_old(Pawn deadPawn, FactionDef factionOf)
        {
                bool flag = factionOf.Equals(Faction.OfPlayer);
         
            Pawn spawn = PawnGenerator.GeneratePawn(flag ? this.playerPawnKind : this.hostilePawnKind, FactionUtility.DefaultFactionFrom(factionOf));
            spawn.Position = deadPawn.Position;
            spawn.SpawnSetup(deadPawn.Map, false);
            if (flag)
            {
                spawn.SetFaction(Faction.OfPlayer, null);
            }
            else
            {
                IntVec3 intVec = new IntVec3(deadPawn.Map.Size.x / 2, 0, deadPawn.Map.Size.z / 2);
                Lord defendShip = LordMaker.MakeNewLord(FactionUtility.DefaultFactionFrom(factionOf), new LordJob_DefendShip(FactionUtility.DefaultFactionFrom(factionOf), intVec), deadPawn.Map, null);
                defendShip.AddPawn(spawn);
            }
         
                deadPawn.Destroy(DestroyMode.Vanish);
            

        }

        public static bool isBorg(Pawn pawn)
        {
            return pawn.def.race.body.defName.Equals("borg");
        }

        private PawnKindDef getPlayerKind()
        {
            return this.playerPawnKind;
        }
        private PawnKindDef getHostileKind()
        {
            return this.hostilePawnKind;
        }
        public float spawnChanceAI = 1.0f;
        public float spawnChancePlayer = 1.0f;

        public PawnKindDef playerPawnKind = PawnKindDef.Named("PlayerBorgDrone");
        public PawnKindDef hostilePawnKind=PawnKindDef.Named("BorgDrone3");
        private Faction factionHostile = Faction.OfAncientsHostile;

    }
}
