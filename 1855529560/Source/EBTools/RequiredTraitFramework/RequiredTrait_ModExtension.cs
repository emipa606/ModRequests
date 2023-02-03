using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBTools.RequiredTraitFramework
{
    class RequiredTrait_ModExtension : DefModExtension
    {
        public List<TraitDef> requiredTraitsAll;
        public List<TraitDef> requiredTraitsOne;
        public List<TraitDef> forbiddenTraits;
        public List<TraitDef> increaseChanceTraits;
        public List<TraitDef> decreaseChanceTraits;
    }
}
