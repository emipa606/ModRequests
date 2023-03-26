using UnityEngine;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using HarmonyLib;

namespace AutoHarvesterCycle
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.fluffykittens.autoharvestercycle");

            //VFE.Mechanoids.Buildings.Building_AutoSower, VFEM, Version=1.0.1.2, Culture=neutral, PublicKeyToken=null
            Type targetType = Type.GetType("VFE.Mechanoids.Buildings.Building_AutoPlant, VFEM");
            MethodInfo targetmethod = AccessTools.Method(targetType, "GetGizmos");

            // find the static method to call before (i.e. Prefix) the targetmethod
            HarmonyMethod postfixmethod = new HarmonyMethod(typeof(AutoHarvesterCycle.HarmonyPatches).GetMethod("GetGizmosPostfix"));

            // patch the targetmethod, by calling prefixmethod before it runs, with no postfixmethod (i.e. null)
            harmony.Patch(targetmethod, null, postfixmethod);
        }


        /*
         * We want to remove the start/stop toggle since the new start/stop cycle button does similar things
         * With the current mech mod this should always be the last item in the gizmo list.
         * However in case the code changes, I'm adding a bunch of checks here to reduce the odds of us removing a random button in a future update
         */
        public static void GetGizmosPostfix(ref IEnumerable<Gizmo> __result)
        {
            if (__result.GetType() != typeof(List<Gizmo>)) return;
            List<Gizmo> gList = (List<Gizmo>)__result;

            if (gList.Count == 0) return;
            Gizmo lastItem = gList[gList.Count - 1];

            if (lastItem.GetType() != typeof(Command_Toggle)) return;
            Command_Toggle cmdToggle = (Command_Toggle)lastItem;

            if (cmdToggle.defaultLabel != "VFEMechStopMachine".Translate() && cmdToggle.defaultLabel != "VFEMechStartMachine".Translate()) return;

            // All checks passed
            gList.RemoveLast<Gizmo>();
        }
    }

    public class CycleComp : ThingComp
    {
        static int ticksPerHour = 2500;

        public bool cycleActive;
        public int cycleHours;
        public int cycleTicksLeft;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            cycleActive = false;
            cycleHours = 24;
            cycleTicksLeft = 0;
        }

        private void log(object obj)
        {
            if(obj == null)
            {
                Log.Message("<null>");
            }
            else
            {
                Log.Message(obj.ToString());
            }
        }
        private void log(string obj)
        {
            Log.Message(obj);
        }

        private void setRunning(bool val)
        {
            Type buildingType = parent.GetType();
            FieldInfo runProp = buildingType.GetField("running");
            runProp.SetValue(parent, val);
        }

        private bool PowerOn => parent.GetComp<CompPowerTrader>().PowerOn;

        public override void CompTick()
        {
            if (PowerOn)
            {
                // Cycle code
                if (cycleActive)
                {
                    cycleTicksLeft--;
                    if (cycleTicksLeft <= 0)
                    {
                        cycleTicksLeft = cycleHours * ticksPerHour;
                        setRunning(true);
                    }
                }
            }
        }

        // Do something
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            List<Gizmo> gizmos = new List<Gizmo>();

            Command_Action cmdTemp;

            cmdTemp = new Command_Action();
            cmdTemp.action = delegate
            {
                cycleHours += 24;
                cycleTicksLeft += 24 * ticksPerHour;
            };
            cmdTemp.defaultLabel = "Extend Cycle";
            cmdTemp.defaultDesc = "Extend cycle by 24 hours";
            cmdTemp.icon = ContentFinder<Texture2D>.Get("UI/plusSymbol");
            gizmos.Add(cmdTemp);

            if (cycleHours > 24)
            {
                cmdTemp = new Command_Action();
                cmdTemp.action = delegate
                {
                    cycleHours -= 24;
                    cycleTicksLeft -= 24 * ticksPerHour;
                    if (cycleTicksLeft < 0)
                    {
                        cycleTicksLeft = 0;
                    }
                };
                cmdTemp.defaultLabel = "Shrink Cycle";
                cmdTemp.defaultDesc = "Shrink cycle by 24 hours";
                cmdTemp.icon = ContentFinder<Texture2D>.Get("UI/minusSymbol");
                gizmos.Add(cmdTemp);
            }

            if (!cycleActive)
            {
                cmdTemp = new Command_Action();
                cmdTemp.action = delegate
                {
                    cycleActive = true;
                    cycleTicksLeft = 0;
                    setRunning(true);
                };
                cmdTemp.defaultLabel = "Start cycle";
                cmdTemp.defaultDesc = $"Start {cycleHours}h work cycle";
                cmdTemp.icon = ContentFinder<Texture2D>.Get("UI/MachineryOn");
                gizmos.Add(cmdTemp);
            }
            else
            {
                cmdTemp = new Command_Action();
                cmdTemp.action = delegate
                {
                    cycleActive = false;
                    setRunning(false);
                };
                cmdTemp.defaultLabel = "Abort cycle";
                cmdTemp.defaultDesc = $"Abort current work cycle";
                cmdTemp.icon = ContentFinder<Texture2D>.Get("UI/MachineryOff");
                gizmos.Add(cmdTemp);
            }

            return gizmos;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref cycleActive, "cycleActive", false, true);
            Scribe_Values.Look<int>(ref cycleHours, "cycleHours", 24, true);
            Scribe_Values.Look<int>(ref cycleTicksLeft, "cycleTicksLeft", 0, true);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();

            string status;
            if (cycleActive)
            {
                if (PowerOn)
                {
                    status = "active";
                }
                else
                {
                    status = "suspended due to lack of power";
                }
            }
            else
            {
                status = "inactive";
            }

            stringBuilder.AppendLine($"{cycleHours / 24} day cycle ({status})");
            if (cycleActive)
            {
                if (cycleTicksLeft > 24 * ticksPerHour)
                {
                    int approxDays = cycleTicksLeft / (24 * ticksPerHour);
                    if (approxDays == 1)
                    {
                        stringBuilder.AppendLine($"{approxDays} day until repetition");
                    }
                    else
                    {
                        stringBuilder.AppendLine($"{approxDays} days until repetition");
                    }
                }
                else
                {
                    int approxHours = cycleTicksLeft / ticksPerHour;
                    if (approxHours == 1)
                    {
                        stringBuilder.AppendLine($"{approxHours} hour until repetition");
                    }
                    else
                    {
                        stringBuilder.AppendLine($"{approxHours} hours until repetition");
                    }
                }
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }
    }

    public class CycleCompProperties : CompProperties
    {
        /// <summary>
        /// These constructors aren't strictly required if the compClass is set in the XML.
        /// </summary>
        public CycleCompProperties()
        {
            this.compClass = typeof(CycleComp);
        }

        public CycleCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }

}
