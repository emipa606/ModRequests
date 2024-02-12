using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class Hediff_Implant_MindSuppressor : Hediff_Implant
    {
        //passively reduces resistance of prisoner every few ticks
        public override void Tick()
        {
            //calls base method first
            base.Tick();
            //proceeds if wearer is a prisoner
            if (pawn.IsPrisoner)
            {
                //every long tick
                if (Find.TickManager.TicksGame % 6000 == 0)
                {
                    //has a chance of recruiting prisoner
                    if (pawn.guest.resistance == 0f)
                    {
                        float recruitChance = this.RecruitDifficulty(pawn);
                        if (Rand.Value < recruitChance)
                        {
                            //picks a random pawn to recruit because otherwise a new recruit method needs to be created; too much work
                            List<Pawn> allColonists = pawn.Map.mapPawns.FreeColonistsSpawned;
                            if (!allColonists.NullOrEmpty())
                            {
                                int randomIndex = Rand.Range(0, allColonists.Count - 1);
                                Pawn randomRecruiter = allColonists.ElementAt(randomIndex);
                                InteractionWorker_RecruitAttempt.DoRecruit(randomRecruiter, pawn, false);
                                //sends letter
                                string letterLabel = "LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap;
                                Find.LetterStack.ReceiveLetter(letterLabel, "MessageAutomaticRecruitSuccess".Translate(pawn.LabelShort, pawn, recruitChance.ToStringPercent()), LetterDefOf.PositiveEvent, pawn, null, null, null, null);
                            }
                        }   
                    }
                    //reduces resistance if not 0
                    else
                    {
                        pawn.guest.resistance = Mathf.Max(0f, (pawn.guest.resistance - ArtifactsExpandedMod.modSettings.mindSuppressorStrength));
                    }
                }
            }
        }

        public float RecruitDifficulty(Pawn recruitee)
        {
            float baseDifficulty = recruitee.kindDef.baseRecruitDifficulty;
            Rand.PushState();
            Rand.Seed = recruitee.HashOffset();
            float finalDifficulty = baseDifficulty + Rand.Gaussian(0f, 0.15f);
            Rand.PopState();
            if (recruitee.Faction != null)
            {
                finalDifficulty += Mathf.Abs(Mathf.Min((int)recruitee.Faction.def.techLevel, 4) - Mathf.Min((int)Faction.OfPlayer.def.techLevel, 4)) * 0.16f;
            }
            return Mathf.Clamp(finalDifficulty, 0.01f, 0.99f);
        }
    }
}