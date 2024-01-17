using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace PrisonWalls
{
    public class Building_PrisonBar : Building
    {
        public override void TickRare()
        {
            Log.Message("Temperature changing");
            GenTemperature.EqualizeTemperaturesThroughBuilding(this, 14f, twoWay: true);
        }
    }
}
