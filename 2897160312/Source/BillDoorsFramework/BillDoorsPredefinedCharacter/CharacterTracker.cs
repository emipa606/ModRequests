using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BillDoorsPredefinedCharacter
{
    public class GameComponent_CharacterTracker : GameComponent
    {
        private static GameComponent_CharacterTracker instance;
        public GameComponent_CharacterTracker()
        {
            instance = null;
        }
        public GameComponent_CharacterTracker(Game game)
        {
            instance = null;
        }

        public override void FinalizeInit()
        {
            instance = this;
        }

        public Dictionary<Pawn, PredefinedCharacterParmDef> pawnPDCPairInt = new Dictionary<Pawn, PredefinedCharacterParmDef>();
        public Dictionary<PredefinedCharacterParmDef, Pawn> trackedPawnsInt = new Dictionary<PredefinedCharacterParmDef, Pawn>();

        public static Dictionary<Pawn, PredefinedCharacterParmDef> PawnPDCPair => instance?.pawnPDCPairInt;
        public static Dictionary<PredefinedCharacterParmDef, Pawn> TrackedPawns => instance?.trackedPawnsInt;

        List<Pawn> pawnsList;
        List<PredefinedCharacterParmDef> defList;
        List<Pawn> pawnsList2;
        List<PredefinedCharacterParmDef> defList2;

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref trackedPawnsInt, "TrackedPawns", LookMode.Def, LookMode.Reference, ref defList, ref pawnsList);
            Scribe_Collections.Look(ref pawnPDCPairInt, "PawnPDCPair", LookMode.Reference, LookMode.Def, ref pawnsList2, ref defList2);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (pawnPDCPairInt == null) pawnPDCPairInt = new Dictionary<Pawn, PredefinedCharacterParmDef>();
                if (trackedPawnsInt == null) trackedPawnsInt = new Dictionary<PredefinedCharacterParmDef, Pawn>();
            }
        }
    }

    public class Scroll
    {
        private Vector2 scrollPosition;
        private float scrollViewHeight;
        public void Draw<T>(Rect rect, IEnumerable<T> @enum, Func<Rect, T, bool, float> func, string label)
        {
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, label);
            Text.Anchor = TextAnchor.UpperLeft;
            if (@enum.EnumerableNullOrEmpty()) return;
            List<T> list = @enum.ToList();
            Widgets.BeginGroup(rect);
            Rect outRect = new Rect(0f, 25, rect.width, rect.height - 25);
            Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
            float num = 0f;
            int num2 = 0;
            for (int i = list.Count() - 1; i >= 0; i--)
            {
                num += func(new Rect(rect2.x, num, rect2.width, 30f), list[i], num2 % 2 == 1);
                num2++;
            }
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = num;
            }
            Widgets.EndScrollView();
            Widgets.EndGroup();
        }
    }
}
