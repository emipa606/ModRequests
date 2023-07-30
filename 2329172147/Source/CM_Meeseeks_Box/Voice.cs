using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public class Voice : IExposable
    {
        public float pitch = 1.0f;
        public int volume = 25;

        public Voice()
        {
            pitch = (Rand.Value * 0.1f) + 0.95f;
            volume = Rand.RangeInclusive(80, 100);
        }

        public void ExposeData()
        {
            Scribe_Values.Look<float>(ref this.pitch, "pitch", 1.0f);
            Scribe_Values.Look<int>(ref this.volume, "volume", 90);
        }

        public override string ToString()
        {
            return string.Format("voice: [pitch: {0}, volume: {1}]", pitch, volume);
        }
    }
}
