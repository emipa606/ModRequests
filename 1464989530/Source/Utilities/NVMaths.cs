using System;

namespace NightVision
{
    public static class NVMaths
    {
        public static float ClampMod(float cap, float mod)
        {
            if (mod > cap == mod > 0)
            {
                if (cap > 0 == mod > 0)
                {
                    return cap;
                }
                return 0;
            }
            return mod;
        }

        public static float ClampMods(float baseMod, float nvMod, float psMod, float hdMod, float nvCap, float psCap, bool canCheat)
        {
            var mod = baseMod;
            mod += ClampMod(nvCap, nvMod) + ClampMod(psCap, psMod) + hdMod;
            if (!canCheat)
            {
                mod = Settings.Store.ClampToMultipliers(mod);
            }
            return (float)Math.Round(mod - baseMod,
                Constants.NUMBER_OF_DIGITS);

        }
    }
}