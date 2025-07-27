using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace CM_Callouts
{
    public abstract class TextMoteQueue
    {
        public IntVec3 position = IntVec3.Invalid;
        public Map map = null;
        public List<MoteText> queuedMotes = new List<MoteText>();


        public TextMoteQueue(IntVec3 aPosition, Map aMap)
        {
            position = aPosition;
            map = aMap;
        }

        public bool Valid()
        {
            return (position != IntVec3.Invalid && map != null && !map.info.parent.Destroyed);
        }

        public abstract void AddMote(MoteText newMote);

        public abstract bool Update();
    }

    public class TextMoteQueueTickBased : TextMoteQueue
    {
        public const int delayTicks = 30;
        public int lastReleaseTick = -1;

        public TextMoteQueueTickBased(IntVec3 aPosition, Map aMap) : base(aPosition, aMap)
        {

        }

        public override void AddMote(MoteText newMote)
        {
            newMote.def = CalloutDefOf.CM_Callouts_Thing_Mote_Text_Ticked;

            int currentTick = Find.TickManager.TicksGame;

            if (queuedMotes.Count == 0 && currentTick > lastReleaseTick + delayTicks && Valid())
            {
                lastReleaseTick = currentTick;
                GenSpawn.Spawn(newMote, position, map);
            }
            else
            {
                queuedMotes.Add(newMote);
            }
        }

        public override bool Update()
        {
            if (!Valid())
                return false;

            int currentTick = Find.TickManager.TicksGame;
            if (currentTick > lastReleaseTick + delayTicks)
            {
                lastReleaseTick = currentTick;
                if (queuedMotes.Count > 0)
                {
                    GenSpawn.Spawn(queuedMotes[0], position, map);
                    queuedMotes.RemoveAt(0);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class TextMoteQueueRealTime : TextMoteQueue
    {
        public const float delayTime = 1.0f / 2.0f;
        public float lastReleaseTime = 0.0f;
        public float timeAccumulated = 0.0f;

        public TextMoteQueueRealTime(IntVec3 aPosition, Map aMap) : base(aPosition, aMap)
        {
            
        }

        public override void AddMote(MoteText newMote)
        {
            if (queuedMotes.Count == 0 && timeAccumulated > lastReleaseTime + delayTime && Valid())
            {
                lastReleaseTime = timeAccumulated;
                GenSpawn.Spawn(newMote, position, map);
            }
            else
            {
                queuedMotes.Add(newMote);
            }
        }

        public override bool Update()
        {
            if (!Valid())
                return false;

            timeAccumulated += Time.deltaTime;

            if (timeAccumulated > lastReleaseTime + delayTime)
            {
                lastReleaseTime = timeAccumulated;
                if (queuedMotes.Count > 0)
                {
                    GenSpawn.Spawn(queuedMotes[0], position, map);
                    queuedMotes.RemoveAt(0);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }

}
