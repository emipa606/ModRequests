using Reload.Data;
using Reload.Defs;
using RimWorld;
using Verse;

namespace Reload.Components
{
    public class CompProperties_Reload : CompProperties
    {
        public AmmoDef ammoDef;

        public int magazineCapacity;

        public int baseReloadTicks;

        public int loadsAmountPerReload;

        public int moveSpeedPenalty;

        public SoundDef beginSound;

        public SoundDef reloadSound;

        public SoundDef endSound;

        public string LabelReloadSeconds => $"{baseReloadTicks / 60f:0.00}s";

        public int LoadAmount => loadsAmountPerReload <= 0 ? magazineCapacity : loadsAmountPerReload;

        public CompProperties_Reload()
        {
            compClass = typeof(CompReload);
        }
        public CompProperties_Reload(ReloadData data)
        {
            compClass = typeof(CompReload);
            CopyFrom(data);
        }

        public void CopyFrom(ReloadData reloadData)
        {
            if (reloadData.ammoDef == "R_Ammo_Industrial_Speical")
                reloadData.ammoDef = "R_Ammo_Industrial_Special";

            ammoDef = (AmmoDef)ThingDef.Named(reloadData.ammoDef);
            magazineCapacity = reloadData.magazineCapacity;
            baseReloadTicks = reloadData.reloadTicks;
            loadsAmountPerReload = reloadData.loadsAmountPerReload;
            moveSpeedPenalty = reloadData.moveSpeedPenalty;
        }
    }
}
