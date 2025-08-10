using UnityEngine;
using Verse;
using RimWorld;

namespace MultiFloorStorage
{
    [StaticConstructorOnStartup]
    public static class TexCommandViewLinkedStorage
    {
        public static readonly Texture2D ViewLinkedStorage = ContentFinder<Texture2D>.Get("UI/Commands/ShowMap");

    }
}