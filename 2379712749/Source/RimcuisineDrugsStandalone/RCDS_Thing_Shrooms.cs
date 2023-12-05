using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

public class RCDS_Thing_Shrooms : ThingWithComps
{

    #region Overrides
    protected override void PostIngested(Pawn ingester)
    {



        //since lsd and shrooms in real life cure depression and can stop drug addictions, that's what these shrooms will do 

        if (ingester.IsColonist || ingester.def.ToString() == "Human")
        {
            //Log.Message("in if statement");

            /*if (ingester.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("PoopEater")))
            {
                ingester.needs.mood.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed("AtePoopEnjoyedIt"));
            }
            else
            {
                ingester.needs.mood.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed("AteStinkyPoop"));
            }*/
            List<Thought> thoughtList = new List<Thought>();
            ingester.needs.mood.thoughts.GetAllMoodThoughts(thoughtList);
            //Log.Message("generated thought list");
            foreach (Thought thought in thoughtList)
            {
                if (thought.def.defName == "RC2_ShroomsHigh" && thought.CurStageIndex == 4)
                {
                    Log.Message("Pawn has level 4 ShroomsHigh");
                    thought.CurStage.description = ingester.NameShortColored.ToString() + " has reached the rarest and most sacred of all highs achieved only by a select few grand - masters across the millenia. " + ingester.NameShortColored.ToString() + "'s high has become so motherfucking that " + ingester.gender.GetPossessive() + " collective conciousness may never return to the local galactic cluster again.";

                    Message newMessage = new Message("Test string", MessageTypeDefOf.PositiveEvent);
                    newMessage.lookTargets = ingester;
                    Messages.Message(newMessage);

                    TaggedString highnessTitle = ingester.NameShortColored + " is the highest person in the universe";
                    TaggedString highnessString = ingester.NameShortColored + " has reached the rarest and most sacred of all highs achieved only by a select few grand - masters across the millenia. " + ingester.NameShortColored + "'s high has become so motherfucking that " + ingester.gender.GetPossessive() + " collective conciousness may never return to the local galactic cluster again. Space and wisdom.";
                    highnessString += "\n\nNews of " + ingester.NameShortColored + "'s spectacular accomplishment will spread throughout the world.";
                    //highnessString += "\nMake sure to have someone take " + ingester.NameShortColored + " to a bed and feed " + ingester.gender.GetPossessive() + " as nessecary. As it can take quite a while for a high as legendary as this one to wear off.";
                    //DiaNode fuck = new DiaNode(highnessString);
                    //fuck.
                    //ChoiceLetter fuckyfuck = LetterMaker.MakeLetter(highnessTitle, highnessString, LetterDefOf.PositiveEvent);
                    Find.LetterStack.ReceiveLetter(highnessTitle, highnessString, LetterDefOf.PositiveEvent, ingester);
                    TaleRecorder.RecordTale(DefDatabase<TaleDef>.GetNamed("BecameTheHighestOne"), ingester);
                    Log.Message("added tale");

                    //because you can't just go around being a depressive asshole the rest of your life after a trip like that
                    //find if pawn has a depressive trait and if so, get rid of it
                    foreach (Trait currentTrait in ingester.story.traits.allTraits)
                    {
                        if (currentTrait.def == TraitDefOf.NaturalMood && currentTrait.Degree < 0)
                        {
                            ingester.story.traits.allTraits.Remove(currentTrait);

                            //too op
                            //ingester.story.traits.allTraits.Add(new Trait(TraitDefOf.NaturalMood, 2));

                            //because why not
                            break;
                        }
                    }

                    //lastly, add the "i was god once" thought
                    ingester.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("IWasGodOnce"));
                }

            }

           // Log.Message("finished foreach loop");

        }

    }

    #endregion
}
