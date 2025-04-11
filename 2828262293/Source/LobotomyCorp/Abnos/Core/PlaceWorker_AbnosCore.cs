using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LobotomyCorp.Abnos
{
    public class PlaceWorker_AbnoCore : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            IntVec3 vec = center + IntVec3.North.RotatedBy(rot);
            IntVec3 vec2 = center + IntVec3.South.RotatedBy(rot);
            //itemを落とす所
            GenDraw.DrawFieldEdges(new List<IntVec3> { vec }, Color.blue);
            GenDraw.DrawFieldEdges(new List<IntVec3> { vec2 }, Color.green);

            Room room = vec.GetRoom(currentMap);
            if(room != null && room.Role != RoomRoleDefOf.None) GenDraw.DrawFieldEdges(room.Cells.ToList<IntVec3>(), Color.blue);
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            IntVec3 vec = loc + IntVec3.North.RotatedBy(rot);
            IntVec3 vec2 = loc + IntVec3.South.RotatedBy(rot);

            if (vec.Impassable(map) || vec2.Impassable(map)) return "MustPlaceFreeSpaces";
            //if (vec.GetRoom(map).Role == LCDefOf.LCAbnosRoom) return "not duplicated core";
            return AcceptanceReport.WasAccepted;
        }

        /*
        public override void DrawMouseAttachments(BuildableDef def)
        {
            Log.Message("nyan");
            // 設置する際に、マウスで表示されるやつ
        }
        */

        //

    }

    public class RoomRoleWorker_ContainmentRoom : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            Thing core = Cache.GetCore(room);
            if (core == null) return 0f;
            return 200000f;
        }
    }
}
