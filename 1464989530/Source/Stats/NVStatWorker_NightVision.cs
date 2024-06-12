#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using JetBrains.Annotations;

namespace NightVision
{
    [UsedImplicitly]
    public class NVStatWorker_NightVision : NVStatWorker
    {
        public NVStatWorker_NightVision()
        {
            this.Glow = 0f;
            this.StatEffectMask = ApparelFlags.GrantsNV;
            this.DefaultStatValue = Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER;
            this.Acronym = Str.NV;
        }
    }
}