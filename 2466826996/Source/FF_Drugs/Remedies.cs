using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FF_Drugs
{
        public sealed class FF_RemediesMod : Mod
        {
            public FF_RemediesMod(ModContentPack content) : base(content)
            {
                new Harmony("rimworld.freefolk.remedies").PatchAll();
            }
        }
}
