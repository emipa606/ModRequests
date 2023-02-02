using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace WallDraggerOverhaul
{
	[StaticConstructorOnStartup]
	public static class DesignationDragger_UpdateDragCells_Patch
	{
		public static void Prefix (DesignationDragger __instance, out int __state)
		{
			FieldInfo[] fields = typeof(DesignationDragger).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

			FieldInfo field_lastUpdateFrame = fields.Where(x => x.Name.Equals("lastUpdateFrame")).First();

			__state = (int) field_lastUpdateFrame.GetValue(__instance);
		}

		public static void Postfix (DesignationDragger __instance, int __state)
		{


			if (!MyDefOf.ToggleDragMode.IsDown)
			{
				return;
			}

			FieldInfo[] fields = typeof(DesignationDragger).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

			FieldInfo field_dragCells = fields.Where(x => x.Name.Equals("dragCells")).First();
			FieldInfo field_startDragCell = fields.Where(x => x.Name.Equals("startDragCell")).First();

			MethodInfo method_TryAddCell = typeof(DesignationDragger).GetMethod("TryAddDragCell", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(IntVec3) }, new ParameterModifier[] { });

			Designator selDes = Find.DesignatorManager.SelectedDesignator;

			if (Time.frameCount == __state)
			{
				return;
			}
			if (selDes.DraggableDimensions == 1)
			{
				((List<IntVec3>) field_dragCells.GetValue(__instance)).Clear();


				IntVec3 origin = (IntVec3) field_startDragCell.GetValue(__instance);
				IntVec3 destination = UI.MouseCell();

				IntVec3 direction = destination - origin;
				direction.x = Math.Abs(direction.x);
				direction.z = Math.Abs(direction.z);

				int x = origin.x;
				int z = origin.z;

				double dt_dx = 1.0 / direction.x;
				double dt_dz = 1.0 / direction.z;

				double t = 0;

				int n = 1;
				int x_inc, z_inc;
				double t_next_vertical, t_next_horizontal;

				if (direction.x == 0)
				{
					x_inc = 0;
					t_next_horizontal = dt_dx;
				}
				else if (destination.x > origin.x)
				{
					x_inc = 1;
					n += destination.x - x;
					t_next_horizontal = dt_dx;
				}
				else
				{
					x_inc = -1;
					n += x - destination.x;
					t_next_horizontal = dt_dx;
				}

				if (direction.z == 0)
				{
					z_inc = 0;
					t_next_vertical = dt_dz;
				}
				else if (destination.z > origin.z)
				{
					z_inc = 1;
					n += destination.z - z;
					t_next_vertical = dt_dz;
				}
				else
				{
					z_inc = -1;
					n += z - destination.z;
					t_next_vertical = dt_dz;
				}

				for (; n > 0; n--)
				{
					method_TryAddCell.Invoke(__instance, new object[] { new IntVec3(x, 0, z) });

					if (t_next_vertical < t_next_horizontal)
					{
						z += z_inc;
						t = t_next_vertical;
						t_next_vertical += dt_dz;
					}
					else
					{
						x += x_inc;
						t = t_next_horizontal;
						t_next_horizontal += dt_dx;
					}
				}
			}
		}

		static DesignationDragger_UpdateDragCells_Patch ()
		{
			Harmony harmony = new Harmony("energistics.walldraggeroverhaul");

			MethodInfo original = AccessTools.Method(typeof(DesignationDragger), "UpdateDragCellsIfNeeded");

			HarmonyMethod postfix = new HarmonyMethod(typeof(DesignationDragger_UpdateDragCells_Patch).GetMethod(nameof(Postfix)));
			HarmonyMethod prefix = new HarmonyMethod(typeof(DesignationDragger_UpdateDragCells_Patch).GetMethod(nameof(Prefix)));

			harmony.Patch(original, prefix: prefix, postfix: postfix);
		}
	}

	public class CodeInstructionBuilder
	{
		private ILGenerator ilGenerator;

		public CodeInstructionBuilder (ILGenerator ilGenerator)
		{
			this.ilGenerator = ilGenerator;
		}

		public static IEnumerable<CodeInstruction> IncFieldOfVar (int varIndex, FieldInfo field)
		{
			yield return new CodeInstruction(OpCodes.Ldloca_S, varIndex);
			yield return new CodeInstruction(OpCodes.Ldflda, field);
			yield return new CodeInstruction(OpCodes.Dup);
			yield return new CodeInstruction(OpCodes.Ldind_I4);
			yield return new CodeInstruction(OpCodes.Ldc_I4_1);
			yield return new CodeInstruction(OpCodes.Add);
			yield return new CodeInstruction(OpCodes.Stind_I4);
		}

		public static IEnumerable<CodeInstruction> IncVarByFieldVar (int indexToInc, FieldInfo field, int indexOfVarField)
		{
			yield return new CodeInstruction(OpCodes.Ldloc_S, indexToInc);
			yield return new CodeInstruction(OpCodes.Ldloc_S, indexOfVarField);
			yield return new CodeInstruction(OpCodes.Ldfld, field);
			yield return new CodeInstruction(OpCodes.Add);
			yield return new CodeInstruction(OpCodes.Stloc_S, indexToInc);
		}

		public static IEnumerable<CodeInstruction> DecFieldOfVar (int varIndex, FieldInfo field)
		{
			yield return new CodeInstruction(OpCodes.Ldloca_S, varIndex);
			yield return new CodeInstruction(OpCodes.Ldflda, field);
			yield return new CodeInstruction(OpCodes.Dup);
			yield return new CodeInstruction(OpCodes.Ldind_I4);
			yield return new CodeInstruction(OpCodes.Ldc_I4_1);
			yield return new CodeInstruction(OpCodes.Sub);
			yield return new CodeInstruction(OpCodes.Stind_I4);
		}

		public static IEnumerable<CodeInstruction> DecVarByFieldVar (int indexToDec, FieldInfo field, int indexOfVarField)
		{
			yield return new CodeInstruction(OpCodes.Ldloc_S, indexToDec);
			yield return new CodeInstruction(OpCodes.Ldloc_S, indexOfVarField);
			yield return new CodeInstruction(OpCodes.Ldfld, field);
			yield return new CodeInstruction(OpCodes.Sub);
			yield return new CodeInstruction(OpCodes.Stloc_S, indexToDec);
		}

		public static IEnumerable<CodeInstruction> WriteLog (string message)
		{
			yield return new CodeInstruction(OpCodes.Ldstr, message);
			yield return new CodeInstruction(OpCodes.Ldc_I4_0);
			yield return new CodeInstruction(OpCodes.Call, typeof(Log).GetMethod(nameof(Log.Message)));
		}
	}

	public class LabelCreator
	{
		private ILGenerator ilGenerator;

		private Dictionary<object, Label> labels = new Dictionary<object, Label>();

		public LabelCreator (ILGenerator ilGenerator)
		{
			this.ilGenerator = ilGenerator;
		}

		public Label GetLabel (object key)
		{
			Label label;
			if (!labels.TryGetValue(key, out label))
			{
				label = this.ilGenerator.DefineLabel();
				labels.Add(key, label);
			}
			return label;
		}
	}

	public static class ILLocalsManager
	{
		private static Dictionary<string, int> locals = new Dictionary<string, int>();

		public static ILGenerator ILGenerator { get; set; }

		public static void AddLocal (string name, int index)
		{
			locals.Add(name, index);
		}

		public static void NewLocal (string name, Type type)
		{
			LocalBuilder builder = ILGenerator.DeclareLocal(type);
			locals.Add(name, builder.LocalIndex);
		}

		public static int GetIndex (string name)
		{
			return locals.GetValueSafe(name);
		}
	}

	public static class Extensions
	{
		public static void EnqueueAll<T> (this Queue<T> prefix, IEnumerable<T> suffix)
		{
			foreach (T item in suffix)
			{
				prefix.Enqueue(item);
			}
		}
	}
}
