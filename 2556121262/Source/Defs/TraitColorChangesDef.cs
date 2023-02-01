using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace HalfDragons
{
    public class TraitColorChangesDef : Def
    {
        public List<TraitColorChange> colorChanges = null;

        public void SetHairColor(Pawn pawn)
        {
            if(TryGetColor(pawn, out Color color))
            {
                if(pawn.story != null)
                {
                    pawn.story.hairColor = color;
                }
            }
        }

        public bool TryGetColor(Pawn pawn, out Color color)
        {
            color = default(Color);
            List<TraitDef> pawnTraits = pawn.story?.traits?.allTraits?.ConvertAll(trait => trait.def);
            if (pawnTraits.NullOrEmpty())
            {
                return false;
            }
            List<Color> colors = new List<Color>();
            foreach(TraitDef trait in pawnTraits)
            {
                if (HasOverrideFor(trait))
                {
                    colors.Add(GetOverridenColor(trait));
                }
            }
            if (colors.NullOrEmpty())
            {
                return false;
            }
            color = colors.RandomElement();
            return true;
        }

        public bool HasOverrideFor(TraitDef trait)
        {
            if (colorChanges.NullOrEmpty())
            {
                return false;
            }
            return colorChanges.Any(change => change.trait == trait);
        }
        public bool HasOverrideFor(Trait trait)
        {
            return HasOverrideFor(trait.def);
        }

        public Color GetOverridenColor(TraitDef trait)
        {
            return colorChanges.Find(change => change.trait == trait).color;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (colorChanges.NullOrEmpty())
            {
                yield return "Required list \"colorChanges\" null or empty";
            }
            else
            {
                foreach(TraitColorChange change in colorChanges)
                {
                    foreach (string error in change.ConfigErrors())
                    {
                        yield return error;
                    }
                }
            }
        }
    }
    public class TraitColorChange
    {
        public TraitDef trait;
        public Color color;
        public IEnumerable<string> ConfigErrors()
        {
            if(trait == default(TraitDef))
            {
                yield return "Required field \"trait\" not set";
            }
        }
    }
}
