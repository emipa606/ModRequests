using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using FloatMenuWrapper;
using System.Text.RegularExpressions;

namespace TakeOffIndoor
{
    public class TOCPreset : IExposable
    {
        public string name;
        public TakeOffData data;
        public List<RuleFilter> filters = new List<RuleFilter>();
        public void ExposeData()
        {
            if (name != "")
            {
                Scribe_Values.Look(ref name, "Name", "");
                Scribe_Deep.Look(ref data, "Data", null);
                Scribe_Collections.Look(ref filters, "Filters",LookMode.Deep,null);
            }
        }

        static Regex namePat = new Regex(@"^newPreset(\d+)$");

        public TOCPreset(string Name,TakeOffData Data,List<RuleFilter> Filters)
        {
            name = Name;
            data = Data;
            filters = Filters;
        }
        public TOCPreset()
        {
            if (TakeOffSettings.presets != null)
            {
                if (TakeOffSettings.presets.Count > 0)
                {
                    int cnt = 1;
                    foreach (TOCPreset preset in TakeOffSettings.presets)
                    {
                        if (preset != null)
                        {
                            if (namePat.Match(preset.name).Success)
                            {
                                string tmps = namePat.Replace(preset.name, "$1"); int tmpi;
                                if (Int32.TryParse(tmps, out tmpi))
                                {
                                    if (cnt <= tmpi)
                                    {
                                        cnt = tmpi + 1;
                                    }
                                }
                            }
                        }
                    }
                    name = "newPreset" + cnt.ToString("0");
                }
                else
                {
                    name = "newPreset1";
                }
            }
            name = "newPreset";

            data = new TakeOffData();
        }

        private static TOCPreset _none;
        private static bool noneInit = false;
        public static TOCPreset None
        {
            get
            {
                if (!noneInit)
                {
                    noneInit = true;
                    _none = new TOCPreset();
                    _none.data = TakeOffSettings.data;
                    _none.name = "None";
                }
                return _none;
            }
        }



        public bool Evaluate(Pawn pawn)
        {
            bool flg = true;

            //debug.WriteLine("evalute pawn:" + (pawn.Name as NameTriple).First);


            if (filters == null)
            {
                return false;

            }
            foreach (RuleFilter filter in filters)
            {
                if (filter != null)
                {
                    flg = filter.Evaluate(pawn, flg);
                }
            }


            //debug.WriteLine("evalute result:" + flg.ToString());
            return flg;

        }

        public static TOCPreset GetMatchData(Pawn pawn)
        {
            if (TakeOffSettings.presets != null)
            {
                foreach (TOCPreset preset in TakeOffSettings.presets)
                {
                    if (preset.Evaluate(pawn))
                    {
                        return preset;
                    }
                }
            }

            return None;
        }
        public static TOCPreset GetName(string presetName)
        {
            if (presetName == None.name)
            {
                return None;
            }
            foreach (TOCPreset preset in TakeOffSettings.presets)
            {
                if (preset.name==presetName)
                {
                    return preset;
                }
            }

            return null;
        }


    }
}
