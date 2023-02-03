using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BaseRobot
{
    // Token: 0x02000012 RID: 18
    public class MainTabWindow_Robots : MainTabWindow_PawnTable
	{
        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000055 RID: 85 RVA: 0x00005244 File Offset: 0x00003444
        protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
                                                      where p.RaceProps.IsMechanoid
                                                      orderby p.RaceProps.baseBodySize, p.def.label
                                                      select p;

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000056 RID: 86 RVA: 0x000052CB File Offset: 0x000034CB
        protected override PawnTableDef PawnTableDef => RobotPawnTableDefOf.Robots;

        // Token: 0x06000057 RID: 87 RVA: 0x000052D2 File Offset: 0x000034D2
        public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}
}
