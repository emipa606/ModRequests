using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AdditionalJoyObjects
{

    public class Building_Arcade : Building
    {

        public List<Pawn> owners = new List<Pawn>();

        public bool Unowned => owners.Count <= 0;

        public IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if (!Spawned)
                {
                    return Enumerable.Empty<Pawn>();
                }
                return Map.mapPawns.FreeColonists;
            }
        }

        public IEnumerable<Pawn> AssignedPawns => owners;

        public int MaxAssignedPawnsCount => 5;

        private int OwnerInspectCount
        {
            get
            {
                if (owners.Count > 3)
                {
                    return 3;
                }
                return owners.Count;
            }
        }

        private bool PlayerCanSeeOwners
        {
            get
            {
                if (Faction == Faction.OfPlayer)
                {
                    return true;
                }
                for (int i = 0; i < owners.Count; i++)
                {
                    if (owners[i].Faction == Faction.OfPlayer || owners[i].HostFaction == Faction.OfPlayer)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        public void TryAssignPawn(Pawn pawn)
        {
            if (!owners.Contains(pawn))
            {
                owners.Add(pawn);
            }
        }


        public void TryUnassignPawn(Pawn pawn)
        {
            if (owners.Contains(pawn))
            {
                owners.Remove(pawn);
            }
        }


        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            //Log.Message("Despawned arcade cabinet");
            /*if(mode != DestroyMode.KillFinalize) //if not killed, still drop killedLeavings //disabled: DoLeavingsFor(... killFinalize ...) force-drops slag
            {
                GenLeaving.DoLeavingsFor(this, Map, DestroyMode.KillFinalize, this.OccupiedRect());
            }*/
            RemoveAllOwners();
            base.DeSpawn();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                owners.RemoveAll((Pawn x) => x.Destroyed);
            }
            Scribe_Collections.Look(ref owners, "owners", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                SortOwners();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine();
            if (PlayerCanSeeOwners)
            {
                stringBuilder.AppendLine("ForColonistUse".Translate());
                if (owners.Count == 0)
                {
                    stringBuilder.AppendLine("Owner".Translate() + ": " + "Nobody".Translate().ToLower());
                }
                else if (owners.Count == 1)
                {
                    stringBuilder.AppendLine("Owner".Translate() + ": " + owners[0].Label);
                }
                else
                {
                    stringBuilder.Append("Owners".Translate() + ": ");
                    bool conjugate = false;
                    for (int i = 0; i < OwnerInspectCount; i++)
                    {
                        if (conjugate)
                        {
                            stringBuilder.Append(", ");
                        }
                        conjugate = true;
                        stringBuilder.Append(owners[i].LabelShort);
                    }
                    if (owners.Count > 3)
                    {
                        stringBuilder.Append($" (+ {owners.Count - 3})");
                    }
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }


        public void SortOwners()
        {
            owners.SortBy((Pawn x) => x.thingIDNumber);
        }


        private void RemoveAllOwners()
        {
            owners.Clear();
        }

        public bool AssignedAnything(Pawn pawn)
        {
            return false; //not sure what to do here tbh, I don't want to add a OwnedArcade variable to pawns
        }
    }
}
