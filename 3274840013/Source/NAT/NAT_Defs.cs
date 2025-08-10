using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;
using UnityEngine;
using System.Diagnostics;

namespace NAT
{
	[DefOf]
	public static class NATDefOf
	{
		public static BoxActionDef TriedToDoSmth;

		public static ThingDef NAT_WarpedObelisk_Inducer;

		public static QuestScriptDef NAT_CollectorLair;

		public static ThingDef NAT_DarkLairCasket;

		public static ThingDef NAT_RustedSoldier;

		public static ThingDef NAT_FleshChunkWithNotes;

		public static ThingDef NAT_CollectorSpawner;

		public static ThingDef NAT_CollectorBag;

		public static ThingDef NAT_RustedMassIncoming;

		public static ThingDef NAT_RustedTrooperIncoming;

		public static ThingDef NAT_PainFigure;

		public static ThingDef NAT_PainWall;

		public static JobDef NAT_CollectorSteal;

		public static JobDef NAT_CollectorWander;

		public static JobDef NAT_CollectorWait;

		public static JobDef NAT_CollectorWait_Reworked;

		public static JobDef NAT_CollectorSteal_Reworked;

		public static JobDef NAT_CollectorEscape;

		public static JobDef NAT_CollectorEscape_Teleport;

		public static JobDef NAT_ChangeRustedWeapon;

		public static JobDef NAT_RequestDrop;

		public static JobDef NAT_UseItemByRust;

		public static HediffDef NAT_ShockFromLair;

		public static HediffDef NAT_HediffCollector;

		public static MapGeneratorDef NAT_Box;

		public static HediffDef NAT_InducedPain;

		public static PawnKindDef NAT_Collector;

		public static PawnKindDef NAT_Collector_Reworked;

		public static PawnKindDef NAT_RustedSphere;

		public static ThingSetMakerDef NAT_Reward_CollectorLair;

		public static PawnGroupKindDef NAT_RustedArmy;

		public static PawnTableDef NAT_Rusts;
	}
}
