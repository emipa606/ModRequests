using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;

namespace Clutter_StructureWall
{
    [StaticConstructorOnStartup]
    static class UITex
    {
        public static Texture2D WallPrev = ContentFinder<Texture2D>.Get("Clutter/Ui/WallPreview");
        public static Texture2D WallPrev2 = ContentFinder<Texture2D>.Get("Clutter/Ui/WallPreview2");
        public static Texture2D ModWallPrev = ContentFinder<Texture2D>.Get("Clutter/Ui/ModWallPreview");
        public static Texture2D ModWallPrev2 = ContentFinder<Texture2D>.Get("Clutter/Ui/ModWallPreview2");

    }
}
