using RimWorld;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Verse;

namespace RealNamesInfinite {

    [StaticConstructorOnStartup]
    internal static class RealNamesInfinite_Initializer {

        static RealNamesInfinite_Initializer() {
            LongEventHandler.QueueLongEvent(Setup, "LibraryStartup", false, null);
        }

        public static void Setup() {

            //RealNamesInfiniteMod.Settings.FirstTime = true; //test firstTime experience

            if(RealNamesInfiniteMod.Settings.FirstTime) {
                RealNamesInfiniteMod.Settings.ResetToDefaults();
                RealNamesInfiniteMod.Settings.FirstTime = false;
                //RealNamesInfiniteMod.Settings.Write(); //not needed since ResetToDefaults is called until FirstTime = false is writtne, and it isn't until the settings page is opened.
            }

            Tools.ReplaceWords(ref RealNamesInfiniteMod.Settings.Replacements);

        }

    }

    public static class Tools {

        public static void ReplaceWords(ref List<ReplacementSet> replacements) {
            foreach(Type defType in GenDefDatabase.AllDefTypesWithDatabases()) {
                foreach(Def def in GenDefDatabase.GetAllDefsInDatabaseForDef(defType)) {

                    foreach(ReplacementSet set in replacements) {
                        if(set.IsEnabled) {
                            string o = set.Original;
                            string r = set.Replacement;

                            if(!string.IsNullOrEmpty(o) && !string.IsNullOrEmpty(r)) {
                                if(!set.OnlyReplaceForDefWithSameLabel || def.label == o)
                                    ReplaceWordInDefWith(set.Original, set.Replacement, def);
                            }
                        }
                    }

                }
            }
        }

        static void ReplaceWordInDefWith(string word, string replacement, Def def) {

            if(def.label != null) {
                def.label = Regex.Replace(def.label, @"\b" + word + @"\b", replacement, RegexOptions.IgnoreCase);
            }

            string s = def.description;
            if(s != null) {
                s = Regex.Replace(s, @"\b" + word + @"\b", replacement);
                s = Regex.Replace(s, @"\b" + FirstLetterToUpperCase(word) + @"\b", FirstLetterToUpperCase(replacement));
                def.description = s;
            }

        }

        public static string FirstLetterToUpperCase(this string s) {
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

    }


}
