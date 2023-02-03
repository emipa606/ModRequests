using System.Security.Policy;
using UnityEngine;
using Verse;

// ReSharper disable All

namespace WallHeater
{
    [StaticConstructorOnStartup]
    public static class ResourceBank
    {
        public const string modName = "WallHeater";
        public static string NeedsWall = "Can only be placed on a wall";
        public static string WallAlreadyOccupied = "WallHeater_WallAlreadyOccupied".Translate();
    }
}