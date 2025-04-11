using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using System.Linq;
using Verse.Sound;
using RimWorld;


namespace LobotomyCorp.Abnos
{
    public class CompAbnosCore : ThingComp
    {
        private int count = 0;
        private static readonly int TICK_TIME = 60;

        public Thing abnos;
        private Room room;

        private Command setAbnos;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            abnos = null;
            room = null;

            setAbnos = new Command_Target()
            {
                targetingParams = new TargetingParameters()
                {
                    validator = t => {

                        if (t == null || !t.IsValid || !t.HasThing
                        || !t.Thing.HasThingCategory(LCDefOf.LCorpAbnormality)
                        ) return false;
                        return true;
                    },
                },
                defaultLabel = "set Abnos",
                action = t => {
                    if(t.Thing.TryGetComp<CompAbnos_Base>().core != null)
                    {
                        //Messages.
                        Log.Message("登録済み");
                        return;
                    }
                    abnos = t.Thing;
                    abnos.TryGetComp<CompAbnos_Base>().core = parent;
                    //abnos.Position = room.ExtentsClose.CenterCell;
                },
            };
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            abnos = null;
            room = null;
        }

        // Longがだいたい3倍で１秒ごとにcount+1って感じ。
        public override void CompTick()
        {
            count = count > TICK_TIME ? 0 : count + 1;
            if (count % TICK_TIME != TICK_TIME-1) return;
            
            
            room = parent.GetCoreRoom();
            /*
            if (abnos == null)
            {
                abnos = room.RoomAbnos();
                if (abnos != null)
                {
                    abnos.TryGetComp<CompAbnos_Base>().core = parent;
                }
            }
            }*/
        }

        // 追加で表示する内容
        public override string CompInspectStringExtra()
        {
            string str = base.CompInspectStringExtra();
            str += (room?.Role.label ?? "none") + ":" + count;
            str += "\n" + (abnos?.Label ?? "nothing there") + ".";
            str += "\n"+parent.Position;
            return str;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref abnos, "abnos");
            if(abnos!=null) abnos.TryGetComp<CompAbnos_Base>().core = parent;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if(abnos == null) {
                yield return setAbnos;
            }
            else{
                foreach (Gizmo g in abnos.GetAbnosAction()) yield return g;
            }
        }

    }

}
