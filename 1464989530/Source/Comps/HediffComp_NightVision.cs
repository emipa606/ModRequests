// Nightvision NightVision HediffComp_NightVision.cs
// 
// 16 05 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using System;
using System.Text;
using Verse;

namespace NightVision
{
    /// <summary>
    ///     More comps and props and comps and props and comps and props
    /// </summary>
    public class HediffComp_NightVision : HediffComp
    {
        [UsedImplicitly]
        public HediffCompProperties_NightVision Props => (HediffCompProperties_NightVision)props;


        public override string CompTipStringExtra => TipString();

        private string TipString()
        {
            switch (Props.LightModifiers.Setting)
            {
                case VisionType.NVNightVision: return "NVGiveNV".Translate();
                case VisionType.NVPhotosensitivity: return "NVGivePS".Translate();
                case VisionType.NVCustom:
                    StringBuilder result = new StringBuilder();
                    if (Math.Abs(Props.LightModifiers[0]) > 0.0001)
                    {
                        result.AppendFormat("{0} = {1:+#;-#;0}% {2}", "NVZeroSh".Translate(), Props.LightModifiers[0] * 100, "NVWorkMoveShort".Translate());

                        if (Math.Abs(Props.LightModifiers[1]) > 0.0001)
                        {
                            result.AppendLine();
                        }
                    }

                    if (Math.Abs(Props.LightModifiers[1]) > 0.0001)
                    {
                        result.AppendFormat("{0} = {1:+#;-#;0}% {2}", "NVFullSh".Translate(), Props.LightModifiers[1] * 100, "NVWorkMoveShort".Translate());
                    }

                    return result.ToString();
                default: return string.Empty;
            }
        }
    }
}