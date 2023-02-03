using DubsBadHygiene;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DBHExtension
{
    public class DefensivePoolProperties : CompProperties
    {
        public float capacity = 36000f;
		public float ticksBetweenDamage = 300;
        public float baseDamage = 5;
		public int fillRate=5000;
		public int drainRate=5000;
		public Vector2 waterDrawSize;
		public Vector3 waterOffset;
		public Vector2 waterColorSize;
		public Vector3 waterColorOffset;
		public string waterTexPath;
        public DefensivePoolProperties()=>compClass=typeof(DefensivePoolComp);
    }

    public class DefensivePoolComp : ThingComp
    {
        public DefensivePoolProperties Props { get { return (DefensivePoolProperties)props;} }
    }
    public class DefensivePool_Building : Building_FillableThing
    {
        private DefensivePoolComp DefensivePoolComp;

		public DefensivePool_Building()
        {
		
			this.flowRate = 0;
        }

		Graphic WaterGraphic;

		public override void Draw()
		{
			
			if (Pool == null)
				Pool = GraphicDatabase.Get<Graphic_Multi>(DefensivePoolComp.Props.waterTexPath, ShaderDatabase.Transparent, DefensivePoolComp.Props.waterDrawSize, Color.white);
			Vector3 pos = DrawPos+DefensivePoolComp.Props.waterOffset;
			pos.y += poolwater_y;

			
			this.Graphic.Draw(DrawPos,Rotation, this, 0f);
			foreach (Pawn pawn in this.occupants)
			{
				Vector3 drawPos = pawn.DrawPos;
				drawPos.y = this.DrawPos.y + swimmerYOffset;
				DubUtils.RenderPawnWithY(pawn, drawPos, 0f);
			}
			if (Pool == null)
			{
				//Pool = GraphicDatabase.Get<Graphic_Multi>(this.def.graphicData.texPath + "_bottom", ShaderDatabase.Transparent, this.def.graphicData.drawSize, Color.white);
			}
			if (this.poolmat == null)
			{
				this.poolmat = Pool.MatAt(base.Rotation, null);
			}
			if (WaterGraphic == null)
			{
				//Graphic water = GraphicDatabase.Get<Graphic_Multi>(this.def.graphicData.texPath + "_w", ShaderDatabase.Transparent, DefensivePoolComp.Props.waterDrawSize, Color.white);
				//water.Draw(DrawPos, Rotation, this);
				WaterGraphic = GraphicDatabase.Get<Graphic_Multi>($"{DefensivePoolComp.Props.waterTexPath}_w", ShaderDatabase.Transparent, DefensivePoolComp.Props.waterColorSize, Color.white);
			}
			if (this.water == null)
			{
				this.water = WaterGraphic.MatAt(base.Rotation, null);
			}

			Vector3 drawPos2 = DrawPos + DefensivePoolComp.Props.waterOffset;
			Quaternion quaternion = this.QuatFromRot(base.Rotation);
			if (this.caustic1 == null)
			{
				this.caustic1 = new Material(GraphicsCache.WaterCaustics);
			}
			Mesh mesh = Pool.MeshAt(base.Rotation);
			Mesh mesh2 = WaterGraphic.MeshAt(base.Rotation);
			propertyBlock.SetColor(ShaderPropertyIDs.Color, this.InstanceAlpha(Mathf.Lerp(1f, pool_alpha, this.FuelPercentOfMax)));
			Graphics.DrawMesh(mesh, drawPos2, quaternion, this.poolmat, 0, null, 0, propertyBlock);
			if (this.FuelPercentOfMax > 0f)
			{
				drawPos2.y += poolwater_y;
				this.caustic1.mainTextureOffset = this.causticOffset1;
				Vector2 vector = new Vector2(DefensivePoolComp.Props.waterDrawSize.x - 2f, DefensivePoolComp.Props.waterDrawSize.y - 2f);
				if (base.Rotation == Rot4.West || base.Rotation == Rot4.East)
				{
					vector = vector.Rotated();
				}
				Matrix4x4 matrix = Matrix4x4.TRS(drawPos2, quaternion, new Vector3(vector.x * caustics_Scale, 0f, vector.y * caustics_Scale));
				Graphics.DrawMesh(MeshPool.plane10, matrix, this.caustic1, 0);
				propertyBlock.SetColor(ShaderPropertyIDs.Color, this.InstanceAlpha(poolwater_alpha * this.FuelPercentOfMax));
				Graphics.DrawMesh(mesh2, drawPos2+DefensivePoolComp.Props.waterColorOffset, quaternion, this.water, 0, null, 0, propertyBlock);
			}

		}

		public override float capacity
        {
            get
            {
                return DefensivePoolComp.Props.capacity;
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            DefensivePoolComp = this.TryGetComp<DefensivePoolComp>();
			pipe=GetComp<CompPipe>();
			
		}
		bool gettingWater;
        int onTick = 0;


		

        public override ushort PathFindCostFor(Pawn p)
        {
			return (ushort)(Flowing? 800:0);
        }

        public override ushort PathWalkCostFor(Pawn p)
        {
			return (ushort)(Flowing ? 800 : 0);
		}

        public override void Tick()
        {
			//Slightly modified version of the original pool code.
			ticker++;
			if (ticker > sackboy)
			{
				ticker = 0;
			}
			causticOffset1.y = Mathf.InverseLerp(0f, sackboy,ticker);
			causticOffset1.x = Mathf.InverseLerp(sackboy, 0f,ticker);
			caustic_col_1.a = Mathf.InverseLerp(sackboy, 0f,ticker);
			caustic_col_2.a = Mathf.InverseLerp(0f, sackboy,ticker);
			if (this.IsHashIntervalTick(5)) {
				float drainRate = DefensivePoolComp.Props.drainRate / 2500f * 5f;
				fuel -= fuel > drainRate ? drainRate : fuel;
				if (fuel < capacity)
				{
					if (Map.weatherManager.curWeather.rainRate > 0.1f && this.OccupiedRect().Any((IntVec3 x) => !x.Roofed(Map)))
					{
						fuel += Map.weatherManager.curWeather.rainRate * 0.1f;
						fuel = Mathf.Clamp(fuel, 0f, capacity);
					}
					if (pipe != null)
					{
						float num = flowRate / 2500f * 5f;
						ContaminationLevel contaminationLevel;
						gettingWater = pipe.pipeNet.PullWater(num, out contaminationLevel) || (flowRate < 0);
						if (gettingWater)
						{
							fuel += num;

							fuel = Mathf.Clamp(fuel, 0f, capacity);
						}
					}
				}
			}

			//Our extended code
			if (onTick == DefensivePoolComp.Props.ticksBetweenDamage)
			{
				onTick = 0;
				return;
			}

			
			onTick++;
			if (Flowing && !IsFull && gettingWater)
			{
				List<Pawn> pawns = Map.mapPawns.AllPawnsSpawned;

				for (int a = 0; a < pawns.Count; a++)
				{
					Pawn pawn = pawns[a];
					if (pawn != null && Map.thingGrid.ThingsAt(pawn.Position).Contains(this))
					{
						pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt,DefensivePoolComp.Props.baseDamage,instigator:this,spawnFilth:false));
					}
				}
			}
        }


		public override string GetInspectString()
		{

			string res = base.GetInspectString();
            if (DebugSettings.godMode)
            {
				res += $"\nDamage: {DefensivePoolComp.Props.baseDamage}";
            }
			return res;
		}

		public bool Flowing
        {
            get
            {
				return flowRate > 0;
            }
        }

		public void ToggleFlow()
        {
			flowRate = !Flowing ? DefensivePoolComp.Props.fillRate : 0;
        }

		public override IEnumerable<Gizmo> GetGizmos()
		{
			/*
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			*/
			if (Flowing || pipe.pipeNet.WaterStorage>DefensivePoolComp.Props.fillRate)
			{
				yield return new Command_Action
				{
					action = new Action(ToggleFlow),
					defaultLabel = Flowing ? "ToggleDefensePoolOff".Translate():"ToggleDefensePoolOn".Translate(),
					defaultDesc = Flowing ? "ToggleDefensePoolOffDesc".Translate() : "ToggleDefensePoolOnDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get(Flowing ? "DBH/UI/LowerFlowRate": "DBH/UI/RaiseFlowRate", true)
				};
			}
			if (DebugSettings.godMode)
			{
				yield return new Command_Action
				{
					action = delegate ()
					{
						fuel = capacity;
					},
					defaultLabel = "Fill"
				};
				yield return new Command_Action
				{
					action = delegate ()
					{
						fuel = 0f;
					},
					defaultLabel = "Empty"
				};
			}
			yield break;
			yield break;
		}
	}
}
