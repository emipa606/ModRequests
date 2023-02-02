using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RenameGun
{
    public class GameComponent_RenameManager : GameComponent
    {
        public List<Thing> things;
        public List<CompFixedName> comps;

        public static GameComponent_RenameManager Instance;
        public GameComponent_RenameManager(Game game)
        {
            Instance = this;
        }

        public void Init()
        {
            things ??= new List<Thing>();
            comps ??= new List<CompFixedName>();
            Instance = this;
        }
        public override void LoadedGame()
        {
            base.LoadedGame();
            Init();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            Init();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (RenameGunSettings.allowPawnsToRenameGuns)
            {
                foreach (var comp in comps)
                {
                    if (!comp.fixedName.NullOrEmpty() && RenameGunSettings.alwaysKeepPlayerSetNames)
                    {
                        continue;
                    }
                    if (comp.colonistSetName.NullOrEmpty())
                    {
                        var holdingPawn = comp.HoldingPawn;
                        if (holdingPawn != null)
                        {
                            if (holdingPawn != comp.curPawnHolder)
                            {
                                comp.curPawnHolder = holdingPawn;
                                comp.holdingCounter = 0;
                            }
                            comp.holdingCounter++;
                            if (comp.holdingCounter >= RenameGunSettings.holdingPeriodInDaysForAutoRename * GenDate.TicksPerDay)
                            {
                                comp.AutoRename();
                            }
                        }
                    }
                }
            }

        }

        public void TryAddThing(CompFixedName compFixedName)
        {
            Init();
            if (!things.Contains(compFixedName.parent))
            {
                things.Add(compFixedName.parent);
            }
            comps = things.Select(x => x.TryGetComp<CompFixedName>()).ToList();
        }

        public void RemoveThing(CompFixedName compFixedName)
        {
            things.Remove(compFixedName.parent);
            comps = things.Select(x => x.TryGetComp<CompFixedName>()).ToList();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Instance = this;
            Scribe_Collections.Look(ref things, "things", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Init();
                things = things.Where(x => x.TryGetComp<CompFixedName>() != null).ToList();
                comps = things.Select(x => x.TryGetComp<CompFixedName>()).ToList();
            }
        }
    }
}
