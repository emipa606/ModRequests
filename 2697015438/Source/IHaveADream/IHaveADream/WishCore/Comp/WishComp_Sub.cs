using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class WishComp_Sub : WishComp
    {
        public WishCompProperties_Sub Props => (WishCompProperties_Sub)props;

        public bool fulfilled = false;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref fulfilled, "fulfilled", false);
        }
        public override bool CompOnFulfill()
        {
            fulfilled = true;
            for (int i = 0; i < parent.comps.Count; i++)
            {
                if (parent.comps[i] == this) continue;
                if (!parent.comps[i].CompOnFulfill())
                {
                    fulfilled = false;
                    break;
                }
            }
            return false;
        }
        public override void CompPostAdd()
        {
            base.CompPostAdd();
            if (Props.preventWishMood)
            {
                parent.preventDebuff = true;
                parent.preventBuff = true;
            }
        }
        public override void CompPostChange(int value)
        {
            base.CompPostChange(value);
            if (value < 0 && fulfilled)
            {
                fulfilled = false;
            }
        }

        public override string CompDescription()
        {
            return "";
        }

    }
}
