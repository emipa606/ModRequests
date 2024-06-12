using Verse;

namespace NightVision
{
    /// <summary>
    /// General strings defined in xml file in nightvision/languages
    /// </summary>
    public static class Str
    {
        public const string PhotoSTag = "Photosensitivity";

        public static readonly string NV = "NightVisionNV".Translate();
        public static readonly string PS = "NightVisionPS".Translate();

        public static readonly string Drk = "NightVisionDrk".Translate();
        public static readonly string BriL = "NightVisionBriL".Translate();

        public static string AcronymDef(string statName, string stAcronym) => "NightVisionAcronymDef".Translate(statName, stAcronym);

        public static readonly string Darkness = "NightVisionDarkness".Translate();
        public static readonly string Bright = "NightVisionBright".Translate();

        public static readonly string Effect = "NVEffects".Translate();

        public static readonly string NightVision = VisionType.NVNightVision.ToString().Translate();

        public static readonly string Photosens = VisionType.NVPhotosensitivity.ToString().Translate();

        public static string MaxAtGlow(float glow)
        {
            return "NVMaxAtGlow".Translate(glow.ToStringPercent());
        }


        public const string Alabel = "{0:+#;-#;0}%";
        public const string BodyKey = "WholeBody";
        public const string Maxline = " ({0}{1}: {2,6: +#0.0%;-#0.0%;+0.0%})";
        public const string ModifierLine = "{0}: {1,6:+#0.0%;-#0.0%;0.0%}";
        public const string MultiplierLine = "{0}: {1,6:x#0.0%;x#0.0%;x0.0%}";
        public const string XLabel = "x{0: #0}%";

        public static readonly string FullLabel =
                    "NVFullLabel".Translate() + " = {0:+#;-#;0}%";

        public static readonly string FullMultiLabel = "NVFullLabel".Translate() + " = x{0:##}%";

        public static readonly string ZeroLabel =
                    "NVZeroLabel".Translate() + " = {0:+#;-#;0}%";

        public static readonly string ZeroMultiLabel = "NVZeroLabel".Translate() + " = x{0:##}%";
        public static readonly string ExpIntro = "{3} " + Str.Effect + Maxline;
    }
}
