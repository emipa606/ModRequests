using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace EstupendoBase
{
	public static class Extensions
	{
		public static Estupendo_MapComponent GetEstupendoMC(this Map map)
		{
			return map.GetComponent<Estupendo_MapComponent>();
		}

		public static string AsTitle(this string title)
		{
			if( title == null ) return "?";
			return GenText.CapitalizeAsTitle(title);
		}

		public static bool SupportsDisplay(this IntVec3 Cell, Map map)
		{
			return Cell.GetThingList(map).Any((Thing t) => t is Buildings.Building_Plinth);
		}

		public static void Borrow(this CompArt shell, CompArt lender)
		{
			if( shell == null || lender == null ) return;
			FieldInfo authorField = typeof(CompArt).GetField("authorNameInt", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
			FieldInfo titleField = typeof(CompArt).GetField("titleInt", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
			FieldInfo taleField = typeof(CompArt).GetField("taleRef", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

			authorField.SetValue(shell, authorField.GetValue(lender));
			titleField.SetValue(shell, titleField.GetValue(lender));
			taleField.SetValue(shell, taleField.GetValue(lender));
		}

		public static void UnBorrow(this CompArt shell)
		{
			if( shell == null ) return;
			FieldInfo authorField = typeof(CompArt).GetField("authorNameInt", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
			FieldInfo titleField = typeof(CompArt).GetField("titleInt", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
			FieldInfo taleField = typeof(CompArt).GetField("taleRef", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

			authorField.SetValue(shell, null);
			titleField.SetValue(shell, null);
			taleField.SetValue(shell, null);
		}
	}

	public interface IDisplayable
	{
		float	GetImpactOffset();
		float	GetImpactFactor();
		bool	PermitThing(Thing t);
		string	TransformPlaqueLabel(string OldLabel);
		bool	WantsToRetain { get; }
		IEnumerable<Thing>	RegisterIngredients(IEnumerable<Thing> ingredients);
		CompArt GetArt();
		float	GetExtraMass();
		void	DrawOnDisplay(Vector3 loc, Rot4 rot);
		string	GetImpactBreakdown();
		IntVec2 GetRequiredDisplaySize();
		bool IsDisplayed { get; set; }
	}

	public interface IDisplay
	{
		float GetImpactOffset();
		float GetImpactFactor();
		void Notify_Changed();
		float GetMass();
	}

}

namespace EstupendoBase.Defs
{
	[DefOf]
	public static class EstupendoDefs
	{
		public static ThingCategoryDef  ThingCategory_Displayable;
		public static StatCategoryDef   Estupendo_DisplayStats;
	}
}
