using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

using RimWorld.BaseGen;

namespace AP_PredatorsRework
{
    public class Skeleton_SymbolResolver : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            ResolveParams resolveParamsCorpses = rp;
            int corpseCount = 9;
            for (int i = 0; i < corpseCount; i++)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Villager, Faction.OfAncients, PawnGenerationContext.NonPlayer, canGeneratePawnRelations: false, forceGenerateNewPawn: true);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                HealthUtility.DamageUntilDead(pawn);
                pawn.Corpse.GetComp<CompRottable>().RotProgress = 5000000;
                pawn.Corpse.Age = 36000000;
                resolveParamsCorpses.singleThingToSpawn = pawn.Corpse;
                BaseGen.symbolStack.Push("thing", resolveParamsCorpses);
            }
            Pawn boa = Find.WorldPawns.AllPawnsAlive.First(x => x.def.defName == "AP_Titanboa");
            boa.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
            resolveParamsCorpses.singleThingToSpawn = boa;
            BaseGen.symbolStack.Push("thing", resolveParamsCorpses);
        }
    }
}
