using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Crystosentient.Abillities
{
	// Token: 0x02000028 RID: 40
	public class CrystalSpawners_CompProperties : CompProperties_AbilityEffect
	{
		// Token: 0x0400006E RID: 110
		public List<IntVec2> pattern;

		// Token: 0x0400006F RID: 111
		public Color dustColor = new Color(160f, 160f, 160f);

		// Token: 0x04000070 RID: 112
		public List<CrystalPatterns> crystalPatterns = new List<CrystalPatterns>();

		// Token: 0x04000071 RID: 113
		public ThingDef spawnBuildingDef;

		// Token: 0x04000072 RID: 114
		public List<ThingDef> spawnThingRandom;

		// Token: 0x04000073 RID: 115
		public bool allowOnBuildings = true;

		// Token: 0x04000074 RID: 116
		public List<PawnKindDef> spawnPawnRandom;

		// Token: 0x04000075 RID: 117
		public string moreThingsTerm;

		// Token: 0x04000076 RID: 118
		public int makeAmount;

		// Token: 0x04000077 RID: 119
		public FloatRange moreThingsCountRange = new FloatRange(1f, 2f);

		// Token: 0x04000078 RID: 120
		public FloatRange moreThingsCountRangeTwo = new FloatRange(1f, 2f);

		// Token: 0x04000079 RID: 121
		public List<string> stringList;

		// Token: 0x0400007A RID: 122
		public string moreThingsTermTwo;

		// Token: 0x0400007B RID: 123
		public List<string> testTerms;

		// Token: 0x0400007C RID: 124
		public string morePawnsTerm;

		// Token: 0x0400007D RID: 125
		public string morePawnsTermTwo;

		// Token: 0x0400007E RID: 126
		public FloatRange morePawnsCountRange = new FloatRange(1f, 2f);

		// Token: 0x0400007F RID: 127
		public FloatRange morePawnsCountRangeTwo = new FloatRange(1f, 2f);

		// Token: 0x04000080 RID: 128
		public FloatRange thingCountRange = new FloatRange(1f, 2f);

		// Token: 0x04000081 RID: 129
		public FloatRange pawnCountRange = new FloatRange(1f, 2f);

		// Token: 0x04000082 RID: 130
		public List<List<IntVec2>> patterns;

		// Token: 0x04000083 RID: 131
		public float abilityRadius;

		// Token: 0x04000084 RID: 132
		public DamageDef damageDef;

		// Token: 0x04000085 RID: 133
		public int damageAmount;

		// Token: 0x04000086 RID: 134
		public SoundDef soundExplosion;

		// Token: 0x04000087 RID: 135
		public ThingDef preExplosionSpawnThingDef;

		// Token: 0x04000088 RID: 136
		public float preExplosionSpawnChance;

		// Token: 0x04000089 RID: 137
		public int preExplosionSpawnThingCount;

		// Token: 0x0400008A RID: 138
		public ThingDef postExplosionSpawnThingDef;

		// Token: 0x0400008B RID: 139
		public float postExplosionSpawnChance;

		// Token: 0x0400008C RID: 140
		public int postExplosionSpawnThingCount;

		// Token: 0x0400008D RID: 141
		public EffecterDef effecterDef;

		// Token: 0x0400008E RID: 142
		public int maintainForTicks;

		// Token: 0x0400008F RID: 143
		public float scale;

		// Token: 0x04000090 RID: 144
		public FloatRange temperatureRange = new FloatRange(1f, 2f);

		// Token: 0x04000091 RID: 145
		public List<IntVec2> patternRoof;

		// Token: 0x04000092 RID: 146
		public bool doExplosion;

		// Token: 0x04000093 RID: 147
		public List<IntVec2> patternTerrain;
	}
}
