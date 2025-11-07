using BillDoorsFramework;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace BillDoorsFramework
{
    public class IncidentWorker_BombardmentCE : IncidentWorker
    {
        BombardmentExtension extension => def.GetModExtension<BombardmentExtension>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!def.HasModExtension<BombardmentExtension>())
            {
                Log.Error(def.defName + " does not have BombardmentExtension");
                return false;
            }
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 origin = CellFinder.RandomEdgeCell(map);
            LaunchMissiles(parms, origin, extension.countRange.RandomInRange);
            Messages.Message(extension.messageKey.Translate(), new TargetInfo(parms.spawnCenter, map), MessageTypeDefOf.NeutralEvent);
            return true;
        }

        private void LaunchMissiles(IncidentParms parms, IntVec3 origin, int count)
        {
            LaunchMissile(parms, origin);
            for (int i = 0; i < count - 1; i++)
            {
                if (CellFinder.TryFindRandomCellNear(parms.spawnCenter, (Map)parms.target, extension.radius, null, out var pos))
                {
                    LaunchMissile(parms, origin, pos);
                }
            }
        }

        private void LaunchMissile(IncidentParms parms, IntVec3 origin)
        {
            LaunchMissile(parms, origin, parms.spawnCenter);
        }

        private void LaunchMissile(IncidentParms parms, IntVec3 origin, IntVec3 pos)
        {
            Map map = (Map)parms.target;
            Log.Message(parms.spawnCenter.ToString());
            ProjectileCE projectile = (ProjectileCE)ThingMaker.MakeThing(extension.possibleThings.RandomElement(), null);
            GenSpawn.Spawn(projectile, pos, map);
            projectile.Launch(projectile, new Vector2(pos.x, pos.z), -1.57079633f, 0, 100, 100);
            projectile.Destination = new Vector2(pos.x, pos.z);
        }
    }

    public class BombardmentExtension : DefModExtension
    {
        public int radius = 5;

        public List<ThingDef> possibleThings = new List<ThingDef>();

        public IntRange countRange = new IntRange(3, 5);

        public string messageKey = "MessageShipChunkDrop";
    }
}
