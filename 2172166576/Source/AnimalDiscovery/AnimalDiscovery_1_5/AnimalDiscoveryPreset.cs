using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace AnimalDiscovery
{
    public class AnimalDiscoveryPreset : IExposable
    {
        public bool IsValid => !animalList.NullOrEmpty();

        public AnimalDiscoveryPreset()
        {

        }

        public AnimalDiscoveryPreset(string name)
        {
            this.name = name;
            SetPrest();
        }

        public void SetPrest()
        {
            animalList.Clear();
            foreach (AnimalAlertSettingItem item in AnimalDiscoveryMod.Settings.SettingItems)
            {
                if (item.def == null)
                {
                    item.ResolveDef();
                }
                animalList.Add(item);
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref this.name, "name", null, false);
            Scribe_Collections.Look<AnimalAlertSettingItem>(ref animalList, "animalList", LookMode.Deep, Array.Empty<object>());
        }

        public string GetToolTip()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (AnimalAlertSettingItem item in animalList)
            {
                item.ResolveDef();
                stringBuilder.AppendLine(item.Label);
            }
            return stringBuilder.ToString();
        }

        public string name;

        public List<AnimalAlertSettingItem> animalList = new List<AnimalAlertSettingItem>();
    }
}
