using System.Collections.Generic;
using HugsLib;
using RimWorld;
using Verse;

namespace BrightFlames
{
    public class FireGlowQueue
    {
        private static readonly ThingDef fireGlowDef = ThingDef.Named("FireGlow");
        private readonly List<Fire> queue = new List<Fire>(32);

        public void RegisterFire(Fire fire)
        {
            queue.Add(fire);
        }

        public void Tick()
        {
            if (Find.TickManager.TicksGame % ModBaseBrightFlames.CheckInterval == 0) SpawnGlows();
        }

        private void SpawnGlows()
        {
            foreach (var fire in queue)
            {
                var map = fire?.Map;
                if (map == null || !fire.Spawned) continue;
                var fireGlow = GenSpawn.Spawn(fireGlowDef, fire.Position, map) as FireGlow;
                fireGlow?.SetFire(fire);
            }
            queue.Clear();
        }
    }

    [StaticConstructorOnStartup]
    public class ModBaseBrightFlames : ModBase
    {
        public override string ModIdentifier => "BrightFlames";

        private static FireGlowQueue queue;
        public static FireGlowQueue Queue { get { return queue ??= new FireGlowQueue(); } }

        //private static Settings settings;

        public override void DefsLoaded()
        {
            //settings = new Settings(Settings);
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            Queue.Tick();
        }

        public const int CheckInterval = 74;
    }
}