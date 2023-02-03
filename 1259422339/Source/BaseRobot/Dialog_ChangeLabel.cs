using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace BaseRobot
{
	// Token: 0x02000016 RID: 22
	public class Dialog_ChangeLabel : Window
	{
		// Token: 0x06000066 RID: 102 RVA: 0x000054B0 File Offset: 0x000036B0
		public Dialog_ChangeLabel(Pawn pawn)
		{
			if (pawn.Name == null)
			{
				curName = pawn.Label;
				pawn.Name = new NameSingle(pawn.Label, false);
			}
			curName = pawn.Name.ToString();
			this.pawn = pawn;
			forcePause = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000067 RID: 103 RVA: 0x0000551C File Offset: 0x0000371C
		private NameSingle CurPawnName
		{
			get
			{
                if (pawn.Name is NameSingle nameSingle)
                {
                    return new NameSingle(curName, false);
                }
                throw new InvalidOperationException();
			}
		}

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x06000068 RID: 104 RVA: 0x00005552 File Offset: 0x00003752
        public override Vector2 InitialSize => new Vector2(500f, 175f);

        // Token: 0x06000069 RID: 105 RVA: 0x00005564 File Offset: 0x00003764
        public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(15f, 15f, 500f, 50f), curName.ToString().Replace(" '' ", " "));
			Text.Font = GameFont.Small;
			var text = Widgets.TextField(new Rect(15f, 50f, (inRect.width / 2f) - 20f, 35f), curName);
			if (text.Length < MaxNameLength)
			{
				curName = text;
			}
			if (Widgets.ButtonText(new Rect((inRect.width / 2f) + 20f, inRect.height - 35f, (inRect.width / 2f) - 20f, 35f), "OK", true, false, true) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
			{
				if (curName.Length < 1)
				{
					curName = pawn.Name.ToString();
				}
				pawn.Name = CurPawnName;
				Find.WindowStack.TryRemove(this, true);
				Messages.Message("RobotGainsName".Translate(new object[]
				{
					curName
				}), pawn, MessageTypeDefOf.PositiveEvent, true);
			}
		}

		// Token: 0x0400003C RID: 60
		private const int MaxNameLength = 16;

		// Token: 0x0400003D RID: 61
		private readonly Pawn pawn;

		// Token: 0x0400003E RID: 62
		private string curName;
	}
}
