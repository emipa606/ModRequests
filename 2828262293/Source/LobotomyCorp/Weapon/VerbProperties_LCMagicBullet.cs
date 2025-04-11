using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LobotomyCorp.Weapon
{
    public class VerbProperties_LCMagicBullet : VerbProperties
    {
        public ThingDef circle;
        private List<Vector3> cells = null;

        public VerbProperties_LCMagicBullet()
        {
            verbClass = typeof(Verb_MagicBullet);
        }

        public List<Vector3> AffectCells 
        {
            get{
                if (cells == null)
                {
                    cells = new List<Vector3>();
                    for(int i = 0; i < range; i++)
                    {
                        cells.Add(new Vector3(0f, 0f, i));
                    }
                }
                return cells;
            }
        }

    }
}
