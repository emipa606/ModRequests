using Verse;

namespace zed_0xff.CPS;

public class CompProperties_TimeSpeed : CompProperties {
        public float ratio = 1;

        public CompProperties_TimeSpeed() {
            compClass = typeof(CompTimeSpeed);
        }
}
