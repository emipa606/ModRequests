using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ReTill
{
    public class AutoReTillZone : MapComponent
    {
        public HashSet<Zone_Growing> active = new HashSet<Zone_Growing>();

        public AutoReTillZone(Map map) : base(map)
        {
        }

        public static void Set(Zone_Growing zone, bool value)
        {
            if (value)
            {
                zone.Map.GetComponent<AutoReTillZone>().active.Add(zone);
            }
            else
            {
                zone.Map.GetComponent<AutoReTillZone>().active.Remove(zone);
            }
        }

        public static bool Get(Zone_Growing zone)
        {
            return zone.Map.GetComponent<AutoReTillZone>().active.Contains(zone);
        }

        public static void Forget(Zone_Growing zone)
        {
            zone.Map.GetComponent<AutoReTillZone>().active.Remove(zone);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref active, "autoReTill", LookMode.Reference);

            active.Remove(null); // just in case a zone disappears from a save file somehow
        }
    }
}
