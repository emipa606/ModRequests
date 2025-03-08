using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EstupendoBase.Buildings
{
	public class ThingDef_Plaque : ThingDef
	{

	}

	public class Building_Plaque : Building, ISlotGroupParent, IDisplay
	{
		public ThingDef_Plaque Def => this.Def as ThingDef_Plaque;

		private CompArt				InnerArt;
		private StorageSettings		ParentStorage = null;
		private StorageSettings		Storage = null;
		private SlotGroup			slot;
		private CellRect			ClaimedRect = CellRect.Empty;
		private List<Thing>         Contained;
		private OverlayHandle?		overlayHandle;

		public CompArt			Art => InnerArt ?? (InnerArt = GetComp<CompArt>());
		public bool				StorageTabVisible => true;
		public StorageSettings	GetParentStoreSettings() => ParentStorage;
		public StorageSettings	GetStoreSettings() => Storage;
		public string			SlotYielderLabel() => this.LabelCap;
		public bool				IgnoreStoredThingsBeauty => true;
		public SlotGroup		GetSlotGroup() => slot;
		public bool				Empty => Contained.Count == 0;

		public Building_Plaque()
		{
			slot = new SlotGroup(this);
			Contained = new List<Thing>();
		}

		public override void PostMake()
		{
			base.PostMake();
			ParentStorage = new StorageSettings();
			ParentStorage.CopyFrom(this.def.building.fixedStorageSettings);
			Storage = new StorageSettings();
			Storage.CopyFrom(this.def.building.defaultStorageSettings);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref Storage, "storage_settings", new object[] { this });
			Scribe_Values.Look<CellRect>(ref ClaimedRect, "ClaimedRect");
			Scribe_Collections.Look(ref Contained, "Contained", LookMode.Reference);

			if( Scribe.mode == LoadSaveMode.LoadingVars )
			{
				foreach( Thing t in Contained )
				{
					GainThing(t);
				}
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map.GetEstupendoMC().ReleaseDisplayCells(this);
			foreach( Thing t in Contained )
			{
				LoseThing(t);
			}
			Map.overlayDrawer.Disable(this, ref overlayHandle);
			base.DeSpawn(mode);
		}

		#region SlotGroupParent
		public IEnumerable<IntVec3> AllSlotCells()
		{
			yield return this.Position;
		}

		public List<IntVec3> AllSlotCellsList() => AllSlotCells().ToList();

		public void Notify_ReceivedThing(Thing newItem)
		{
			if( Accepts(newItem) )
			{
				GainThing(newItem);
				Notify_Changed();
			}
		}

		public void Notify_LostThing(Thing lostItem)
		{
			LoseThing(lostItem);
			Notify_Changed();
		}

		public void GainThing(Thing gained)
		{
			Contained.AddDistinct(gained);
			gained.Rotation = Rotation;
			if( gained is IDisplayable displayable ) displayable.IsDisplayed = true;
		}

		public void LoseThing(Thing lost)
		{
			Contained.Remove(lost);
			if( lost is IDisplayable displayable ) displayable.IsDisplayed = false;
		}

		public bool Accepts(ThingDef thingDef)
		{
			if( thingDef.thingCategories == null ) { return false; }
			if( ( !thingDef.thingCategories.Contains(Defs.EstupendoDefs.ThingCategory_Displayable) ) && !thingDef.thingCategories.Contains(ThingCategoryDefOf.Corpses) ) { return false; }
			return true;
		}

		public bool Accepts(Thing thing)
		{
			if( !Storage.AllowedToAccept(thing) ) return false;
			if( Contained.Contains(thing) ) return true;
			foreach( Thing t in Contained )
			{
				if( t is IDisplayable displayable )
				{
					if( !displayable.PermitThing(thing) ) return false;
				}
			}
			
			if( thing.HasThingCategory(Defs.EstupendoDefs.ThingCategory_Displayable) ) return true;
			return Accepts(thing.def);
		}

		public CompArt GetArt()
		{
			foreach( Thing t in Contained )
			{
				if( t is IDisplayable display )
				{
					CompArt ret = display.GetArt();
					if( ret != null ) return ret;
				}
			}
			return null;
		}
		#endregion

		public override string Label
		{
			get
			{
				if( Art != null && Art.Active ) return Art.Title;
				return base.Label;
			}
		}

		public void Notify_Changed()
		{
			CellRect ToClaim = GetRequiredDisplayRect();
			if( Map.GetEstupendoMC().ClaimDisplayCells(ToClaim, this) )
			{
				ClaimedRect = ToClaim;
			}
			
			if( !Empty && !Art.Active )
			{
				Art.Borrow(GetArt());
			}

			if( Empty && Art.Active )
			{
				Art.UnBorrow();
			}
		}

		public IntVec2 GetRequiredDisplaySize(Thing thing = null)
		{
			IntVec2 SizeRequired = new IntVec2(1, 1);
			List<Thing> ToSize = new List<Thing>();

			if( thing != null ) ToSize.Add(thing);
			ToSize.AddRange(Contained);

			foreach( Thing displayed in ToSize )
			{
				if( displayed is IDisplayable display )
				{
					IntVec2 display_size = display.GetRequiredDisplaySize();
					if( Rotation.IsHorizontal ) display_size = display_size.Rotated();
					SizeRequired.x = Math.Max(display_size.x, SizeRequired.x);
					SizeRequired.z = Math.Max(display_size.z, SizeRequired.z);
				}
			}
			return SizeRequired;
		}

		public CellRect GetRequiredDisplayRect(Thing thing = null)
		{
			CellRect ret = new CellRect(Position.x, Position.z, 1, 1);
			ret = ret.MovedBy(Rotation.FacingCell);

			IntVec2 Size = GetRequiredDisplaySize(thing);

			IntVec3 ExpansionCell = ret.CenterCell;
			ExpansionCell += Rotation.FacingCell * ( Size.z - 1 );
			ExpansionCell += Rotation.RighthandCell * (int)Math.Ceiling(( Size.x - 1 ) / 2f);

			ret = ret.ExpandToFit(ExpansionCell);

			ExpansionCell -= Rotation.RighthandCell * (Size.x - 1);

			ret = ret.ExpandToFit(ExpansionCell);
			return ret;
		}

		private bool? PlinthWasClear = null;
		public bool IsPlinthClear => !ClaimedRect.Cells.Any((IntVec3 cell) => !cell.SupportsDisplay(Map));

		public string GetListing(bool include_impact)
		{
			string ret = "Contents:\r\n";
			foreach( Thing thing in Contained )
			{
				ret += "\t" + thing.Label.AsTitle() + "\r\n";
				if( include_impact && thing is IDisplayable display )
				{
					ret += "\t\t" + display.GetImpactBreakdown() + "\r\n";
				}
			}
			return ret;
		}

		public float GetImpactFactor()
		{
			float ret = 1f;
			foreach( Thing thing in Contained )
			{
				if( thing is IDisplayable display )
				{
					ret *= display.GetImpactFactor();
				}
			}
			return ret;
		}

		public float GetImpactOffset()
		{
			float ret = 0f;
			foreach( Thing thing in Contained )
			{
				if( thing is IDisplayable display )
				{
					ret += display.GetImpactOffset();
				}
			}
			return ret;
		}

		public float GetMass()
		{
			float ret = 0f;
			foreach( Thing thing in Contained )
			{
				if( thing is IDisplayable display )
				{
					ret += display.GetExtraMass();
				}
			}
			return ret;
		}

		public List<Dialog_InfoCard.Hyperlink> GetLinks()
		{
			return Contained.ConvertAll<Dialog_InfoCard.Hyperlink>((Thing t) => new Dialog_InfoCard.Hyperlink(t));
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if( !Empty )
			{
				yield return new StatDrawEntry(Defs.EstupendoDefs.Estupendo_DisplayStats, "Contents", Contained.Count.ToString(), GetListing(false), 1200, null, GetLinks(), false);
				yield return new StatDrawEntry(Defs.EstupendoDefs.Estupendo_DisplayStats, "Impact", "x" + GetImpactFactor().ToString("F2") + " + " + GetImpactOffset().ToString("F2"), GetListing(true), 1300, null, GetLinks(), false);
			}
			yield break;
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			if( IsPlinthClear )
			{
				Vector3 draw_loc = ClaimedRect.CenterVector3 + new Vector3(0, AltitudeLayer.Building.AltitudeFor(), 0);

				foreach( Thing containee in Contained )
				{
					if( containee is IDisplayable displayable )
					{
						displayable.DrawOnDisplay(draw_loc, Rotation);
					}
				}

				if( !( PlinthWasClear ?? false ) )
				{
					Map.overlayDrawer.Disable(this, ref overlayHandle);
					PlinthWasClear = true;
				}
			}
			else
			{
				if( PlinthWasClear ?? true )
				{
					this.overlayHandle = new OverlayHandle?(Map.overlayDrawer.Enable(this, OverlayTypes.OutOfFuel));
					PlinthWasClear = false;
				}
			}
		}
	}
}

