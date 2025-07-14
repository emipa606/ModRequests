using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VSIERationalTraitDevelopment
{
    public class TraitPoolDef : Def
    {
        public string key;
        public List<BackstoryTrait> traits;
        public bool allTraitsIncluded;
    }
}
