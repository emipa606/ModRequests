using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using WildPlantPicker.DataStore;
using WildPlantPicker.DataStore.Entities;
using WildPlantPicker.StaticResources;

namespace WildPlantPicker.Helper
{
    [StaticConstructorOnStartup]
    internal static class FloatMenuHelper
    {
        internal static void SimpleConfirmFloatMenu(string confirmMessage, Action confirmedAction = null)
        {
            FloatMenuUtility.MakeMenu(new int[] { 0 }, x => confirmMessage, x => () => { 
                if (confirmedAction != null)
                {
                    confirmedAction();
                }
            });
        }
        internal static void AutoHarvestTypeGetter(Action<AutoHarvestType> action)
        {
            List<Pair<AutoHarvestType, Action>> list = new List<Pair<AutoHarvestType, Action>>();
            foreach (AutoHarvestType t in Enum.GetValues(typeof(AutoHarvestType)))
            {
                list.Add(new Pair<AutoHarvestType, Action>(t, () => action(t)));
            }
            DataContext dc = DataContext.Current();
            FloatMenuUtility.MakeMenu(list, x => CommonHelper.TranslateEnum(x.First, dc).ToString(), x => () => x.Second());
        }
        internal static void TargetPlantAddGetter(IEnumerable<ThingDef> registed, Action<ThingDef> action)
        {
            List<Pair<ThingDef, Action>> list = new List<Pair<ThingDef, Action>>();
            HashSet<ThingDef> allowedPlants;
            DataContext dc = DataContext.Current();
            if (dc.m_OnlyStrictlyWildPlants)
            {
                allowedPlants = Defined.PURE_WILD_PLANTS;
            }
            else
            {
                allowedPlants = Defined.HARVESTABLE_PLANTS;
            }
            foreach (ThingDef allowedPlant in allowedPlants.Where(x => !registed.Contains(x)))
            {
                list.Add(new Pair<ThingDef, Action>(allowedPlant, () => action(allowedPlant)));
            }
            if (list.Any())
            {
                FloatMenuUtility.MakeMenu(list, x => x.First.LabelCap, x => () => x.Second());
            }
            else
            {
                SimpleConfirmFloatMenu("WPP_TargetPlantDontAdd".Translate());
            }
        }
        internal static void TargetPlantRemoveGetter(IEnumerable<TargetPlantCondition> registed, Action<TargetPlantCondition> action)
        {
            IEnumerable<Pair<ThingDef, Action>> removalPlants = registed.Select(x => x.m_PlantDef).Except(Defined.IMMUTABLE_PLANTS).Select(x => new Pair<ThingDef, Action>(x, () => action(registed.First(y => y.m_PlantDef == x))));
            if (removalPlants.Any())
            {
                FloatMenuUtility.MakeMenu(removalPlants, x => x.First.LabelCap, x => () => x.Second());
            }
            else
            {
                SimpleConfirmFloatMenu("WPP_TargetPlantDontRemove".Translate());
            }
        }
    }
}
