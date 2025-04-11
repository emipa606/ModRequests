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
    public class BuildingAbnos : Building
    {

        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            str += Position;
            return str;
        }



    }


}
