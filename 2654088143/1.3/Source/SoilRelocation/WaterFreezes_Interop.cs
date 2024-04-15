using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace SR
{
    public static class WaterFreezes_Interop
	{
		private static Type _waterFreezesAPIType = Type.GetType("WF.API, UdderlyEvelyn.WaterFreezes");
		private static Func<Map, IntVec3, float> _takeCellIceDelegate;
		private static Func<Map, IntVec3, float> _queryCellIceDelegate;
		private static Func<Map, IntVec3, float> _queryCellWaterDelegate;
		private static Func<Map, IntVec3, TerrainDef> _queryCellNaturalWaterDelegate;
		private static Func<Map, IntVec3, TerrainDef> _queryCellAllWaterDelegate;
		private static Action<Map, IntVec3> _clearCellNaturalWaterDelegate;
		private static Action<Map, IntVec3> _clearCellWaterDelegate;
		private static Func<TerrainDef, bool> _isThawableIceDelegate;

		/// <summary>
		/// Returns true if the target type for interop was located.
		/// </summary>
		public static bool InteropTargetIsPresent = _waterFreezesAPIType != null;

		/// <summary>
		/// Call WF.API.IsThawableIce without needing to reference the assembly.
		/// </summary>
		/// <param name="def">TerrainDef to check whether is thawable ice</param>
		/// <returns></returns>
		public static bool IsThawableIce(TerrainDef def)
		{
			if (_waterFreezesAPIType != null)
			{
				if (_isThawableIceDelegate == null) //Everything in here should only execute once if the mod is present.
					_isThawableIceDelegate = (Func<TerrainDef, bool>)_waterFreezesAPIType.GetMethod("IsThawableIce").CreateDelegate(typeof(Func<TerrainDef, bool>)); //Cache it..
				return _isThawableIceDelegate(def);
			}
			return false; //Mod not loaded, return false so nothing is detected as WF ice.
		}

		/// <summary>
		/// Call WF.API.QueryCellAllWater without needing to reference the assembly.
		/// </summary>
		/// <param name="cell">cell to get the water def for from the AllWaterTerrainGrid</param>
		/// <returns>TerrainDef of water at cell from the AllWaterTerrainGrid, null if none</returns>
		public static TerrainDef QueryCellAllWater(Map map, IntVec3 cell)
		{
			if (_waterFreezesAPIType != null)
			{
				if (_queryCellAllWaterDelegate == null) //Everything in here should only execute once if the mod is present.
					_queryCellAllWaterDelegate = (Func<Map, IntVec3, TerrainDef>)_waterFreezesAPIType.GetMethod("QueryCellAllWater").CreateDelegate(typeof(Func<Map, IntVec3, TerrainDef>)); //Cache it..
				return _queryCellAllWaterDelegate(map, cell);
			}
			return null; //Mod not loaded, return null.
		}

		/// <summary>
		/// Call WF.API.QueryCellNaturalWater without needing to reference the assembly.
		/// </summary>
		/// <param name="cell">cell to get the natural water def for</param>
		/// <returns>TerrainDef of natural water at cell, null if none</returns>
		public static TerrainDef QueryCellNaturalWater(Map map, IntVec3 cell)
		{
			if (_waterFreezesAPIType != null)
			{
				if (_queryCellNaturalWaterDelegate == null) //Everything in here should only execute once if the mod is present.
					_queryCellNaturalWaterDelegate = (Func<Map, IntVec3, TerrainDef>)_waterFreezesAPIType.GetMethod("QueryCellNaturalWater").CreateDelegate(typeof(Func<Map, IntVec3, TerrainDef>)); //Cache it..
				return _queryCellNaturalWaterDelegate(map, cell);
			}
			return null; //Mod not loaded, return null.
		}

		/// <summary>
		/// Call WF.API.QueryCellWater without needing to reference the assembly.
		/// </summary>
		/// <param name="cell">cell to get the water depth for</param>
		/// <returns>water depth at cell</returns>
		public static float? QueryCellWater(Map map, IntVec3 cell)
		{
			if (_waterFreezesAPIType != null)
			{
				if (_queryCellWaterDelegate == null) //Everything in here should only execute once if the mod is present.
					_queryCellWaterDelegate = (Func<Map, IntVec3, float>)_waterFreezesAPIType.GetMethod("QueryCellWater").CreateDelegate(typeof(Func<Map, IntVec3, float>)); //Cache it..
				return _queryCellWaterDelegate(map, cell);
			}
			return null; //Mod not loaded, return null.
		}

		/// <summary>
		/// Call WF.API.TakeCellIce without needing to reference the assembly.
		/// </summary>
		/// <param name="cell">cell to get the ice thickness for and clear the ice at</param>
		/// <returns>ice thickness at cell prior to clearing</returns>
		public static float? TakeCellIce(Map map, IntVec3 cell)
		{
			if (_waterFreezesAPIType != null)
			{
				if (_takeCellIceDelegate == null) //Everything in here should only execute once if the mod is present.
					_takeCellIceDelegate = (Func<Map, IntVec3, float>)_waterFreezesAPIType.GetMethod("TakeCellIce").CreateDelegate(typeof(Func<Map, IntVec3, float>)); //Cache it..
				return _takeCellIceDelegate(map, cell);
			}
			return null; //Mod not loaded, return null.
		}

		/// <summary>
		/// Call WF.API.QueryCellIce without needing to reference the assembly.
		/// </summary>
		/// <param name="cell">cell to get the ice thickness for</param>
		/// <returns>ice thickness at cell</returns>
		public static float? QueryCellIce(Map map, IntVec3 cell)
		{
			if (_waterFreezesAPIType != null)
			{
				if (_queryCellIceDelegate == null) //Everything in here should only execute once if the mod is present.
					_queryCellIceDelegate = (Func<Map, IntVec3, float>)_waterFreezesAPIType.GetMethod("QueryCellIce").CreateDelegate(typeof(Func<Map, IntVec3, float>)); //Cache it..
				return _queryCellIceDelegate(map, cell);
			}
			return null; //Mod not loaded, return null.
		}

		/// <summary>
		/// Call WF.API.ClearCellNaturalWater without needing to reference the assembly.
		/// </summary>
		/// <param name="cell">cell to clear natural water at</param>
		public static void ClearCellNaturalWater(Map map, IntVec3 cell)
		{
			if (_waterFreezesAPIType != null)
			{
				if (_clearCellNaturalWaterDelegate == null) //Everything in here should only execute once if the mod is present.
					_clearCellNaturalWaterDelegate = (Action<Map, IntVec3>)_waterFreezesAPIType.GetMethod("ClearCellNaturalWater").CreateDelegate(typeof(Action<Map, IntVec3>)); //Cache it..
				_clearCellNaturalWaterDelegate(map, cell);
			}
		}

		/// <summary>
		/// Call WF.API.ClearCellWater without needing to reference the assembly.
		/// </summary>
		/// <param name="cell">cell to get the water depth for</param>
		public static void ClearCellWater(Map map, IntVec3 cell)
		{
			if (_waterFreezesAPIType != null)
			{
				if (_clearCellWaterDelegate == null) //Everything in here should only execute once if the mod is present.
					_clearCellWaterDelegate = (Action<Map, IntVec3>)_waterFreezesAPIType.GetMethod("ClearCellWater").CreateDelegate(typeof(Action<Map, IntVec3>)); //Cache it..
				_clearCellWaterDelegate(map, cell);
			}
		}
	}
}
