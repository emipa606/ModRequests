// BillDoorsFramework.ThingWithComps
using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class ThingWithComps_GraphicDataByTechLevel : Verse.ThingWithComps
    {
        private TechLevel techLevelForGraphic;

        private TechLevel makerTechLevel;

        private Graphic graphicIntTech;

        public override Graphic Graphic
        {
            get
            {
                if (makerTechLevel == TechLevel.Undefined)
                {
                    makerTechLevel = Faction == null ? Faction.OfPlayer.def.techLevel : Faction.def.techLevel;
                }
                if (techLevelForGraphic != makerTechLevel)
                {
                    techLevelForGraphic = makerTechLevel;
                    DefModExtension_GraphicDataByTechLevel modExtension = def.GetModExtension<DefModExtension_GraphicDataByTechLevel>();
                    foreach (GraphicDataAndTechLevelPair graphicData in modExtension.graphicDatas)
                    {
                        if (graphicData.techLevel == techLevelForGraphic)
                        {
                            graphicIntTech = graphicData.graphicData.GraphicColoredFor(this);
                        }
                    }
                }
                return graphicIntTech ?? base.DefaultGraphic;
            }
        }

        public override void PostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
        {
            base.PostGeneratedForTrader(trader, forTile, forFaction);
            if (trader.orbital)
            {
                makerTechLevel = TechLevel.Spacer;
            }
            if (forFaction != null) { makerTechLevel = forFaction.def.techLevel; }
        }

        public override Thing SplitOff(int count)
        {
            ThingWithComps_GraphicDataByTechLevel thing = base.SplitOff(count) as ThingWithComps_GraphicDataByTechLevel;
            thing.makerTechLevel = makerTechLevel;
            return thing;
        }
    }

}
