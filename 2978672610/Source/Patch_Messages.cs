using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using System.Reflection;

// colorize "gene extracted" messages
// hide "Unstable mutation gained" message
namespace zed_0xff.GeneCollectorQOL {
    // public static void Message(string text, LookTargets lookTargets, MessageTypeDef def, bool historical = true)
    [HarmonyPatch(typeof(Messages), nameof(Messages.Message), new Type[] {typeof(string), typeof(LookTargets), typeof(MessageTypeDef), typeof(bool)})]
    static class Patch_Message
    {
        public static float sat => 1.0f;
        public static Color yellow => new Color(sat, sat, 0);
        public static Color green  => new Color(0,   sat, 0);
        public static Color red    => new Color(sat, 0,   0);

        static string colorize(string text, Color c){
            if( text == null )
                return text;

            int pos = text.IndexOf(": ");
            if( pos != -1 ){
                text = text.Substring(0, pos+2) + text.Substring(pos+2).Colorize(c);
            }
            return text;
        }

        static bool Prefix(ref string text, LookTargets lookTargets, MessageTypeDef def, bool historical){
            if( def != MessageTypeDefOf.PositiveEvent )
                return true;

            if( ModConfig.Settings.patchAG && text.StartsWith("Unstable mutation gained by") )
                return false;

            if( ModConfig.Settings.patchGeneExtractor_colorize
                    && lookTargets != null
                    && lookTargets.targets.Count == 2
                    && lookTargets.targets[0].Thing is Pawn pawn
                    && lookTargets.targets[1].Thing is Genepack gp
                    && text.StartsWith("GeneExtractionComplete".Translate(pawn.Named("PAWN")))
              ){
                switch( GeneCache.GenepackStatus(gp) ){
                    case 0:
                        return false;
                    case 1:
                        text = colorize(text, red);
                        break;
                    case 2:
                        text = colorize(text, yellow);
                        break;
                }
            }

            return true;
        }
    }
}
