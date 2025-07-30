using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Grammar;

namespace CM_Wheres_Daddy_Going
{
    public class Tale_DaddyWentToGetTheThing : Tale_DoublePawnAndDef
    {
        public Tale_DaddyWentToGetTheThing()
        {
        }

        public Tale_DaddyWentToGetTheThing(Pawn pawn)
            : base(pawn, GeneratePawnDadIsLeaving(pawn), GenerateThingDadIsGetting(pawn))
        {
        }

        public static Pawn GeneratePawnDadIsLeaving(Pawn dad)
        {
            List<Pawn> childrenOnMap = dad.relations.Children.Where(pawn => pawn.Map == dad.Map).ToList();
            Pawn child = null;
            if (childrenOnMap.Count > 0)
                child = childrenOnMap.RandomElement();

            return child;
        }

        public static ThingDef GenerateThingDadIsGetting(Pawn dad)
        {
            return WheresDaddyGoingMod.Instance.MemoriesOfDad.WhatIsParentGetting(dad);
        }
    }
}
