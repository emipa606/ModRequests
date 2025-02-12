﻿using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Thank you to RunAndGun author "roolo" on Steam for allowing me to use his code!
/// Anything below this point is his code merely slightly changed to work with my assembly!
/// </summary>

namespace UseYourGun
{
    class DictWeaponRecordHandler : SettingHandleConvertible
    {
        public Dictionary<string, WeaponRecord> inner = new Dictionary<string, WeaponRecord>();
        public Dictionary<string, WeaponRecord> InnerList { get { return inner; } set { inner = value; } }

        public override void FromString(string settingValue)
        {
            inner = new Dictionary<string, WeaponRecord>();
            if (!settingValue.Equals(string.Empty))
            {
                foreach (string str in settingValue.Split('|'))
                {
                    string[] split = str.Split(',');
                    if (split.Count() < 4) //ensures that it works for users that still have old WeaponRecords saved. 
                    {
                        inner.Add(str.Split(',')[0], new WeaponRecord(Convert.ToBoolean(str.Split(',')[1]), Convert.ToBoolean(str.Split(',')[2]), ""));
                    }
                    else
                    {
                        inner.Add(str.Split(',')[0], new WeaponRecord(Convert.ToBoolean(str.Split(',')[1]), Convert.ToBoolean(str.Split(',')[2]), str.Split(',')[3]));
                    }
                }
            }
        }

        public override string ToString()
        {
            List<string> strings = new List<string>();
            foreach (KeyValuePair<string, WeaponRecord> item in inner)
            {
                strings.Add(item.Key + "," + item.Value.ToString());
            }

            return inner != null ? string.Join("|", strings.ToArray()) : "";
        }
    }
}
