using UnityEngine;
using Verse;

namespace Revolus.TextileStats {
    [StaticConstructorOnStartup]
    public class UI {
        public static readonly Texture2D InfoButton = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton");
    }
}
