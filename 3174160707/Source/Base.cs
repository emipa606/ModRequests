using HugsLib;
using Reload.Data;
using Reload.UI;
using RimWorld;
using System.Linq;
using Verse;

namespace Reload
{
    public class Base : ModBase
    {
        public static Base Instance { get; private set; } // Sinlgeton

        public override string ModIdentifier => "NKMReload";

        StockDatabase _stockDataStorage;

        public StockDatabase StockDataStorage => _stockDataStorage;

        public Base()
        {
            Instance = this;
        }
        public override void MapComponentsInitializing(Map map)
        {
            base.MapComponentsInitializing(map);
            _stockDataStorage = Find.World.GetComponent<StockDatabase>();
        }
        public override void WorldLoaded()
        {
            base.WorldLoaded();
            if (_stockDataStorage == null)
                Find.World.GetComponent<StockDatabase>();
        }
        public override void DefsLoaded()
        {
            base.DefsLoaded();
            GenerateDefs();
            Setting.Init();
        }
        void GenerateDefs()
        {
            PawnTableDef assignTableDef = DefDatabase<PawnTableDef>.AllDefs.First((PawnTableDef def) => def.defName == "Assign");
            PawnColumnDef column = new PawnColumnDef
            {
                defName = $"StockColumn",
                label = $"StockPolicy".Translate(),
                workerClass = typeof(PawnColumnWorker_Stock),
                sortable = false
            };
            this.ModContentPack.AddDef(column);
            DefGenerator.AddImpliedDef<PawnColumnDef>(column);
            int index = assignTableDef.columns.FindIndex(x => x.defName == "Carry");
            assignTableDef.columns.Insert(index, column);
        }
    }
}