using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Bullet_AlwaysIntercept : Bullet
    {
        IntVec3 cachedPosition;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cachedPosition = Position;
        }

        public override void Tick()
        {
            base.Tick();
            if (Position != cachedPosition)
            {
                CheckForFreeIntercept(Position);
                cachedPosition = Position;
            }
        }

        private bool CheckForFreeIntercept(IntVec3 c)
        {
            if (destination.ToIntVec3() == c)
            {
                return false;
            }
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (!CanHit(thing))
                {
                    continue;
                }
                bool flag2 = false;
                if (thing.def.Fillage == FillCategory.Full)
                {
                    if (!(thing is Building_Door building_Door) || !building_Door.Open)
                    {
                        Impact(thing);
                        return true;
                    }
                    flag2 = true;
                }
                float num2 = 0f;
                if (thing is Pawn pawn)
                {
                    num2 = 0.4f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                    if (pawn.GetPosture() != 0)
                    {
                        num2 *= 0.1f;
                    }
                    if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                    {
                        if (preventFriendlyFire)
                        {
                            num2 = 0f;
                        }
                        else
                        {
                            num2 *= Find.Storyteller.difficulty.friendlyFireChanceFactor;
                        }
                    }
                }
                else if (thing.def.fillPercent > 0.2f)
                {
                    num2 = (flag2 ? 0.05f : ((!DestinationCell.AdjacentTo8Way(c)) ? (thing.def.fillPercent * 0.15f) : (thing.def.fillPercent * 1f)));
                }
                if (num2 > 1E-05f)
                {
                    if (Rand.Chance(num2))
                    {
                        Impact(thing);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
