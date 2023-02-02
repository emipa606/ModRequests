using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Clockwork
{
    public class ThingComp_Windup : ThingComp
    {
        public CompProperties_WindUp Props => (CompProperties_WindUp)props;
        private float assemblyProgress;

        public override void CompTick()
        {
            float num = 1f / (Props.daysToAssembly * 60000f);
            assemblyProgress += num;
            if (assemblyProgress > 1f)
            {
                AssembleWindup();
            }
        }

        public void AssembleWindup()
        {
            try
            {
                PawnGenerationRequest request = new PawnGenerationRequest(Props.spawnPawn, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, newborn: true);
                for (int i = 0; i < parent.stackCount; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    Name name = new NameSingle(Props.spawnPawn.label);
                    pawn.Name = name;
                    if (Props.equipWeapon != null)
                    {
                        ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(Props.equipWeapon);
                        pawn.equipment.AddEquipment(thingWithComps);
                    }
                    PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent);
                }
            }
            finally
            {
                parent.Destroy();
            }
        }

        public override string CompInspectStringExtra()
        {
            return "AssemblyProgress".Translate() + ": " + assemblyProgress.ToStringPercent();
        }

    }

    public class CompProperties_WindUp : CompProperties
    {
        public float daysToAssembly = 0.01f;

        public PawnKindDef spawnPawn;

        public ThingDef equipWeapon;

        public CompProperties_WindUp()
        {
            compClass = typeof(ThingComp_Windup);
        }
    }

    public class MainTabWindow_Windups : MainTabWindow_PawnTable
    {
        private static PawnTableDef pawnTableDef = null;

        protected override PawnTableDef PawnTableDef
        {
            get
            {
                if (pawnTableDef == null)
                {
                    pawnTableDef = DefDatabase<PawnTableDef>.GetNamed("Windups");
                }
                return pawnTableDef;
            }
        }

        protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
                                                      where p.RaceProps.IsMechanoid
                                                      select p;

        public override void PostOpen()
        {
            base.PostOpen();
            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }
    }

    [StaticConstructorOnStartup]
    public class WindupRename : PawnColumnWorker_Label
    {
        public static readonly Texture2D rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename");
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {

            if (Widgets.ButtonImage(rect, rename))
            {
                if (pawn == null || !pawn.Spawned)
                {
                    return;
                }
                if (pawn.Name == null)
                {
                    NameSingle name = (NameSingle)(pawn.Name = new NameSingle(pawn.Label));
                    pawn.Name = name;
                }
                Find.WindowStack.Add(new Dialog_NameWindup(pawn));
            }
            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, (Texture)(object)TexUI.HighlightTex);
            }
            TipSignal tip = pawn.Label;
            tip.text = "RenameWindup".Translate();
            TooltipHandler.TipRegion(rect, tip);
        }

        public override int GetMinWidth(PawnTable table)
        {
            return 30;
        }

        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(30, GetMinWidth(table), GetMaxWidth(table));
        }

        public override int GetMaxWidth(PawnTable table)
        {
            return Mathf.Min(base.GetMaxWidth(table), GetMinWidth(table));
        }
    }
    public class Dialog_NameWindup : Window
    {
        private Pawn pawn;

        private string curName;

        private Name CurPawnName
        {
            get
            {
                return new NameSingle(curName);
            }
        }

        public override Vector2 InitialSize => new Vector2(500f, 175f);

        public Dialog_NameWindup(Pawn pawn)
        {
            this.pawn = pawn;
            curName = pawn.Name.ToStringShort;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            closeOnAccept = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            float height = inRect.height;
            float width = inRect.width;
            bool flag = false;
            if ((int)Event.current.type == 4 && (int)Event.current.keyCode == 13)
            {
                flag = true;
                Event.current.Use();
            }
            Text.Font = GameFont.Medium;
            string text = CurPawnName.ToString().Replace(" '' ", " ");
            Widgets.Label(new Rect(15f, 15f, 500f, 50f), text);
            Text.Font = GameFont.Small;
            string text2 = Widgets.TextField(new Rect(15f, 50f, (width / 2f - 20f), 35f), curName);
            if (text2.Length < 16 && CharacterCardUtility.ValidNameRegex.IsMatch(text2))
            {
                curName = text2;
            }
            if (Widgets.ButtonText(new Rect(width / 2f + 20f, height - 35f, width / 2f - 20f, 35f), "OK".Translate(), drawBackground: true, doMouseoverSound: false) || flag)
            {
                pawn.Name = CurPawnName;
                Find.WindowStack.TryRemove(this);
                Messages.Message("WindupGainsName".Translate(curName), pawn, MessageTypeDefOf.PositiveEvent, historical: false);
            }
        }
    }
}