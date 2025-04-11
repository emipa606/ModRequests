using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace LobotomyCorp.Abnos.MoSB
{
    public class CompAbnos_MoSB : CompAbnos_Base
    {

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            action = new Command_Target() { };
        }


        public override void CompTick()
        {
            base.CompTick();
        }



    }
}
