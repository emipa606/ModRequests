using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ArmoredUp;
using HarmonyLib;
using static ArmoredUp.ArmoredUp;
using RimWorld;
using UnityEngine;
using Vehicles;
using Verse;

namespace VehiclesPatches
{
	
	[StaticConstructorOnStartup]
	public static class VehiclesPatch
	{
		public static Type TypeOf_VehicleStatHandler = AccessTools.TypeByName("Vehicles.VehicleStatHandler");
		public static Type TypeOf_VehicleComponent = AccessTools.TypeByName("Vehicles.VehicleComponent");

		static VehiclesPatch()
		{
			new Harmony("ArmoredUp.VehiclesPatches").PatchAllUncategorized();
		}
				
		[HarmonyPatch]
		//[HarmonyPatchCategory("Vehicles")]
		internal static class VehicleStatHandler_ApplyDamageToComponent_Reroute
		{
			public static bool Prepare()
			{
				return TypeOf_VehicleStatHandler != null;
			}
			
			public static MethodBase TargetMethod()
			{
				return AccessTools.Method(TypeOf_VehicleStatHandler, "ApplyDamageToComponent");
				//return TypeOf_VehicleStatHandler.GetMethod("ApplyDamageToComponent", new []{typeof(DamageInfo), typeof(IntVec2), typeof(StringBuilder)});
			}

			public static bool Prefix(VehicleStatHandler __instance, DamageInfo dinfo, IntVec2 hitCell, StringBuilder report = null)
			{
				DamageDef def = dinfo.Def;
				float num = dinfo.Amount;
				if (def.workerClass == typeof(DamageWorker_Extinguish))
				{
					__instance.TryExtinguishFire(dinfo, hitCell);
				}

				if (!def.harmsHealth)
				{
					num = 0f;
				}

				try
				{
					report?.AppendLine("-- DAMAGE REPORT --");
					report?.AppendLine($"Base Damage: {num}");
					report?.AppendLine($"DamageDef: {dinfo.Def}");
					report?.AppendLine($"HitCell: {hitCell}");
					VehicleDamageMultiplierDefModExtension vehDamMultDefModExt =
						dinfo.Weapon?.GetModExtension<VehicleDamageMultiplierDefModExtension>();
					if (vehDamMultDefModExt != null)
					{
						num *= vehDamMultDefModExt.multiplier;
						report?.AppendLine($"ModExtension Multiplier: {vehDamMultDefModExt.multiplier} Result: {num}");
					}

					VehicleDamageMultiplierDefModExtension vehDamMultDefModExt2 =
						dinfo.Instigator?.def.GetModExtension<VehicleDamageMultiplierDefModExtension>();
					if (vehDamMultDefModExt2 != null)
					{
						num *= vehDamMultDefModExt2.multiplier;
						report?.AppendLine($"ModExtension Multiplier: {vehDamMultDefModExt2.multiplier} Result: {num}");
					}

					if (!__instance.vehicle.VehicleDef.properties.damageDefMultipliers.NullOrEmpty() &&
					    __instance.vehicle.VehicleDef.properties.damageDefMultipliers.TryGetValue(dinfo.Def,
						    out var value))
					{
						num *= value;
						report?.AppendLine($"DamageDef Multiplier: {value} Result: {num}");
					}

					if (dinfo.Def.isRanged)
					{
						num *= VehicleMod.settings.main.rangedDamageMultiplier;
						report?.AppendLine(
							$"Settings Multiplier: {VehicleMod.settings.main.rangedDamageMultiplier} Result: {num}");
					}
					else if (dinfo.Def.isExplosive)
					{
						num *= VehicleMod.settings.main.explosiveDamageMultiplier;
						report?.AppendLine(
							$"Settings Multiplier: {VehicleMod.settings.main.explosiveDamageMultiplier} Result: {num}");
					}
					else
					{
						num *= VehicleMod.settings.main.meleeDamageMultiplier;
						report?.AppendLine(
							$"Settings Multiplier: {VehicleMod.settings.main.meleeDamageMultiplier} Result: {num}");
					}

					if (num <= 0f)
					{
						report?.AppendLine($"Final Damage = {num}. Exiting.");
						return false;
					}

					dinfo.SetAmount(num);
					Rot4 dir = __instance.DirectionFromAngle(dinfo.Angle);
					VehicleComponent.VehiclePartDepth hitDepth = VehicleComponent.VehiclePartDepth.External;
					List<VehicleComponent> allLocationalComponents = new();
					if (__instance.componentLocations.TryGetValue(hitCell, out var allLocCompRef))
						allLocationalComponents.AddRange(allLocCompRef.Where(IsIntact));
					List<VehicleComponent> allIntactComponents = __instance.components.Where(IsIntact).ToList();
					for (int i = 0;
					     i < Mathf.Max(__instance.vehicle.VehicleDef.Size.x, __instance.vehicle.VehicleDef.Size.z);
					     i++)
					{
						if (__instance.vehicle.Destroyed)
							break;
						if (dinfo.Amount <= 0f)
							break;

						VehicleComponent result;
						report?.AppendLine($"Damaging = {hitCell}");
						if (allLocationalComponents.Count > 0)
						{
							report?.AppendLine("locational components=(" +
							                   string.Join(",", allLocationalComponents.Select(c => c.props.label)) +
							                   ")");
							report?.AppendLine($"hitDepth = {hitDepth}");
							report?.AppendLine(string.Format("components at hitDepth {0}: ({1})", hitDepth,
								string.Join(",",
									allLocationalComponents.Where(IsExternal).Select(comp => comp.props.label))));
							if (!allLocationalComponents.Where(IsExternal).TryRandomElementByWeight(
								    component => component.props.hitWeight,
								    out result))
							{
								report?.AppendLine(
									"No external locational components found. Hitting internal locational parts.");
								hitDepth = VehicleComponent.VehiclePartDepth.Internal;
								if (!allLocationalComponents.Where(IsInternal).TryRandomElementByWeight(
									    component => component.props.hitWeight,
									    out result))
								{
									result = allIntactComponents.Where(IsInternal)
										.RandomElementByWeightWithFallback(component => component.props.hitWeight);

									if (result == null)
										result = allIntactComponents.RandomElementByWeightWithFallback(component =>
											component.props.hitWeight);

									if (result == null)
										break;

									allIntactComponents.Remove(result);
								}
								else
								{
									report?.AppendLine(
										$"Found Internal Locational Component {result.props.label} at {hitCell}");
									allLocationalComponents.Remove(result);
									allIntactComponents.Remove(result);
								}
							}
							else
							{
								report?.AppendLine(
									$"Found External Locational Component {result.props.label} at {hitCell}");
								allLocationalComponents.Remove(result);
								allIntactComponents.Remove(result);
							}
						}
						else
						{
							report?.AppendLine("No Locational components found. Hitting random internal parts.");
							hitDepth = VehicleComponent.VehiclePartDepth.Internal;
							result = allIntactComponents.Where(IsInternal)
								.RandomElementByWeightWithFallback(component => component.props.hitWeight);
							if (result == null)
								result = allIntactComponents.RandomElementByWeightWithFallback(component =>
									component.props.hitWeight);

							if (result == null)
								break;
							allIntactComponents.Remove(result);
						}

						if (!hitCell.IsValid)
						{
							break;
						}

						if (VehicleMod.settings.debug.debugDrawHitbox)
						{
							IntVec2 intVec = hitCell;
							if (__instance.vehicle.Rotation != Rot4.North)
							{
								intVec = intVec.RotatedBy(__instance.vehicle.Rotation,
									__instance.vehicle.VehicleDef.Size, reverseRotate: true);
							}

							__instance.debugCellHighlight.Add(new Pair<IntVec2, int>(intVec, 100));
						}

						report?.AppendLine($"Damaging {hitCell}");
						if (__instance.HitPawn(dinfo, hitDepth, hitCell, dir, out var hitPawn))
						{
							report?.AppendLine($"Hit {hitPawn} for {dinfo.Amount}. Impact site = {hitCell}");
							break;
						}

						report?.AppendLine($"Applying Damage = {dinfo.Amount} to {result.props.key} at {hitCell}");
						VehicleComponent.Penetration penetration = result.TakeDamage(__instance.vehicle, ref dinfo);
						if (i == 0)
						{
							//new IntVec3(__instance.vehicle.Position.x + hitCell.x, 0, __instance.vehicle.Position.z + hitCell.z);
							__instance.vehicle.Notify_DamageImpact(new VehicleComponent.DamageResult
							{
								penetration = penetration,
								damageInfo = dinfo,
								cell = hitCell
							});
						}

						report?.AppendLine($"Fallthrough Damage = {dinfo.Amount}");
					}
				}
				finally
				{
					__instance.RecalculateHealthPercent();
				}

				return false;
			}


			private static bool IsIntact(VehicleComponent comp)
			{
				return comp.HealthPercent > 0f;
			}
			private static bool IsExternal(VehicleComponent comp)
			{
				return comp.Depth == VehicleComponent.VehiclePartDepth.External && comp.HealthPercent > 0f;
			}
			private static bool IsInternal(VehicleComponent comp)
			{
				return comp.Depth == VehicleComponent.VehiclePartDepth.Internal && comp.HealthPercent > 0f;
			}
		}

		[HarmonyPatch]
		//[HarmonyPatchCategory("Vehicles")]
		internal static class VehicleComponent_TakeDamage_Patch
		{
			public static bool Prepare()
			{
				return TypeOf_VehicleComponent != null;
			}
			
			public static MethodBase TargetMethod()
			{
				return TypeOf_VehicleComponent.GetMethod("TakeDamage", new[] { typeof(VehiclePawn), typeof(DamageInfo).MakeByRefType(), typeof(bool)});
			}
			
			public static bool Prefix(VehicleComponent __instance, VehiclePawn vehicle, ref DamageInfo dinfo,
				ref VehicleComponent.Penetration __result, bool ignoreArmor = false)
			{
				__result = VehicleComponent.Penetration.NonPenetrated;
				float armorDamage = dinfo.Amount;
				if (!ignoreArmor)
				{
					armorDamage = ReduceDamageFromArmor_Reroute(__instance, ref dinfo, out __result);
				}

				__instance.health -= armorDamage;
				__instance.health = Mathf.Clamp(__instance.health, 0f, __instance.MaxHealth);
				if (dinfo.Amount > 0f)
				{
					if (!__instance.props.reactors.NullOrEmpty())
					{
						foreach (Reactor reactor in __instance.props.reactors)
						{
							reactor.Hit(vehicle, __instance, ref dinfo, __result);
						}
					}

					vehicle.EventRegistry[VehicleEventDefOf.DamageTaken].ExecuteEvents();
					if (vehicle.GetStatValue(VehicleStatDefOf.MoveSpeed) <= 0.1f)
					{
						vehicle.ignition.Drafted = false;
					}

					if (vehicle.Spawned && vehicle.GetStatValue(VehicleStatDefOf.BodyIntegrity) < 0.01f)
					{
						vehicle.Kill(dinfo);
					}
				}

				return false;
			}

			public static float ReduceDamageFromArmor_Reroute(VehicleComponent __instance, ref DamageInfo dinfo,
				out VehicleComponent.Penetration result)
			{
				result = VehicleComponent.Penetration.NonPenetrated;
				DamageArmorCategoryDef armorCategory = dinfo.Def.armorCategory;
				float effectiveArmorPen = Mathf.Max(dinfo.ArmorPenetrationInt, 0.001f);
				float effectiveArmorRating = __instance.ArmorRating(armorCategory, out float _);

				if (dinfo.Def.armorCategory == null || float.IsInfinity(dinfo.ArmorPenetrationInt) ||
				    dinfo.ArmorPenetrationInt > float.MaxValue * 0.001f || effectiveArmorRating < 0.01f)
				{
					result = VehicleComponent.Penetration.Penetrated;
					return dinfo.Amount;
				}

				result = ((!__instance.props.hitbox.fallthrough) ? VehicleComponent.Penetration.NonPenetrated : VehicleComponent.Penetration.Penetrated);

				float blockPercent = Mathf.Clamp01(effectiveArmorRating / effectiveArmorPen);
				bool isFullBlock = blockPercent >= 1f;

				float armorDamage = dinfo.Amount;
				if (isFullBlock)
				{
					armorDamage *= Mathf.Max(settings.ArmorMinDamage,
						effectiveArmorPen / effectiveArmorRating);
					armorDamage /= settings.VehiclePseudoHPMult;
				}
				else
				{
					armorDamage *= Mathf.Max(blockPercent, settings.ArmorMinDamage) *
					               settings.ArmorPennedDamageMult;
				}

				armorDamage *= settings.ArmorDamageMult;

				// If the armor doesn't have enough HP, it blocks only as much as it can.
				// Corrects for the case of block% going below 100, and thus even less blockage.
				if (armorDamage >= __instance.health)
				{
					blockPercent *= __instance.health / armorDamage;
					if (isFullBlock)
						blockPercent /= settings.ArmorPennedDamageMult;
					isFullBlock = false;
					armorDamage = __instance.health + 1;
				}

				if (isFullBlock)
					result = VehicleComponent.Penetration.Deflected;
				else if (blockPercent >= 0.75f)
				{
					result = VehicleComponent.Penetration.Diminished;
					dinfo.BluntIfSharp();
				}
				else if (blockPercent >= 0.5f)
				{
					result = VehicleComponent.Penetration.NonPenetrated;
				}
				else
					result = VehicleComponent.Penetration.Penetrated;
				
				if (!isFullBlock)
				{
					blockPercent *= blockPercent;
				}

				dinfo.SetAmount(dinfo.Amount * (1f - blockPercent));
				dinfo.armorPenetrationInt = dinfo.ArmorPenetrationInt * (1f - blockPercent);

				return armorDamage;
			}
		}

		[HarmonyPatch]
		//[HarmonyPatchCategory("Vehicles")]
		internal static class VehicleComponent_ArmorRating_Patch
		{
			
			public static bool Prepare()
			{
				return TypeOf_VehicleComponent != null;
			}
			
			public static MethodBase TargetMethod()
			{
				return TypeOf_VehicleComponent.GetMethod("ArmorRating");
			}
			
			public static void Postfix(ref float __result, VehicleComponent __instance)
			{
				float armorDamageMult = Mathf.Max(settings.MinDamagedArmorEffectiveness,
					settings.MinDamagedArmorEffectiveness +
					(1f - settings.MinDamagedArmorEffectiveness) * __instance.HealthPercent);
				float armorSharp = OriginalAP(StatDefOf.ArmorRating_Sharp);
				__result *= armorDamageMult;
				if (armorSharp <= 0.0001f)
					return;
				float armorModifier = settings.VehicleArmorCurve.Evaluate(armorSharp) / armorSharp;
				__result *= armorModifier;
				return;


				float OriginalAP(StatDef armorStat)
				{
					float num = __instance.vehicle.GetStatValue(armorStat);
					if (!__instance.SetArmorModifiers.NullOrEmpty())
					{
						foreach (List<StatModifier> value4 in __instance.SetArmorModifiers.Values)
						{
							if (TryGetModifier(value4, out var value2))
							{
								return value2;
							}
						}
					}

					float num2 = num + __instance.vehicle.statHandler.GetUpgradeableStatValue(armorStat);
					StatModifier statModifier =
						__instance.props.armor?.FirstOrDefault((rating) => rating.stat == armorStat);
					if (statModifier != null)
					{
						num = statModifier.value;
						num2 = num;
					}

					if (!__instance.AddArmorModifiers.NullOrEmpty())
					{
						foreach (List<StatModifier> value5 in __instance.AddArmorModifiers.Values)
						{
							if (TryGetModifier(value5, out var value3))
							{
								num2 += value3;
							}
						}
					}

					return num2;

					bool TryGetModifier(List<StatModifier> statModifiers, out float value)
					{
						value = 0f;
						if (statModifiers.NullOrEmpty())
						{
							return false;
						}

						foreach (StatModifier statModifier2 in statModifiers)
						{
							if (statModifier2.stat == armorStat)
							{
								value = statModifier2.value;
								return true;
							}
						}

						return false;
					}
				}
			}
		}
	}
}