using System;
using UnityEngine;
using Verse;

namespace LyingFace {

    [StaticConstructorOnStartup]
    public static class MyTex {
        public static readonly Texture2D LyingFaceNorth = ContentFinder<Texture2D>.Get("UI/Commands/LyingFaceNorth", true);
        public static readonly Texture2D LyingFaceEast = ContentFinder<Texture2D>.Get("UI/Commands/LyingFaceEast", true);
        public static readonly Texture2D LyingFaceWest = ContentFinder<Texture2D>.Get("UI/Commands/LyingFaceWest", true);
        public static readonly Texture2D LyingFaceSouth = ContentFinder<Texture2D>.Get("UI/Commands/LyingFaceSouth", true);
    }
}
