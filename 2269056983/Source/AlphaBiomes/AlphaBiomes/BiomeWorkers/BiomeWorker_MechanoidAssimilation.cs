using System;
using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse;

namespace MechanoidAssimilation
{
    public class BiomeWorker_MechanoidAssimilation : BiomeWorker
    {
        public override float GetScore(Tile tile, int tileID)
        {
            
            if (tile.WaterCovered)
            {
                return -100f;
            }
            
            else return 100f;
            
        }
    }
}
