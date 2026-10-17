// ******************************************************************
//       /\ /|       @file       MapExtension.cs
//       \ V/        @brief      地图扩展
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-07-27 11:46:48
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using System.Linq;
using RimWorld;
using Verse;

namespace SR.ModRimWorld.RaidExtension
{
    public static class ThingValidator
    {
        public static bool IsAnimal(Thing t, float minTargetRequireHealthScale)
        {
            return t is Pawn animal && animal.RaceProps != null && animal.RaceProps.Animal
                && !animal.Dead && animal.RaceProps.baseHealthScale >= minTargetRequireHealthScale;
        }

        public static bool IsTree(Thing t)
        {
            return t is Plant plant && !plant.IsBurning() && plant.IsTree() && plant.CanYieldNow();
        }
    }
}
