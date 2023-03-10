using Verse;

namespace SurvivalistsAdditions {

  public sealed class ThingCountExposable : IExposable {

    public ThingDef thingDef;
    public int count;


    public ThingCountExposable() {
    }


    public ThingCountExposable(ThingDef thingDef, int count) {
      this.thingDef = thingDef;
      this.count = count;
    }


    public override string ToString() {
      return string.Concat(new object[]
      {
        "(",
        count,
        "x ",
        (thingDef == null) ? "null" : thingDef.defName,
        ")"
      });
    }


    public override int GetHashCode() {
      return (int)thingDef.shortHash + count << 16;
    }


    public void ExposeData() {
      Scribe_Defs.Look(ref thingDef, "thingDef");
      Scribe_Values.Look(ref count, "count", 0, false);
      if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && thingDef == null) {
        Log.Warning("Survivalist's Additions:: Failed to load ThingCount. Setting default.");
        thingDef = DefDatabase<ThingDef>.GetNamedSilentFail("Alpaca_Meat");
        count = (count <= 0) ? 10 : count;
      }
    }
  }
}
