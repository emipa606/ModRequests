using LudeonTK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace BillDoorsUnifiedHaulJob
{
    public static class PawnIRefuelableCache
    {
        public static Dictionary<Pawn, IRefuelable> dict = new Dictionary<Pawn, IRefuelable>();
    }


    public class MapComponent_UnifiedHaulTracker : MapComponent
    {
        public MapComponent_UnifiedHaulTracker(Map map) : base(map)
        {
        }

        List<IRefuelable> refuelables = new List<IRefuelable>();

        Dictionary<Thing, List<IRefuelable>> refuelablesForThing = new Dictionary<Thing, List<IRefuelable>>();

        public List<IRefuelable> RefuelablesReadOnly => refuelables;

        public List<Thing> ActiveRefuelables => RefuelablesReadOnly.Where(t => t.ShouldRefuelNow).Select(i => i.Parent).ToList();

        public IEnumerable<IRefuelable> ActiveIRefuelableInThing(Thing thing)
        {
            if (!refuelablesForThing.ContainsKey(thing)) return new List<IRefuelable>();
            return refuelablesForThing[thing].Where(i => i.ShouldRefuelNow);
        }
        public IEnumerable<IRefuelable> ActiveIRefuelableInThingWithFoundFuel(Thing thing)
        {
            return ActiveIRefuelableInThing(thing).Where(i => i.ShouldRefuelNow && i.FoundFuel != null);
        }

        public IRefuelable ActiveIRefuelableInThingRefuelableFrom(Thing thing, Thing fuel, int amount)
        {
            foreach (var i in ActiveIRefuelableInThing(thing))
            {
                if (i.FuelValidator(fuel) && i.RequestAmount >= amount)
                {
                    return i;
                }
            }
            return null;
        }

        public void Register(IRefuelable refuelable)
        {
            refuelables.AddDistinct(refuelable);
            if (refuelablesForThing.ContainsKey(refuelable.Parent))
            {
                refuelablesForThing[refuelable.Parent].AddDistinct(refuelable);
            }
            else
            {
                refuelablesForThing.Add(refuelable.Parent, new List<IRefuelable> { refuelable });
            }
        }

        public void Deregister(IRefuelable refuelable)
        {
            refuelables.Remove(refuelable);
            if (refuelablesForThing.ContainsKey(refuelable.Parent))
            {
                refuelablesForThing[refuelable.Parent].Remove(refuelable);
                if (refuelablesForThing[refuelable.Parent].NullOrEmpty()) refuelablesForThing.Remove(refuelable.Parent);
            }
        }

        public bool Valid(IRefuelable refuelable)
        {
            if (refuelable.Parent == null || !refuelable.Parent.SpawnedOrAnyParentSpawned)
            {
                Deregister(refuelable);
                return false;
            }
            return true;
        }

        [DebugAction("Bill Doors' Unified Hauling", "Log registered refuelable", false, false, false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void DebugClearTracker()
        {
            var comp = Find.CurrentMap.GetComponent<MapComponent_UnifiedHaulTracker>();
            StringBuilder stb = new StringBuilder();
            string s, d;
            foreach (var t in comp.RefuelablesReadOnly)
            {
                s = t.ShouldRefuelNow ? $"{t.RequestAmount}" : "false";
                d = t.FoundFuel?.ToString() ?? "";
                stb.AppendLine($"{t.Parent} : {s} {d}");
            }
            Log.Message(stb);
        }
    }
}
