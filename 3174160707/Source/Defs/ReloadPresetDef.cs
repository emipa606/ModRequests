using RimWorld;
using Verse;

namespace Reload.Defs
{
    public class ReloadPresetDef : Def
    {
        public bool ignore = false;

        public AmmoDef ammoDef;

        public int magazineCapacity;

        public int reloadTicks;

        public int loadsAmountPerReload = 0;

        public int moveSpeedPenalty;

        public SoundDef beginSound;

        public SoundDef reloadSound;

        public SoundDef endSound;

        public bool needsAmmoToReload = true;

        public bool disableFeature = false;
    }
}