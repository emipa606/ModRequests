using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LobotomyCorp.Abnos
{
    /*
    public class AbnosUtility : MapComponent
    {
        // Mapかなんか作って、Setすると名前とthingIDの関連付けをするクラスを作る
        private static Dictionary<string, Thing> coreAbnosMap 
            = new Dictionary<string, Thing>();

        public AbnosUtility(Map map) : base(map)
        {
            //coreAbnosMap = new Dictionary<int, ThingDef>();
        }

        public static void AddCorrespond(Thing core,Thing abnos)
        {
            if(coreAbnosMap.ContainsKey(core.ThingID)) coreAbnosMap.Remove(core.ThingID);
            coreAbnosMap.Add(core.ThingID, abnos);
        }

        public static Thing GetCorrespondAbnos(Thing core)
        {
            Thing tmp;
            if (coreAbnosMap.TryGetValue(core.ThingID, out tmp)) return tmp;
            return null;
        }



        public override void MapComponentTick()
        {
            base.MapComponentTick();
        }

    }
    */
}
