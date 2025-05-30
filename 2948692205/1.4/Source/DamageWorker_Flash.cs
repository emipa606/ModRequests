using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace IndustrialArmory
{

    public class DamageWorker_Flash : DamageWorker_AddGlobal
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            var result = base.Apply(dinfo, thing);
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                pawn.stances.stunner.StunFor(120, dinfo.Instigator);
            }
            return result;
        }
    }
}
