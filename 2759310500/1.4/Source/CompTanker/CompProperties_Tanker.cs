using Verse;

namespace CompTanker
{
    public class CompProperties_Tanker : CompProperties
    {
        public TankType contents = TankType.Invalid;
        public double storageCap = 10000;
        public double fillAmount = 0.5;
        public double drainAmount = 0.5;

        public CompProperties_Tanker() => compClass = typeof(CompTanker);
    }
}
