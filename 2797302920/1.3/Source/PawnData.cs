using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Renamon
{
    public class PawnData : IExposable
    {
        public ThingDef race;
        public NameTriple name;
        public BeardDef beardDef;
        public HairDef hairDef;
        public Color skinColor;
        public Color skinColorSecond;
        public Color hairColor;
        public Color hairColorSecond;
        public string crownType;
        public List<Trait> traits;
        public List<SkillRecord> skillRecords;
        public string childhoodIdentifier;
        public string adulthoodIdentifier;
        public BodyTypeDef bodyType;
        public TattooDef faceTattoo;
        public TattooDef bodyTattoo;

        public PawnData()
        {

        }

        public PawnData(Pawn source)
        {

        }
        public void ExposeData()
        {
            Scribe_Deep.Look(ref name, "name");
            Scribe_Defs.Look(ref race, "race");
            Scribe_Defs.Look(ref beardDef, "beardDef");
            Scribe_Defs.Look(ref hairDef, "hairDef");
            Scribe_Defs.Look(ref faceTattoo, "faceTattoo");
            Scribe_Defs.Look(ref bodyTattoo, "bodyTattoo");
            Scribe_Defs.Look(ref bodyType, "bodyType");
            Scribe_Values.Look(ref crownType, "crownType");
            Scribe_Values.Look(ref skinColor, "skinColor");
            Scribe_Values.Look(ref skinColorSecond, "skinColorSecond");
            Scribe_Values.Look(ref hairColor, "hairColor");
            Scribe_Values.Look(ref hairColorSecond, "hairColorSecond");
            Scribe_Values.Look(ref childhoodIdentifier, "childhoodIdentifier");
            Scribe_Values.Look(ref adulthoodIdentifier, "adulthoodIdentifier");
            Scribe_Collections.Look(ref traits, "traits");
            Scribe_Collections.Look(ref skillRecords, "skillRecords");
        }
    }
}
