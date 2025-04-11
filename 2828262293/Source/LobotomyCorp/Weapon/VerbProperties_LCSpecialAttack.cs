using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LobotomyCorp.Weapon
{
    public class VerbProperties_LCSpecialAttack : VerbProperties
    {
        public int hitCount = 1;
        public DamageDef noLCDamage;

        public bool affectColonist = true;

        public bool IsRangeAttack = false;

        public AOEType aoeType;
    }

    public class AOEType
    {
        public Multi multi = Multi.Box;

        public Vector2 range = Vector2.zero;
        public int radius = 0;
        public int length = 1;

        private HashSet<IntVec3> affects;
        public HashSet<IntVec3> AffectCells
        {
            get
            {
                if (affects.EnumerableNullOrEmpty())
                {
                    affects = new HashSet<IntVec3>();
                    if (multi == Multi.Box)
                    {
                        for (int y = 0; y <= range.y; y++)
                        {
                            for (int x = (int)range.x * -1; x <= range.x; x++)
                            {
                                affects.Add(new IntVec3(x, 0, y));
                            }
                        }
                    }
                    else if (multi == Multi.Bleath)
                    {
                        int f = Math.Max(length, radius);
                        for (int y = 0; y < f; y++)
                        {
                            for (int x = y * -1; x <= y; x++)
                            {
                                affects.Add(new IntVec3(x, 0, y));
                            }
                        }
                    }
                    else if (multi == Multi.Line)
                    {
                        int f = Math.Max(length, radius);
                        for (int y = 0; y < f; y++)
                        {
                            affects.Add(new IntVec3(0, 0, y));
                        }
                    }
                    else
                    {
                        if (multi == Multi.Room && radius == 0) radius = 5;
                        for (int y = -1 * radius; y <= radius; y++)
                        {
                            for (int x = -1 * radius; x <= radius; x++)
                            {
                                if (x * x + y * y <= radius * radius)
                                {
                                    affects.Add(new IntVec3(x, 0, y));
                                }
                            }
                        }
                    }
                }
                return affects;
            }
        }

        public List<IntVec3> AffectCellsOf(IntVec3 pos)
        {
            return AffectCells.Select((x) => x + pos).ToList();
        }
        public List<IntVec3> AffectCellsOf(IntVec3 pos, Rot4 rot)
        {
            return Rotate(rot).Select((x) => x + pos).ToList();
        }

        public IEnumerable<IntVec3> Rotate(Rot4 rot)
        {
            foreach (IntVec3 v3 in AffectCells)
            {
                yield return v3.RotatedBy(rot);
            }
        }

    }


    public enum Multi
    {
        Box,
        Circle,
        Line,
        Bleath,
        Room,
    }
}
