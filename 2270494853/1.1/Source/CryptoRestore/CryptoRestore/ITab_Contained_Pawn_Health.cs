using RimWorld;
using UnityEngine;
using Verse;

namespace CryptoRestore
{
    public class ITab_Contained_Pawn_Health : ITab
    {                                                            
        private Pawn PawnForHealth
        {
            get
            {                          
                return (base.SelThing as Building_CryptoRestore)?.ContainedThing as Pawn;
            }
        }

        public ITab_Contained_Pawn_Health()
        {
            size = new Vector2(630f, 430f);
            labelKey = "TabHealth";
            tutorTag = "Health";
        }

        protected override void FillTab()
        {
            Pawn pawnForHealth = PawnForHealth;
            if (pawnForHealth == null)
            {
                DrawTabNoPawn();
            }
            else
            {
                HealthCardUtility.DrawPawnHealthCard(new Rect(0f, 20f, size.x, size.y - 20f), pawnForHealth, false, true, pawnForHealth);
            }  
        }

        private void DrawTabNoPawn()
        {                                 
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.grey;
            Widgets.Label(new Rect(0f, 0f, size.x, size.y), "(" + "CasketContains".Translate() + ": " + "UnknownLower".Translate() + ")");
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }
}