﻿using Verse;
using RimWorld;

namespace Tacticowl
{
	class Stance_RunAndGun_Cooldown : Stance_Cooldown
	{
		public override bool StanceBusy => Pawn != null && Pawn.CurJobDef != JobDefOf.Goto;
		public Stance_RunAndGun_Cooldown() { }
		public Stance_RunAndGun_Cooldown(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb) { }
	}
	class Stance_RunAndGun : Stance_Warmup
	{
		public override bool StanceBusy => false;
		public Stance_RunAndGun() { }
		public Stance_RunAndGun(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb) { }
	}
}
