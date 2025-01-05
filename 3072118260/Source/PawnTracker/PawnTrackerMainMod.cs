using System;
using System.IO;
using UnityEngine;
using Verse;

namespace PawnTrackerMain
{
    public class PawnTrackerMainMod : Mod
    {
        private static ModContentPack PawnTrackerMainContent;
        public static string RootDirectory => PawnTrackerMainContent.RootDir; 
        public PawnTrackerMainMod(ModContentPack content) : base(content)
        {
            PawnTrackerMainContent = content;
        }
    }
}
