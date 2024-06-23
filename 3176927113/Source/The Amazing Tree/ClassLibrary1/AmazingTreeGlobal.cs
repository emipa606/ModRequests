using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace The_Amazing_Tree
{
    public class MapComponent_PsyMediumManager : MapComponent
    {
        // Token: 0x060000F4 RID: 244 RVA: 0x000064C6 File Offset: 0x000046C6
        public MapComponent_PsyMediumManager(Map map) : base(map)
        {
            AddComps();
        }
        public override void MapGenerated()
        {
            base.MapGenerated();
            AddComps();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref psyMediumThings, "CompPsycastMedium",LookMode.Reference);
        }

        public void AddComps()
        {
            psyMediumThings = new HashSet<Thing>();
            foreach (var item in map.listerThings.AllThings)
            {
                var comp = item.TryGetComp<CompPsycastMedium>();
                
                if (comp != null)
                {
                    psyMediumThings.Add(item);
                }
            }
        }
        public void AddThing(Thing t)
        {
            psyMediumThings.Add(t);
        }

        public HashSet<CompPsycastMedium> PsyMediumComps
        {
            get
            {
                var set=new HashSet<CompPsycastMedium>();
                foreach(var thing in psyMediumThings)
                {
                    set.Add(thing.TryGetComp<CompPsycastMedium>());
                }
                return set;
            }
        }
        HashSet<Thing> psyMediumThings;

    }

}
