//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using RimWorld;
//using Verse;
//using Verse.AI;


//namespace CompinjectionThingy
//{
//    //Scraped with bits o Hospitality mod help :D
//    [StaticConstructorOnStartup]
//    internal static class CompInjection
//    {
//        static CompInjection()
//        {
//            LongEventHandler.QueueLongEvent(new Action(CompInjection.InjectStuff), "Initializing", true, null);
//        }

//        private static Assembly Assembly
//        {
//            get
//            {
//                return Assembly.GetAssembly(typeof(CompInjection));
//            }
//        }

//        public static void InjectStuff()
//        {
//            //string[] VaniliaThings = { "Bed", "DoubleBed", "RoyalBed","HospitalBed" };
//            //foreach(string t in VaniliaThings)
//            //{
//            //    AddStuffToComp(t, "ClutterCabinetA");
//            //    AddStuffToComp(t, "ClutterLockerA");
//            //}

//            string[] VaniliaThingsSeats = { "TableShort", "TableLong" };
//            foreach (string s in VaniliaThingsSeats)
//            {
//                AddStuffToComp(s, "ClutterStool");
//                AddStuffToComp(s, "ClutterComfySeat");
//            }
//        }

//        public static void AddStuffToComp(string thingdefName, string target)
//        {
//            ThingDef thingdefThing = ThingDef.Named(thingdefName);
//            ThingDef targetThingy = ThingDef.Named(target);
//            if (thingdefThing != null && targetThingy != null)
//            {
//                var compThingy = thingdefThing.GetCompProperties<CompProperties_AffectedByFacilities>();
//                if(compThingy!=null)
//                {
//                    compThingy.linkableFacilities.Add(targetThingy);
//                }
//                else
//                {
//                    CompProperties_AffectedByFacilities newCompy = null;
//                    newCompy.compClass = typeof(CompAffectedByFacilities);
//                    newCompy.linkableFacilities.Add(targetThingy);
//                    thingdefThing.comps.Add(newCompy);
//                }
//            }
//            else
//            {
//                if (thingdefThing == null) 
//                {
//                    Log.Warning("Cant find ThingDef to add to AffectedByFacilities");
//                }
//                if(targetThingy == null)
//                {
//                    Log.Warning("Cant find Target def to add to AffectedByFacilities");
//                }
//            }
//        }

//        private static string AssemblyName
//        {
//            get
//            {
//                return CompInjection.Assembly.FullName.Split(new char[]
//                {
//                    ','
//                }).First<string>();
//            }
//        }

//    }
//}

