using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using LobotomyCorp.Abnos;
using Verse.AI;

namespace LobotomyCorp
{
    [StaticConstructorOnStartup]    
    public static class Cache
    {
        private static LobotomyThingList list = new LobotomyThingList();

        public static readonly List<ThingDef> abnos = list.AbnosList;

        public static Action setLCDamage = list.LCDamage;

        private static readonly string LC = "LCorp";
        private static readonly string LCSetting = LC + ".Setting";
        public static readonly string isLCDamageAvailable = LCSetting + ".isLCDamageAvailable";
        public static readonly string canDamageColonist = LCSetting + ".canDamageColonist";
        public static readonly string canDamageSameThingsInDifferentCell = LCSetting + ".canDamageSameThingsInDifferentCell";

        //private static readonly string packageId = "igugf.chocorofuru.lobotomycorporation";

        private class LobotomyThingList
        {
            public List<ThingDef> weapons;
            public List<ThingDef> bullets;
            public List<ThingDef> abnos;
            public List<ThingDef> all;

            public Action LCDamage;

            public LobotomyThingList()
            {
                weapons = new List<ThingDef>();
                bullets = new List<ThingDef>();
                abnos = new List<ThingDef>();
                all = new List<ThingDef>();
                foreach(ThingDef t in DefDatabase<ThingDef>.AllDefs)
                {
                    if(t.modContentPack != null 
                        && t.modContentPack.PackageId.Equals(
                            "igugf.chocorofuru.lobotomycorporation"))
                    {
                        if (t.IsWeapon) weapons.Add(t);
                        else if (t.category == ThingCategory.Projectile) bullets.Add(t);
                        else if ( t.IsAnormality() ) abnos.Add(t);
                        all.Add(t);
                    //    Log.Message(t.label);
                    }
                }

                LCDamage = () =>
                {
                    for (int i = 0; i < all.Count; i++)
                    {
                        ThingDef td = all[i];
                        foreach (DefModExtension mod in td.modExtensions ?? new List<DefModExtension>())
                        {
                            if (mod is LCDamage) ((LCDamage)mod).Change(ref td);
                        }
                    }
                };
                LCDamage.Invoke();

            }

            public List<ThingDef> AbnosList => abnos;

        }

        public static Thing GetCore()
        {
            foreach (Thing thing in Find.Selector.SelectedObjectsListForReading)
            {
                if (thing.def.IsLCCore()) return thing;
            }
            return null;
        }

        public static Thing GetCore(Room room)
        {
            if (room == null) return null;
            List<Thing> contain = room.ContainedAndAdjacentThings;
            if (contain.NullOrEmpty()) return null;
            List<Thing> core = contain.FindAll((Thing t) => t.def.IsLCCore());

            foreach(Thing t in core)
            {
                if (room.Equals(t.GetCoreRoom())) return t;
            }
            return null;
        }

        public static bool HasCore(Room room) => GetCore(room) != null;
        
        public static bool IsLCCore(this ThingDef td)
        {
           //return false;
           return td == LCDefOf.LCCore || td == LCDefOf.LCCoreWall;
        }

        public static Room GetCoreRoom(this Thing t)
        {
            IntVec3 vec3 = t.Position + IntVec3.North.RotatedBy(t.Rotation);
            return vec3.GetRoom(t.Map);
        }

        public static Thing RoomAbnos(this Room room)
        {

            if (room == null || room.Role == RoomRoleDefOf.None) return null;
            List<Thing> things = room.ContainedAndAdjacentThings;
            foreach (Thing t in things)
            {
                if (Cache.abnos.Contains(t.def)) return t;
            }
            return null;
        }

        public static Thing GetCoreAbnos() 
        {
            return GetCore()?.TryGetComp<CompAbnosCore>().abnos ?? null;
        }

        public static IEnumerable<Gizmo> GetAbnosAction(this Thing thing)
        {
            CompAbnos_Base cmp = thing.GetComp();
            return cmp.CoreGizmos();
        }

        public static CompAbnos_Base GetComp(this Thing t)
        {
            return t.TryGetComp<CompAbnos_Base>();
        }

        public static bool IsRoomNothing(this Room r)
            => r == null || r.Role == RoomRoleDefOf.None;

        public static string ToListString<T>(this List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            list.ForEach((T t) => sb.Append(t + ","));
            return sb.ToString();
        }

        public static void MakeManhunter(this Pawn p)
        {
            p.mindState.mentalStateHandler.TryStartMentalState(
                MentalStateDefOf.Manhunter, forceWake:true);
        }

        public static bool IsAnormality(this Thing t) {
            return IsAnormality(t.def);
        }

        public static bool IsAnormality(this ThingDef th)
        {
            return (th.thingCategories?.Contains(LCDefOf.LCorpAbnormality)) ?? false;
        }

        

    }
 
    [DefOf]
    public static class LCDefOf
    {
        
        
        public static readonly ThingDef LCCore;
        public static readonly ThingDef LCCoreWall;

        public static readonly ThingCategoryDef LCorpAbnormality;
        public static readonly RoomRoleDef LCAbnosRoom;

        public static readonly JobDef LCJob_ApproachAbnos;
        
    }
    

}
