using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SYS
{
	// Token: 0x02000003 RID: 3
	[HarmonyPatch(typeof(PawnRenderer))]
	[HarmonyPatch("DrawEquipment")]
	public static class DrawEquipment_WeaponBackPatch
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002078 File Offset: 0x00000278
		[HarmonyPrefix]
		public static bool DrawEquipmentPrefix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc)
		{
			if(!SYSMod.instance.Settings.AlwaysVisible)
            {
				return true;
            }
			var flag = ___pawn.Dead || !___pawn.Spawned;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				var flag2 = ___pawn.equipment == null || ___pawn.equipment.Primary == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					var flag3 = ___pawn.CurJob != null && ___pawn.CurJob.def.neverShowWeapon;
					if (flag3)
					{
						result = false;
					}
					else
					{
						var stance_Busy = ___pawn.stances.curStance as Stance_Busy;
						CompWeaponExtention comp = ___pawn.equipment.Primary.GetComp<CompWeaponExtention>();
						CompSheath comp2 = ___pawn.equipment.Primary.GetComp<CompSheath>();
						var flag4 = stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid;
						var flag5 = flag4;
						if (flag5)
						{
							Vector3 vector = rootLoc;
							var hasThing = stance_Busy.focusTarg.HasThing;
							Vector3 a;
							if (hasThing)
							{
								a = stance_Busy.focusTarg.Thing.DrawPos;
							}
							else
							{
								a = stance_Busy.focusTarg.Cell.ToVector3Shifted();
							}
							var num = 0f;
							var flag6 = (a - ___pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f;
							if (flag6)
							{
								num = (a - ___pawn.DrawPos).AngleFlat();
							}
							var flag7 = comp != null && comp.littleDown;
							if (flag7)
							{
								vector.z += -0.2f;
							}
							vector += new Vector3(0f, 0f, 0.4f).RotatedBy(num);
							vector.y += 0.0390625f;
							__instance.DrawEquipmentAiming(___pawn.equipment.Primary, vector, num);
							var flag8 = comp2 != null;
							if (flag8)
							{
								DrawEquipment_WeaponBackPatch.DrawSheath(comp2, ___pawn, rootLoc, comp2.SheathOnlyGraphic);
							}
							result = false;
						}
						else
						{
							var flag9 = (___pawn.carryTracker == null || ___pawn.carryTracker.CarriedThing == null) && (___pawn.Drafted || (___pawn.CurJob != null && ___pawn.CurJob.def.alwaysShowWeapon) || (___pawn.mindState.duty != null && ___pawn.mindState.duty.def.alwaysShowWeapon));
							if (flag9)
							{
								Vector3 vector2 = rootLoc;
								var flag10 = comp != null;
								if (flag10)
								{
									switch (___pawn.Rotation.AsInt)
									{
									case 0:
										vector2 += comp.Props.northOffset.position;
										DrawEquipment_WeaponBackPatch.DrawEquipmentAiming(___pawn.equipment.Primary, vector2, comp.Props.northOffset.angle);
										break;
									case 1:
										vector2 += comp.Props.eastOffset.position;
										vector2.y += 0.0390625f;
										DrawEquipment_WeaponBackPatch.DrawEquipmentAiming(___pawn.equipment.Primary, vector2, comp.Props.eastOffset.angle);
										break;
									case 2:
										vector2 += comp.Props.southOffset.position;
										vector2.y += 0.0390625f;
										DrawEquipment_WeaponBackPatch.DrawEquipmentAiming(___pawn.equipment.Primary, vector2, comp.Props.southOffset.angle);
										break;
									case 3:
										vector2 += comp.Props.westOffset.position;
										vector2.y += 0.0390625f;
										DrawEquipment_WeaponBackPatch.DrawEquipmentAiming(___pawn.equipment.Primary, vector2, comp.Props.westOffset.angle);
										break;
									}
								}
								else
								{
									var flag11 = ___pawn.Rotation == Rot4.South;
									if (flag11)
									{
										Vector3 drawLoc = rootLoc + new Vector3(0f, 0f, -0.22f);
										drawLoc.y += 0.0390625f;
										__instance.DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc, 143f);
									}
									else
									{
										var flag12 = ___pawn.Rotation == Rot4.North;
										if (flag12)
										{
											Vector3 drawLoc2 = rootLoc + new Vector3(0f, 0f, -0.11f);
											__instance.DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc2, 143f);
										}
										else
										{
											var flag13 = ___pawn.Rotation == Rot4.East;
											if (flag13)
											{
												Vector3 drawLoc3 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
												drawLoc3.y += 0.0390625f;
												__instance.DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc3, 143f);
											}
											else
											{
												var flag14 = ___pawn.Rotation == Rot4.West;
												if (flag14)
												{
													Vector3 drawLoc4 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
													drawLoc4.y += 0.0390625f;
													__instance.DrawEquipmentAiming(___pawn.equipment.Primary, drawLoc4, 217f);
												}
											}
										}
									}
								}
								var flag15 = comp2 != null;
								if (flag15)
								{
									DrawEquipment_WeaponBackPatch.DrawSheath(comp2, ___pawn, rootLoc, comp2.SheathOnlyGraphic);
								}
								result = false;
							}
							else
							{
								var flag16 = !___pawn.InBed() && ___pawn.GetPosture() == PawnPosture.Standing;
								if (flag16)
								{
									var flag17 = comp2 != null;
									if (flag17)
									{
										DrawEquipment_WeaponBackPatch.DrawSheath(comp2, ___pawn, rootLoc, comp2.FullGraphic);
									}
								}
								result = false;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000261C File Offset: 0x0000081C
		public static void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
		{
			Material matSingle = eq.Graphic.MatSingle;
			Graphics.DrawMesh(MeshPool.plane10, drawLoc, Quaternion.AngleAxis(aimAngle, Vector3.up), matSingle, 0);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002650 File Offset: 0x00000850
		public static void DrawSheath(Pawn pawn, Thing eq, Vector3 drawLoc, float aimAngle, Graphic graphic)
		{
			var angle = aimAngle % 360f;
			CompSheath compSheath = eq.TryGetComp<CompSheath>();
			var flag = compSheath != null;
			if (flag)
			{
				Graphics.DrawMesh(graphic.MeshAt(pawn.Rotation), drawLoc, Quaternion.AngleAxis(angle, Vector3.up), graphic.MatAt(pawn.Rotation, null), 0);
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000026A8 File Offset: 0x000008A8
		public static void DrawSheath(CompSheath compSheath, Pawn pawn, Vector3 drawLoc, Graphic graphic)
		{
			DrawPosition drawPosition = compSheath.Props.drawPosition;
			DrawPosition drawPosition2 = drawPosition;
			if (drawPosition2 != DrawPosition.Back)
			{
				if (drawPosition2 == DrawPosition.Side)
				{
					var flag = pawn.Rotation == Rot4.South;
					if (flag)
					{
						drawLoc += compSheath.Props.northOffset.position;
						drawLoc.y += 0.03904f;
						DrawEquipment_WeaponBackPatch.DrawSheath(pawn, pawn.equipment.Primary, drawLoc, compSheath.Props.northOffset.angle, graphic);
					}
					else
					{
						var flag2 = pawn.Rotation == Rot4.North;
						if (flag2)
						{
							drawLoc += compSheath.Props.southOffset.position;
							drawLoc.y += 0.03904f;
							DrawEquipment_WeaponBackPatch.DrawSheath(pawn, pawn.equipment.Primary, drawLoc, compSheath.Props.southOffset.angle, graphic);
						}
						else
						{
							var flag3 = pawn.Rotation == Rot4.East;
							if (flag3)
							{
								drawLoc += compSheath.Props.eastOffset.position;
								drawLoc.y += 0.03904f;
								DrawEquipment_WeaponBackPatch.DrawSheath(pawn, pawn.equipment.Primary, drawLoc, compSheath.Props.eastOffset.angle, graphic);
							}
							else
							{
								var flag4 = pawn.Rotation == Rot4.West;
								if (flag4)
								{
									drawLoc += compSheath.Props.westOffset.position;
									drawLoc.y += 0.03904f;
									DrawEquipment_WeaponBackPatch.DrawSheath(pawn, pawn.equipment.Primary, drawLoc, compSheath.Props.westOffset.angle, graphic);
								}
							}
						}
					}
				}
			}
			else
			{
				var flag5 = pawn.Rotation == Rot4.South;
				if (flag5)
				{
					drawLoc += compSheath.Props.southOffset.position;
					DrawEquipment_WeaponBackPatch.DrawSheath(pawn, pawn.equipment.Primary, drawLoc, compSheath.Props.southOffset.angle, graphic);
				}
				else
				{
					var flag6 = pawn.Rotation == Rot4.North;
					if (flag6)
					{
						drawLoc += compSheath.Props.northOffset.position;
						drawLoc.y += 0.03904f;
						DrawEquipment_WeaponBackPatch.DrawSheath(pawn, pawn.equipment.Primary, drawLoc, compSheath.Props.northOffset.angle, graphic);
					}
					else
					{
						var flag7 = pawn.Rotation == Rot4.East;
						if (flag7)
						{
							drawLoc += compSheath.Props.eastOffset.position;
							drawLoc.y += 0.03904f;
							DrawEquipment_WeaponBackPatch.DrawSheath(pawn, pawn.equipment.Primary, drawLoc, compSheath.Props.eastOffset.angle, graphic);
						}
						else
						{
							var flag8 = pawn.Rotation == Rot4.West;
							if (flag8)
							{
								drawLoc += compSheath.Props.westOffset.position;
								drawLoc.y += 0.03904f;
								DrawEquipment_WeaponBackPatch.DrawSheath(pawn, pawn.equipment.Primary, drawLoc, compSheath.Props.westOffset.angle, graphic);
							}
						}
					}
				}
			}
		}

		// Token: 0x04000001 RID: 1
		public const float drawYPosition = 0.0390625f;

		// Token: 0x04000002 RID: 2
		public const float drawSYSYPosition = 0.03904f;

		// Token: 0x04000003 RID: 3
		public const float littleDown = -0.2f;
	}
}
