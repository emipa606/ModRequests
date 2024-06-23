using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace The_Amazing_Tree
{
    public class TheAmazingTree : Mod
    {
        public TheAmazingTree(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("TheAmazingTree");
            harmonyInstance.PatchAll();
        }
        public static Harmony harmonyInstance;

    }


}
