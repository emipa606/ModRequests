using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class CompUseEffect_ForgetBadThoughts : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            //calls base method first
            base.DoEffect(usedBy);
            //checks for negative memory-related thougts and removes them
            Pawn_NeedsTracker userNeeds = usedBy.needs;
            if (userNeeds != null)
            {
                Need_Mood userMood = userNeeds.mood;
                if (userMood != null)
                {
                    MemoryThoughtHandler userMemoryHandler = userMood.thoughts.memories;
                    if (userMemoryHandler != null)
                    {
                        List<Thought_Memory> userMemories = userMemoryHandler.Memories;
                        List<Thought_Memory> thoughtsToRemove = new List<Thought_Memory>();
                        if (!userMemories.NullOrEmpty())
                        {
                            foreach (Thought_Memory currentThought in userMemories)
                            {
                                if (currentThought.MoodOffset() < 0)
                                {
                                    thoughtsToRemove.Add(currentThought);
                                }
                            }
                            foreach (Thought_Memory currentMemoryToRemove in thoughtsToRemove)
                            {
                                userMemoryHandler.RemoveMemory(currentMemoryToRemove);
                            }
                        }
                        //gives worry-free thought
                        userMemoryHandler.TryGainMemory(ThoughtDefOf.WorryFree, null);
                    }
                }
            }
        }
    }
}
