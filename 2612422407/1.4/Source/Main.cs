using RimWorld;
using System;
//using System.Diagnostics;
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

            //Stopwatch sw = Stopwatch.StartNew();
            Tools.ReplaceWords(ref RealNamesInfiniteMod.Settings.Replacements);
            //sw.Stop();
            //UnityEngine.Debug.Log("Time taken: " + sw.Elapsed.TotalMilliseconds.ToString() + "ms");

        }

    }

    public static class Tools {

        public static void ReplaceWords(ref List<ReplacementSet> replacements) {
            foreach(Type defType in GenDefDatabase.AllDefTypesWithDatabases()) {

                if(defType.Name != "ThoughtDef") {
                    foreach(Def def in GenDefDatabase.GetAllDefsInDatabaseForDef(defType)) {

                        foreach(ReplacementSet set in replacements) {

                            if(set.IsEnabled) {
                                string o = set.Original;
                                string r = set.Replacement;

                                if(!string.IsNullOrEmpty(o) && !string.IsNullOrEmpty(r)) {
                                    if(!set.OnlyReplaceForDefWithSameLabel || def.label == o)
                                        ReplaceWordInDefWith(o, r, def);
                                }
                            }

                        }

                    }
                } else {
                    foreach(ThoughtDef thoughtDef in GenDefDatabase.GetAllDefsInDatabaseForDef(defType)) {

                        foreach(ReplacementSet set in replacements) {

                            if(set.IsEnabled && !set.OnlyReplaceForDefWithSameLabel) {
                                string o = set.Original;
                                string r = set.Replacement;

                                if(!string.IsNullOrEmpty(o) && !string.IsNullOrEmpty(r)) {
                                        ReplaceWordInThoughtDefWith(o, r, thoughtDef);
                                }
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

        static void ReplaceWordInThoughtDefWith(string word, string replacement, ThoughtDef def) {

            List<ThoughtStage> stages = def.stages;

            if(stages != null) {

                foreach(ThoughtStage stage in stages) {
                    
                    if(stage != null) {

                        string s = stage.label;
                        if(s != null) {
                            s = Regex.Replace(s, @"\b" + word + @"\b", replacement, RegexOptions.IgnoreCase);
                            stage.label = s;
                        }

                        s = stage.description;
                        if(s != null) {
                            s = Regex.Replace(s, @"\b" + word + @"\b", replacement);
                            s = Regex.Replace(s, @"\b" + FirstLetterToUpperCase(word) + @"\b", FirstLetterToUpperCase(replacement));
                            stage.description = s;
                        }

                    }

                }

            }

        }

        public static string FirstLetterToUpperCase(this string s) {
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

    }


}
