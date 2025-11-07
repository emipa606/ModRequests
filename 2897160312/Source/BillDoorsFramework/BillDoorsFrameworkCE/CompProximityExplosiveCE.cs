using CombatExtended.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompProximityExplosiveCE : CompProximityExplosive
    {
        static CompProximityExplosiveCE()
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(6, 6);
        }

        public CompProximityExplosiveCE()
        {
            Callback_TryFindEnemyInRange = new WaitCallback(TryFindEnemyInRange);
        }

        public override void TryFindEnemyInRange(object a)
        {
            try
            {
                if (this.parent.Map == null)
                {
                    return;
                }

                foreach (var pawn in this.parent.Position.PawnsInRange(this.parent.Map, this.Props.detectionRadius))
                {
                    if (!closingPawns.Contains(pawn))
                    {
                        closingPawns.Add(pawn);
                        CheckSring(pawn);
                    }
                    if (pawn.Faction.HostileTo(this.parent.Faction))
                    {
                        this.shouldStartWick = true;
                        return;
                    }
                }

                int i = 0;
                if (closingPawns.Count > 0)
                {
                    while (i < closingPawns.Count)
                    {
                        if (closingPawns[i].Position.DistanceTo(this.parent.Position) > this.Props.detectionRadius)
                        {
                            closingPawns.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"{e.ToString()},{e.StackTrace.ToString()},{e.InnerException.ToString()}");
            }
        }
    }
}
