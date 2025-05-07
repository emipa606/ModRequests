using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DTechprinting.ShardBuilding
{
    public class Command_ShardBuilding : Command_Action
    {
        public Building building;

        public Command_ShardBuilding(Building b)
        {
            this.building = b;
        }
    }
}
