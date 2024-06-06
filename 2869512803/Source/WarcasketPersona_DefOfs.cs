using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using Command_Ability = VFECore.Abilities.Command_Ability;

namespace VFECore.Abilities
{
	[DefOf]
	public static class WP_DefOf
	{
		//Searing Rush
        public static ThingDef WP_CuttingDashFlyer;
		public static DamageDef WP_WhiteHotBurn;
		//Tectonic Crush
		public static ThingDef WP_ProjectileTectonicShockwave;
		public static ThingDef WP_ProjectileTectonicShrapnel;
		//Voltaic Combo
		public static FleckDef MoteComboSlash;
		public static FleckDef MoteComboBlast;
		public static HediffDef WP_VoltaicComboHD;
		public static HediffDef WP_VoltaicAttackSpeedHD;
	}
}