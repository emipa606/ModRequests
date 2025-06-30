using Verse;

namespace MakeAnythingCraftable
{
    public class DefCount<T> : IExposable where T : Def
    {
        public string defName;
        public int count;
        public DefCount()
        {
        }
        public DefCount(string defName, int count)
        {
            this.defName = defName;
            this.count = count;
        }
        public T Def => DefDatabase<T>.GetNamedSilentFail(defName);
        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref count, "count", 1);
        }
    }
}
