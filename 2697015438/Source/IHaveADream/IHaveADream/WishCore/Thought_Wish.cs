using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
    public class Thought_Wish : Thought_Memory
    {

        public WishDef fromWish;
        public bool groupPerWish = false;
        public string wishDesc = "";

        public override bool GroupsWith(Thought other)
        {
            return other == this
                || (base.GroupsWith(other)
                    && fromWish != null
                    && groupPerWish
                    && ((other as Thought_Wish)?.fromWish == fromWish)
                    && CurStageIndex == other.CurStageIndex);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref groupPerWish, "groupPerWish", false);
            Scribe_Values.Look(ref wishDesc, "wishDesc", "");
            Scribe_Defs.Look(ref fromWish, "fromWish");
        }

        public override string LabelCap
        {
            get
            {
                return WishUtility.FormateTierKeyword(base.LabelCap, CurStageIndex);
            }
        }

        public override string Description
        {
            get
            {
                return WishUtility.FormateTierKeyword(base.Description, CurStageIndex)
                    + (fromWish == null ? "" : "\n\n (" + wishDesc + ")");
            }
        }
    }
}
