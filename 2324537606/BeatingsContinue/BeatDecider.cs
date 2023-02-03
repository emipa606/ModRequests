using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BeatingsContinue
{
    class BeatDecider
    {
        public static bool shouldStopBeating(Pawn beater, Pawn beatee)
        {
            return shouldStopBeating(beater) || shouldStopBeating(beatee);
        }

        public static bool shouldStopBeating(Pawn pawn)
        {
            if (pawn.IsColonistPlayerControlled)
            {
                return hasEndangeredPart(pawn);
            }
            else if (pawn.IsPrisonerOfColony)
            {
                string recruitMode = pawn.guest.interactionMode.defName;
                if (recruitMode == "AttemptRecruit")
                {
                    return hasEndangeredPart(pawn);
                }
                else if (recruitMode == "ReduceResistance" || recruitMode == "NoInteraction")
                {
                    return hasEndangeredPart(pawn, true);
                }
            }
            return true;
        }

        private static bool hasEndangeredPart(Pawn p, bool onlyNecessary = false)
        {
            foreach (BodyPartRecord bpr in p.health.hediffSet.GetInjuredParts().ToList().ListFullCopy())
            {
                if (p.health.hediffSet.GetPartHealth(bpr) < 11)
                {
                    foreach (Hediff h in p.health.hediffSet.hediffs.ListFullCopy())
                    {
                        if (h.Part == bpr && !h.IsPermanent())
                        {
                            if (onlyNecessary)
                            {
                                if (p.health.WouldDieAfterAddingHediff(DefDatabase<HediffDef>.GetNamed("Bruise"), bpr, 11))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
