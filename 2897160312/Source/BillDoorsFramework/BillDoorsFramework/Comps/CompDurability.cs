using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class CompDurability : ThingComp
    {
        public CompProperties_Durability Props => props as CompProperties_Durability;
        public float Chance
        {
            get
            {
                if (parent.TryGetComp<CompQuality>() is CompQuality comp)
                {
                    switch (comp.Quality)
                    {
                        case QualityCategory.Awful:
                            return Props.chance * 1.5f;
                        case QualityCategory.Poor:
                            return Props.chance * 1.2f;
                        case QualityCategory.Normal:
                            return Props.chance * 1;
                        case QualityCategory.Good:
                            return Props.chance * 0.9f;
                        case QualityCategory.Excellent:
                            return Props.chance * 0.85f;
                        case QualityCategory.Masterwork:
                            return Props.chance * 0.5f;
                        case QualityCategory.Legendary:
                            return Props.chance * 0.1f;
                    }

                }
                return Props.chance;
            }
        }
    }

    public class CompProperties_Durability : CompProperties
    {
        public CompProperties_Durability()
        {
            compClass = typeof(CompDurability);
        }
        public int time;
        public float chance;
        public bool melee;
        public bool range;
    }
}
