using RimWorld;
using HarmonyLib;
using Verse;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace HDream
{
    public class WishComp
    {

		public WishWithComp parent;

		public WishCompProperties props;

		public Pawn Pawn => parent.pawn;

		public WishDef Def => parent.def;

		public virtual bool CompOnPending(ref int PendingTick)
		{
			return true;
		}
		public virtual bool CompOnFulfill()
		{
			return true;
		}
		public virtual void CompPostMake() { }
		public virtual void CompPostTick() { }
		public virtual void CompExposeData() { }
		public virtual void CompPostAdd() { }
		public virtual void CompPostRemoved() { }
		public virtual void CompPostChange(int value) { }
		public virtual string CompDescription()
		{
			return "\n\n\n";
		}

		public virtual string CompDebugString()
		{
			return null;
		}
	}
}
