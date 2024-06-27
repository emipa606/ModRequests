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
		private HashSet<int> disableProselyting = new HashSet<int>();
		private static int version = 0;
		private List<int> pawnIdeoDataKey;
		private List<Ideo> pawnIdeoDataValue;

		public DataStorage(World world) : base(world) { }

		public override void ExposeData() {
			base.ExposeData();
			Scribe_Collections.Look(ref pawnIdeoData, "pawnIdeoData", LookMode.Value, LookMode.Reference, ref pawnIdeoDataKey, ref pawnIdeoDataValue);
			Scribe_Collections.Look(ref disableProselyting, "disableProselyting", LookMode.Value);
			if(disableProselyting == null) disableProselyting = new HashSet<int>();
			Scribe_Values.Look(ref version, "version");
		}

		public Ideo GetIdeo(Pawn pawn) => pawnIdeoData.TryGetValue(pawn.thingIDNumber);

		public void SetIdeo(Pawn pawn, Ideo ideo) {
			RemoveDisabled(pawn);
			pawnIdeoData[pawn.thingIDNumber] = ideo;
		}

		public void RemoveIdeo(Pawn pawn) => pawnIdeoData.Remove(pawn.thingIDNumber);

		public void SetDiabled(Pawn pawn) {
			RemoveIdeo(pawn);
			disableProselyting.Add(pawn.thingIDNumber);
		}

		public void RemoveDisabled(Pawn pawn) => disableProselyting.Remove(pawn.thingIDNumber);

		public bool GetDisabled(Pawn pawn) => disableProselyting.Contains(pawn.thingIDNumber);

		public override void FinalizeInit() {
			IntentionalProselytismMod._datastorage = this;
		}

		public static Ideo GetIdeoStatic(Pawn pawn, Pawn pawn2 = null) {
			var targ = IntentionalProselytismMod._datastorage.GetIdeo(pawn);
			if(targ is null) {
				if(pawn2 is null) targ = Faction.OfPlayer.ideos.PrimaryIdeo;
				else targ = pawn2.Ideo;
			}
			return targ;
		}

	}
}
