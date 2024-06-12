// Nightvision NightVision Comp_NightVisionApparel.cs
// 
// 16 05 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using System.Text;
using Verse;

namespace NightVision
{
    public class Comp_NightVisionApparel : ThingComp
    {
        [UsedImplicitly]
        public CompProperties_NightVisionApparel Props => (CompProperties_NightVisionApparel)props;

        public override string GetDescriptionPart()
        {
            var result = new StringBuilder(base.GetDescriptionPart());

            if (Props?.AppVisionSetting?.GrantsNV == true)
            {
                result.AppendLine("NVGiveNV".Translate());
            }

            if (Props?.AppVisionSetting?.NullifiesPS == true)
            {
                result.AppendLine("NVNullPS".Translate());
            }

            return result.ToString().Trim();
        }

        public override string CompInspectStringExtra()
        {
            var result = new StringBuilder(base.CompInspectStringExtra());

            if (Props?.AppVisionSetting?.GrantsNV == true)
            {
                result.AppendLine("NVGiveNV".Translate());
            }

            if (Props?.AppVisionSetting?.NullifiesPS == true)
            {
                result.AppendLine("NVNullPS".Translate());
            }

            return result.ToString().Trim();
        }
    }
}