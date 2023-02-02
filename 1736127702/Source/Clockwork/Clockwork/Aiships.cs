using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace Clockwork
{
	public class AirshipLeaving : Skyfaller, IActiveDropPod, IThingHolder
	{
		public int groupID = -1;

		public int destinationTile = -1;

		public TransportPodsArrivalAction arrivalAction;

		public bool createWorldObject = true;

		public WorldObjectDef worldObjectDef;

		private bool alreadyLeft;

		private static List<Thing> tmpActiveDropPods = new List<Thing>();

		public ActiveDropPodInfo Contents
		{
			get
			{
				return ((ActiveDropPod)innerContainer[0]).Contents;
			}
			set
			{
				((ActiveDropPod)innerContainer[0]).Contents = value;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref groupID, "groupID", 0);
			Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
			Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
			Scribe_Values.Look(ref alreadyLeft, "alreadyLeft", defaultValue: false);
			Scribe_Values.Look(ref createWorldObject, "createWorldObject", defaultValue: true);
			Scribe_Defs.Look(ref worldObjectDef, "worldObjectDef");
		}

		protected override void LeaveMap()
		{
			if (alreadyLeft || !createWorldObject)
			{
				base.LeaveMap();
				return;
			}
			if (groupID < 0)
			{
				Log.Error("Drop pod left the map, but its group ID is " + groupID);
				Destroy();
				return;
			}
			if (destinationTile < 0)
			{
				Log.Error("Drop pod left the map, but its destination tile is " + destinationTile);
				Destroy();
				return;
			}
			Lord lord = TransporterUtility.FindLord(groupID, base.Map);
			if (lord != null)
			{
				base.Map.lordManager.RemoveLord(lord);
			}
			TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(worldObjectDef ?? WorldObjectDefOf.TravelingTransportPods);
			//TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("TravellingAirship", true));
			travelingTransportPods.Tile = base.Map.Tile;
			travelingTransportPods.SetFaction(Faction.OfPlayer);
			travelingTransportPods.destinationTile = destinationTile;
			travelingTransportPods.arrivalAction = arrivalAction;
			Find.WorldObjects.Add(travelingTransportPods);
			tmpActiveDropPods.Clear();
			tmpActiveDropPods.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));
			for (int i = 0; i < tmpActiveDropPods.Count; i++)
			{
				AirshipLeaving airshipLeaving = tmpActiveDropPods[i] as AirshipLeaving;
				if (airshipLeaving != null && airshipLeaving.groupID == groupID)
				{
					airshipLeaving.alreadyLeft = true;
					travelingTransportPods.AddPod(airshipLeaving.Contents, justLeftTheMap: true);
					airshipLeaving.Contents = null;
					airshipLeaving.Destroy();
				}
			}
		}
	}
}
