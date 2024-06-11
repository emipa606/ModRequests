using Reload.Defs;
using RimWorld;
using System;
using System.Globalization;
using UnityEngine;
using Verse;

namespace Reload.Data
{
    public class ReloadData
    {
        public string ammoDef;

        public int magazineCapacity;

        public int reloadTicks;

        public int loadsAmountPerReload;

        public int moveSpeedPenalty;

        public ReloadData()
        {
        }

        public ReloadData(ThingDef thingDef)
        {
            float cooldown = thingDef.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 1f);
            ammoDef = AmmoUtils.GetProperAmmo(thingDef).defName;
            magazineCapacity = 10;
            reloadTicks = (int)(cooldown * 60f);
            loadsAmountPerReload = 0;
            moveSpeedPenalty = (int)Mathf.Clamp(thingDef.BaseMass * 10f, 0f, 100f);
        }
        public override string ToString()
        {
            return $"({ammoDef}, {magazineCapacity}, {reloadTicks}, {loadsAmountPerReload}, {moveSpeedPenalty})";
        }
        public static ReloadData FromString(string str)
        {
            str = str.TrimStart('(');
            str = str.TrimEnd(')');
            string[] arr = str.Split(',');
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;

            ReloadData result = new ReloadData()
            {
                ammoDef = Convert.ToString(arr[0], invariantCulture),
                magazineCapacity = Convert.ToInt32(arr[1], invariantCulture),
                reloadTicks = Convert.ToInt32(arr[2], invariantCulture),
                loadsAmountPerReload = Convert.ToInt32(arr[3], invariantCulture),
                moveSpeedPenalty = Convert.ToInt32(arr[4], invariantCulture)
            };
            return result;
        }
        public void Copy(ReloadData source)
        {
            ammoDef = source.ammoDef;
            magazineCapacity = source.magazineCapacity;
            reloadTicks = source.reloadTicks;
            loadsAmountPerReload = source.loadsAmountPerReload;
            moveSpeedPenalty = source.moveSpeedPenalty;
        }
        public void Copy(ReloadPresetDef source)
        {
            ammoDef = source.ammoDef.defName;
            magazineCapacity = source.magazineCapacity;
            reloadTicks = source.reloadTicks;
            loadsAmountPerReload = source.loadsAmountPerReload;
            moveSpeedPenalty = source.moveSpeedPenalty;
        }
    }
}
