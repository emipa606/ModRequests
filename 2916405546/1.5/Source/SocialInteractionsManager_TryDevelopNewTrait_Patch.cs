using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using VanillaSocialInteractionsExpanded;
using Verse;

namespace VSIERationalTraitDevelopment
{

    [HarmonyPatch(typeof(SocialInteractionsManager), "TryDevelopNewTrait")]
    public static class SocialInteractionsManager_TryDevelopNewTrait_Patch
    {
        private static List<BackstoryTrait> _backstoryTraits;
        public static List<BackstoryTrait> AllTraits
        {
            get
            {
                if (_backstoryTraits is null)
                {
                    _backstoryTraits = new List<BackstoryTrait>();
                    foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
                    {
                        foreach (var degreeData in traitDef.degreeDatas)
                        {
                            _backstoryTraits.Add(new BackstoryTrait
                            {
                                def = traitDef,
                                degree = degreeData.degree
                            });
                        }
                    }
                }
                return _backstoryTraits;
            }
        }
        public static bool Prefix(Pawn pawn, string letterTextKey)
        {
            if (pawn.RaceProps.Humanlike && pawn.Faction == Faction.OfPlayer && !pawn.Dead)
            {
                var traits = DefDatabase<TraitDef>.AllDefsListForReading;
                var maxTraitsCount = VSIERationalTraitDevelopmentSettings.maxCountOfTraitsOfPools;
                var comp = Current.Game.GetComponent<SocialInteractionsManager>();
                var triesCount = maxTraitsCount * 100;
                var traitChoices = new List<Trait>();
                var traitPool = DefDatabase<TraitPoolDef>.AllDefs.FirstOrDefault(x => x.key == letterTextKey);
                if (traitPool != null)
                {
                    var validTraits = traitPool.allTraitsIncluded ? AllTraits : traitPool.traits.Where(x => x?.def != null).ToList();
                    while (triesCount > 0 && validTraits.Any())
                    {
                        triesCount--;
                        var te = validTraits.RandomElement();
                        var newTrait = new Trait(te.def, te.degree);
                        if (comp.TraitIsAllowed(pawn, te.def, te.degree) 
                            && traitChoices.Exists(x => x.def == te.def && x.degree == te.degree) is false)
                        {
                            traitChoices.Add(newTrait);
                            validTraits.Remove(te);
                        }
                        if (traitChoices.Count >= maxTraitsCount)
                        {
                            break;
                        }
                    }

                    if (traitChoices.Any())
                    {
                        if (VSIERationalTraitDevelopmentSettings.randomSelectionInsteadOfUI)
                        {
                            Dialog_TraitChoices.GainTrait(pawn, traitChoices.RandomElement(), letterTextKey);
                        }
                        else
                        {
                            Find.WindowStack.Add(new Dialog_TraitChoices(pawn, letterTextKey, traitChoices));
                        }
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
