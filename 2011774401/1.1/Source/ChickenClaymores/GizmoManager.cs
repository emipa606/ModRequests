using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace ChickenExplosives
{
    [StaticConstructorOnStartup]
    static class GizmoManager
    {
        public static Dictionary<ThingDef, Command_Action> remoteTriggerGizmos;

        static GizmoManager()
        {
            remoteTriggerGizmos = new Dictionary<ThingDef, Command_Action>();
            foreach (ThingDef td in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasComp(typeof(CompRemoteTrigger))))
            {
                if (td == null) continue;
                CompProperties_RemoteTrigger cprt = td.GetCompProperties<CompProperties_RemoteTrigger>();
                if (cprt != null)
                {
                    remoteTriggerGizmos.Add(td, new Command_Action
                    {
                        defaultLabel = cprt.labelKey.Translate(),
                        defaultDesc = cprt.descKey.Translate(),
                        icon = ContentFinder<Texture2D>.Get(cprt.iconPath, true),
                        hotKey = cprt.keyBinding,
                        activateSound = cprt.activateSound                        
                    });
                }
            }
        }
    }
}
