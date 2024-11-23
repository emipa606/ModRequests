using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;

namespace HDream
{
    public class WishWithComp : Wish
    {
        public List<WishComp> comps;

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				InitializeComps();
			}
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompExposeData();
				}
			}

		}

		protected override void DoPendingEffect()
		{
			bool preventPending = false;
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (!comps[i].CompOnPending(ref pendingTicks)) preventPending = true;
				}
			}
			if (!preventPending) base.DoPendingEffect();
		}

		public override void OnFulfill()
		{
			bool preventFulfill = false;
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (!comps[i].CompOnFulfill()) preventFulfill = true;
				}
			}
			if(!preventFulfill) base.OnFulfill();
		}

		public override void PostAdd()
		{
			base.PostAdd();
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompPostAdd();
				}
			}
		}

		public override void PostRemoved()
		{
			base.PostRemoved();
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompPostRemoved();
				}
			}
		}

		public override void PostTick()
		{
			base.PostTick();
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompPostTick();
				}
			}
		}

		public override void PostMake()
		{
			base.PostMake();
			InitializeComps();
			if (comps != null)
			{
				for (int num = comps.Count - 1; num >= 0; num--)
				{
					try
					{
						comps[num].CompPostMake();
					}
					catch (Exception arg)
					{
						Log.Error("Error in Wish.CompPostMake(): " + arg);
						comps.RemoveAt(num);
					}
				}
			}
		}

		public override void ChangeProgress(int value)
		{
			base.ChangeProgress(value);
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompPostChange(value);
				}
			}
		}

		public override string DescriptionToFulfill
		{
			get
			{
				if (comps == null) return base.DescriptionToFulfill;
				string desc = "";
				for (int i = 0; i < comps.Count; i++)
				{
					desc += comps[i].CompDescription();
				}
				return base.DescriptionToFulfill + desc;
			}
		}

		protected void InitializeComps()
		{
			if (def.comps != null)
			{
				comps = new List<WishComp>();
				for (int i = 0; i < def.comps.Count; i++)
				{
					WishComp wishComp = null;
					try
					{
						wishComp = (WishComp)Activator.CreateInstance(def.comps[i].compClass);
						wishComp.props = def.comps[i];
						wishComp.parent = this;
						comps.Add(wishComp);
					}
					catch (Exception arg)
					{
						Log.Error("Could not instantiate or initialize a WishComp: " + arg);
						comps.Remove(wishComp);
					}
				}
			}
		}
	}
}
