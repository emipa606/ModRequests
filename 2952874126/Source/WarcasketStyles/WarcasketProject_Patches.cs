using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using RimWorld;
using VFECore;
using VFEPirates;

namespace WarcasketStyles
{

    [HarmonyPatch(typeof(WarcasketProject), "ApplyOn")]
    public class WarcasketProject_ApplyOn_Patch
    {
        public static void Postfix(ref Pawn pawn)
        {
            if (ModLister.IdeologyInstalled)
            {
                if (pawn.ideo == null)
                {
                    Log.Message("debug ideo is null for " + pawn.Name.ToStringShort);
                    return;
                }
                List<Apparel> warcasketParts = pawn.apparel.WornApparel.Where(x => x is Apparel_Warcasket).ToList() ?? null;
                
                foreach (var casketPart in warcasketParts)
                {
                    if (casketPart.def.CanBeStyled())
                    {
                        ThingStyleDef style = pawn.Ideo.GetStyleCategoryFor(casketPart.def)?.GetStyleForThingDef(casketPart.def);
                        if (style != null)
                        {
                            casketPart.StyleDef = style;
                            pawn.apparel.Notify_ApparelChanged();
                        }

                    }
                    
                }
            }

        }
    }
}
