using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace DIYHumans
{
    public class CompCraftedPawn : ThingComp
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public CompProperties_CraftedPawn Props
        {
            get
            {
                return (CompProperties_CraftedPawn)this.props;
            }
        }

        /// <summary>
        /// spawns pawn and applies mental state if required
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Faction pawnFaction = this.Props.faction;
            if (pawnFaction == null)
            {
                pawnFaction = Faction.OfPlayer;
            }
            bool pawnIsNewborn = this.Props.pawnKindDef.maxGenerationAge == 0;
            Pawn pawnToSpawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(this.Props.pawnKindDef, pawnFaction, PawnGenerationContext.NonPlayer, -1, false, pawnIsNewborn, false, false, false, false, 0f, false, true, false, false, false, false, false, false, 0f, 0f, null, 0f, null, null));
            pawnToSpawn.apparel.DestroyAll();
            IntVec3 spawnPosition = this.parent.Position;
            Map spawnMap = this.parent.Map;
            GenSpawn.Spawn(pawnToSpawn, spawnPosition, spawnMap, WipeMode.Vanish);
            ApplyQualityEffects(pawnToSpawn);
            ApplyMentalState(pawnToSpawn);
            this.parent.Destroy(DestroyMode.Vanish);
        }

        /// <summary>
        /// applies various effects to the given pawn based on the quality of this thing
        /// </summary>
        public void ApplyQualityEffects(Pawn pawn)
        {
            CompQuality compQuality = this.parent.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                QualityCategory quality = compQuality.Quality;
                //multiplies all skills
                float skillMultiplier = ((int)quality + 2f) / 4f;
                if (pawn.skills != null)
                {
                    foreach (SkillRecord skill in pawn.skills.skills)
                    {
                        skill.Level = (int)(skill.Level * skillMultiplier);
                    }
                }
                //applies infections
                float infectionChance = (6f - (int)quality) / 12f;
                if (Rand.Value < infectionChance)
                {
                    IEnumerable<BodyPartRecord> bodyParts = pawn.health?.hediffSet?.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
                    int infectionCount = Rand.Range(1, 6 - (int)quality);
                    for (int i = 0; i < infectionCount; i++)
                    {
                        BodyPartRecord randomPart = bodyParts.RandomElement();
                        pawn.health?.AddHediff(HediffDefOf.WoundInfection, randomPart, null, null);
                    }
                }
            }
        }

        /// <summary>
        /// applies a random mental state to the given pawn if possible
        /// </summary>
        public void ApplyMentalState(Pawn pawn)
        {
            if (!!this.Props.possibleMentalStates.NullOrEmpty() && Rand.Value < this.Props.mentalStateChance)
            {
                MentalStateDef mentalState = this.Props.possibleMentalStates.RandomElement();
                pawn.mindState.mentalStateHandler.TryStartMentalState(mentalState, null, false, false, null, false);
            }
        }
    }
}
