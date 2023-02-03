using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBTools.RequiredTraitFramework
{
    public class RequiredTrait_Utils
    {
         
      

        private static bool HasAllTraitsFromList(Pawn pawn,List<TraitDef> traitDefs)
        {            

            foreach(TraitDef td in traitDefs.OrEmptyIfNull())
            {
                if (!pawn.story.traits.HasTrait(td))
                {
                    return false;
                }
            }
            
            return true;
        }

        // CORE FUNCTIONS
        private static bool HasRestrictedTrait(Pawn pawn,List<TraitDef> traitDefs)
        {            
            foreach(TraitDef td in traitDefs)
            {
                if(pawn.story.traits.HasTrait(td))
                {
                    return true;
                }
            }
            return false;
            
            
        }

        private static bool HasAllRequiredTraits(Pawn pawn,List<TraitDef> traitDefs)
        {
            foreach(TraitDef td in traitDefs)
            {
                if (!pawn.story.traits.HasTrait(td))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool HasOneRequiredTrait(Pawn pawn,List<TraitDef> traitDefs)
        {
            foreach(TraitDef td in traitDefs)
            {
                if (pawn.story.traits.HasTrait(td))
                {
                    return true;
                }
            }
            return false;
        }


        // CORE FUNCTIONS LAYER 2

        /// <summary>
        /// Returns true if <paramref name="pawn"/> has no <paramref name="forbidden"/> traits, all <paramref name="reqAll"/> traits, and at least one <paramref name="reqOne"/> trait. Shorthand for overload with a last boolean argument assuming the user put false.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="forbidden"></param>
        /// <param name="reqAll"></param>
        /// <param name="reqOne"></param>
        /// <returns></returns>
        public static bool IsAllowedByTraits(Pawn pawn,List<TraitDef> forbidden,List<TraitDef> reqAll,List<TraitDef> reqOne)
        {
            bool hasForbidden = false;
            bool hasAll = true;
            bool hasOne = true;

            if (forbidden.OrEmptyIfNull().Any())
            {
                hasForbidden = HasRestrictedTrait(pawn,forbidden);
            }

            if (reqAll.OrEmptyIfNull().Any())
            {
                hasAll = HasRestrictedTrait(pawn,reqAll);
            }

            if (reqOne.OrEmptyIfNull().Any())
            {
                hasOne = HasRestrictedTrait(pawn,reqOne);
            }

            return !hasForbidden && hasAll && hasOne;
        }
        
        /// <summary>
        /// Returns true if <paramref name="pawn"/> has no <paramref name="forbidden"/> traits, all <paramref name="reqAll"/> traits, and at least one <paramref name="reqOne"/> trait. Ignore this last requirement if <paramref name="ignoreRequiredOneTraits"/> is true.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="forbidden"></param>
        /// <param name="reqAll"></param>
        /// <param name="reqOne"></param>
        /// <param name="ignoreRequiredOneTraits"></param>
        /// <returns></returns>
        public static bool IsAllowedByTraits(Pawn pawn,List<TraitDef> forbidden,List<TraitDef> reqAll,List<TraitDef> reqOne,bool ignoreRequiredOneTraits)
        {
            return !HasRestrictedTrait(pawn,forbidden) && HasAllRequiredTraits(pawn,reqAll) && (ignoreRequiredOneTraits || HasOneRequiredTrait(pawn,reqOne));
        }


        // Public functions
        /// <summary>
        /// Given a <c>JoyGiver</c> and <c>Pawn</c>, returns the complete modifier for the float return value of the GetChance() method from the <c>JoyGiver</c> instance. Includes both the forbid/allow and increase/decrease chance modifiers.
        /// </summary>
        /// <param name="joy"></param>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static float GetChanceModifierFromTraits(JoyGiver joy,Pawn pawn)
        {
            TraitListBuilder(joy);

            return GetIncDecChanceModifierFromTraits(pawn,inc,dec) * IsAllowedByTraits(pawn,forbidden,reqAll,reqOne).AsInt();
        }
        /// <summary>
        /// Given a <c>JoyGiver_TakeDrug</c> and <c>Pawn</c>, returns the complete modifier for the float return value of the GetChance() method from the <c>JoyGiver_TakeDrug</c> instance. Includes both the forbid/allow and increase/decrease chance modifiers.
        /// </summary>
        /// <param name="joy"></param>
        /// <param name="pawn"></param>
        /// <returns></returns>
         public static float GetChanceModifierFromTraits(JoyGiver_TakeDrug joy,Pawn pawn)
        {
                  
            TraitListBuilder(joy);


            return GetIncDecChanceModifierFromTraits(pawn,inc,dec) * IsAllowedByTraits(pawn,forbidden,reqAll,reqOne).AsInt();
        }
        /// <summary>
        /// Given a <c>JoyGiver_Skygaze</c> and <c>Pawn</c>, returns the complete modifier for the float return value of the GetChance() method from the <c>JoyGiver_Skygaze</c> instance. Includes both the forbid/allow and increase/decrease chance modifiers.
        /// </summary>
        /// <param name="joy"></param>
        /// <param name="pawn"></param>
        /// <returns></returns>
         public static float GetChanceModifierFromTraits(JoyGiver_Skygaze joy,Pawn pawn)
        {
            TraitListBuilder(joy);

            return GetIncDecChanceModifierFromTraits(pawn,inc,dec) * IsAllowedByTraits(pawn,forbidden,reqAll,reqOne).AsInt();
        }


        // helper functions to the above


        /// <summary>
        /// Gets the increase or decrease modifier for the GetChance() method based on the traits contained within <paramref name="pawn"/>'s <paramref name="inc"/> and <paramref name="dec"/> <c>TraitDef</c> lists. Doubles the modifier per trait in <paramref name="inc"/> and halves per trait in <paramref name="dec"/>.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="inc"></param>
        /// <param name="dec"></param>
        /// <returns></returns>
        public static float GetIncDecChanceModifierFromTraits(Pawn pawn,List<TraitDef> inc,List<TraitDef> dec)
        {
            float mod = 1;
            if (inc != null)
            {
                foreach(TraitDef td in inc)
                {
                    if (pawn.story.traits.HasTrait(td))
                    {
                        mod *= 2f;
                    }
                }
            }
            if (dec != null)
            {
                foreach(TraitDef td in dec)
                {
                    if (pawn.story.traits.HasTrait(td))
                    {
                        mod *= .5f;
                    }
                }
            }
            return mod;
        }


        private static void TraitListBuilder(JoyGiver joy)
        {
            if(joy.def.HasModExtension<RequiredTrait_ModExtension>())
            {
                reqAll      = joy.def.GetModExtension<RequiredTrait_ModExtension>().requiredTraitsAll.OrEmptyIfNull();
                reqOne      = joy.def.GetModExtension<RequiredTrait_ModExtension>().requiredTraitsOne.OrEmptyIfNull();
                forbidden   = joy.def.GetModExtension<RequiredTrait_ModExtension>().forbiddenTraits.OrEmptyIfNull();
                inc         = joy.def.GetModExtension<RequiredTrait_ModExtension>().increaseChanceTraits.OrEmptyIfNull();
                dec         = joy.def.GetModExtension<RequiredTrait_ModExtension>().decreaseChanceTraits.OrEmptyIfNull();
            }
        }
        private static void TraitListBuilder(JoyGiver_Skygaze joy)
        {
            if(joy.def.HasModExtension<RequiredTrait_ModExtension>())
            {
                reqAll      = joy.def.GetModExtension<RequiredTrait_ModExtension>().requiredTraitsAll.OrEmptyIfNull();
                reqOne      = joy.def.GetModExtension<RequiredTrait_ModExtension>().requiredTraitsOne.OrEmptyIfNull();
                forbidden   = joy.def.GetModExtension<RequiredTrait_ModExtension>().forbiddenTraits.OrEmptyIfNull();
                inc         = joy.def.GetModExtension<RequiredTrait_ModExtension>().increaseChanceTraits.OrEmptyIfNull();
                dec         = joy.def.GetModExtension<RequiredTrait_ModExtension>().decreaseChanceTraits.OrEmptyIfNull();
            }
        }
        private static void TraitListBuilder(JoyGiver_TakeDrug joy)
        {
            if(joy.def.HasModExtension<RequiredTrait_ModExtension>())
            {
                reqAll      = joy.def.GetModExtension<RequiredTrait_ModExtension>().requiredTraitsAll.OrEmptyIfNull();
                reqOne      = joy.def.GetModExtension<RequiredTrait_ModExtension>().requiredTraitsOne.OrEmptyIfNull();
                forbidden   = joy.def.GetModExtension<RequiredTrait_ModExtension>().forbiddenTraits.OrEmptyIfNull();
                inc         = joy.def.GetModExtension<RequiredTrait_ModExtension>().increaseChanceTraits.OrEmptyIfNull();
                dec         = joy.def.GetModExtension<RequiredTrait_ModExtension>().decreaseChanceTraits.OrEmptyIfNull();
            }
        }

        //class lists
        private static List<TraitDef> forbidden;
        private static List<TraitDef> reqAll;    
        private static List<TraitDef> reqOne;
        private static List<TraitDef> inc;
        private static List<TraitDef> dec;
    }
}
