using System;
using UnityEngine;
using Verse;

namespace AppearanceClothes {
    [StaticConstructorOnStartup]
    internal class MyTexButton {
        public static readonly Texture2D Delete = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);
        public static readonly Texture2D ChangeColor = ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true);
        public static readonly Texture2D RotRight = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight", true);
    }
}
