using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace DTechprinting
{
    public class TechprintGameComponent : Verse.GameComponent
    {

        Game game;

        public TechprintGameComponent(Game game) { this.game = game; }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            Base.UpdateAll();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();

            Base.UpdateAll();
        }
    }
}
