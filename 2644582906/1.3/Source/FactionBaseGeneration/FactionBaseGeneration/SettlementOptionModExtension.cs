using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace FactionBaseGeneration
{
    public class SettlementOptionModExtension : DefModExtension
    {
        public List<string> biomeDefnames;

        public bool removeVanillaGeneratedPawns;

        public List<PawnGenOption> pawnsToGenerate = new List<PawnGenOption>();

        public IntRange distanceToPlayerColony;

        public IntRange numberOfSettlers;

        public Dictionary<PawnKindDef, int> settlers;
    }
}

