using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace IntentionalProselytism {
	public class DataStorage : WorldComponent {

		private Dictionary<int, Ideo> pawnIdeoData = new Dictionary<int, Ideo>();
		private List<int> pawnIdeoDataKey;
		private List<Ideo> pawnIdeoDataValue;

		public DataStorage(World world) : base(world) { }

		public override void ExposeData() {
			base.ExposeData();
			Scribe_Collections.Look(ref pawnIdeoData, "pawnIdeoData", LookMode.Value, LookMode.Reference, ref pawnIdeoDataKey, ref pawnIdeoDataValue);
		}

		public Ideo GetIdeo(Pawn pawn) => pawnIdeoData.TryGetValue(pawn.thingIDNumber);

		public void SetIdeo(Pawn pawn, Ideo ideo) => pawnIdeoData[pawn.thingIDNumber] = ideo;

		public void RemoveIdeo(Pawn pawn) => pawnIdeoData.Remove(pawn.thingIDNumber);

		public override void FinalizeInit() {
			IntentionalProselytismMod._datastorage = this;
		}

		public static Ideo GetIdeoStatic(Pawn pawn, Pawn pawn2=null) {
			var targ = Find.World.GetComponent<DataStorage>().GetIdeo(pawn);
			if(targ is null) {
				if(pawn2 is null) targ = Faction.OfPlayer.ideos.PrimaryIdeo;
				else targ = pawn2.Ideo;
			}
			return targ;
		}

	}
}
