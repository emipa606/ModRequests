using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;

namespace WorksController
{
    [StaticConstructorOnStartup]
    public class WC_PersistableSettingDialog : Window
    {
        //private const int WindowMarginTop = 100;
        //private const int WindowMarginLeft = 100;
        //private const int WindowSizeX = 400;
        //private const int WindowSizeY = 800;
        private const int TOP_AREA_HEIGHT = 30;
        private const int BOTTOM_AREA_HEIGHT = 35;

        public WC_PersistableSettingDialog()
        {
            RefreshSaveFiles();
        }

        private void RefreshSaveFiles()
        {
            files.Clear();
            files.AddRange(WorksControllerUtil.GetSaveFiles());
            //Log.Message(String.Format("get files count = {0}", files.Count));
            files.Sort();
        }

        public void Close()
        {
            RefreshSaveFiles();
            this.Close(true);
        }

        //参考: Dialog_FileList および、それに連なる実装クラス
        public override void DoWindowContents(Rect inRect)
        {
            //top mode selector
            const float MODE_BUTTON_WIDTH = 100f;
            //restore button & text
            Rect rectOutModeRestoreButton = new Rect(inRect.x + ((inRect.width / 2) - MODE_BUTTON_WIDTH), 0, MODE_BUTTON_WIDTH, Mathf.Min(inRect.height, 30f));
            Rect rectModeRestoreButton = rectOutModeRestoreButton;
            rectModeRestoreButton.xMin += 3f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Widgets.DrawBox(rectModeRestoreButton);
            if (selectedMode == Mode.Save)
            {
                if (Widgets.CustomButtonText(ref rectModeRestoreButton, "LoadGameButton".Translate(), passiveColor, Color.white, passiveColor))
                {
                    selectedMode = Mode.Restore;
                    SoundDefOf.TabOpen.PlayOneShot(SoundInfo.OnCamera(MaintenanceType.PerTick));
                    //SoundDefOf.Click.PlayOneShot(SoundInfo.OnCamera(MaintenanceType.PerTick));
                }
            }
            else
            {
                Widgets.DrawBoxSolid(rectModeRestoreButton, activeColor);
                Widgets.Label(rectModeRestoreButton, "LoadGameButton".Translate());
            }
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;

            //save button & text
            Rect rectOutModeSaveButton = new Rect(inRect.x + ((inRect.width / 2)), 0, MODE_BUTTON_WIDTH, Mathf.Min(inRect.height, 30f));
            Rect rectModeSaveButton = rectOutModeSaveButton;
            rectModeSaveButton.xMin += 3f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Widgets.DrawBox(rectModeSaveButton);
            if (selectedMode == Mode.Save)
            {
                Widgets.DrawBoxSolid(rectModeSaveButton, activeColor);
                Widgets.Label(rectModeSaveButton, "SaveGameButton".Translate());
            }
            else
            {
                if (Widgets.CustomButtonText(ref rectModeSaveButton, "SaveGameButton".Translate(), passiveColor, Color.white, passiveColor))
                {
                    selectedMode = Mode.Save;
                    SoundDefOf.TabOpen.PlayOneShot(SoundInfo.OnCamera(MaintenanceType.PerTick));
                }
            }
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;

            //Vector2 rowVector = new Vector2(inRect.width - 16f, 40f);
            float mainRectMarginTop = selectedMode == Mode.Save ? 34f : 0f;
            //mainRectMarginTop += TOP_AREA_HEIGHT;
            mainRectMarginTop = TOP_AREA_HEIGHT;
            Rect mainRect = new Rect(4f, mainRectMarginTop, inRect.width - 8f, inRect.height - (mainRectMarginTop + BOTTOM_AREA_HEIGHT + 4f));
            Widgets.DrawBox(mainRect, 2);
            Widgets.DrawBoxSolid(mainRect, activeColor);

            //save file name text & button
            if (selectedMode == Mode.Save)
            {
                float inputRectWidth = inRect.width * 0.7f;
                Rect inputRect = new Rect(mainRect.x + 4f, mainRectMarginTop + 4f, inputRectWidth - 6f, 30f);
                inputText = Widgets.TextField(inputRect, inputText, FILE_NAME_MAX_LENGTH, InputablePattern);

                if (inputText?.Any() ?? false)
                {
                    Rect buttonRect = new Rect(inputRectWidth + 4f, mainRectMarginTop + 4f, mainRect.width - inputRectWidth - 4f, 30f);
                    if (Widgets.ButtonText(buttonRect, "SaveGameButton".Translate()))
                    {
                        SoundDefOf.Click.PlayOneShot(SoundInfo.OnCamera(MaintenanceType.PerTick));
                        WC_SaveFile saveFile = new WC_SaveFile(inputText, DateTime.Now);
                        AcceptanceReport report = saveFile.TryWrite();
                        ShowFileOperationFinishMessage(report, "SaveGameButton".Translate(), saveFile.NameWithExtension);
                        Close();
                    }
                }
            }

            Rect listRect = new Rect(mainRect.x + 4f, mainRectMarginTop + (selectedMode == Mode.Save ? 38f : 4f), mainRect.width - 8f, mainRect.height - (selectedMode == Mode.Save ? 42f : 8f));
            Widgets.DrawBoxSolid(listRect, Widgets.WindowBGFillColor);

            //file view
            if (this.files.Any())
            {
                EditScrollView(listRect);
            }




            //bottom button
            const float CLOSE_BUTTON_WIDTH = 200f;
            Rect rectOutCloseButton = new Rect(inRect.x + ((inRect.width / 2) - (CLOSE_BUTTON_WIDTH / 2)), inRect.height - BOTTOM_AREA_HEIGHT, CLOSE_BUTTON_WIDTH, Mathf.Min(inRect.height, 30f));
            Rect rectCloseButton = rectOutCloseButton;
            rectCloseButton.xMin += 3f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            if (Widgets.ButtonText(rectCloseButton, "CloseButton".Translate()))
            {
                Close();
            }
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;

        }

        private void ShowFileOperationFinishMessage(AcceptanceReport report, string param1, string param2)
        {
            if (report.Accepted)
            {
                Messages.Message("WC_FileOperationSucceed".Translate(param1, param2), MessageTypeDefOf.NeutralEvent, true);
            }
            else
            {
                Messages.Message(report.Reason, MessageTypeDefOf.NegativeEvent, true);
            }
        }

        private void EditScrollView(Rect inRect)
        {
            const float rowHeight = 45f;
            const float deleteCellWidth = 45f;
            Vector2 rowVector = new Vector2(inRect.width, rowHeight);
            //inRect.height -= 45f;
            float y = rowVector.y;
            float height = (float)this.files.Count() * y;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - 16f, height);
            Rect outRect = inRect;
            //outRect.height -= (TOP_AREA_HEIGHT + BOTTOM_AREA_HEIGHT);
            //Widgets.DrawBox(outRect);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            float columnWidth = inRect.width - 16f;
            float dataWidth = columnWidth - deleteCellWidth;
            Text.Font = GameFont.Small;
            Text.WordWrap = false;

            Color guiBK = GUI.color;

            for (int i = 0; i < this.files.Count; i++)
            {
                WC_SaveFile saveFile = this.files[i];
                float marginTop = inRect.y + (i * rowHeight);

                Rect column = new Rect(inRect.x, marginTop, columnWidth, rowHeight);
                if (i % 2 == 1)
                {
                    Widgets.DrawBoxSolid(column, activeColor);
                }

                GUI.color = DefaultFileTextColor;
                Rect lablCell = new Rect(column.x, column.y, dataWidth * 0.75f, column.height / 2f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lablCell, saveFile.Label);

                GUI.color = guiBK;
                Rect dtCell = new Rect(column.x, column.y + (column.height / 2f), dataWidth * 0.75f, column.height / 2f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(dtCell, saveFile.DisplayDT);

                Rect buttonCell = new Rect(column.x + (dataWidth * 0.75f) + 4f, column.y, dataWidth * 0.25f - 4f, column.height);
                Text.Anchor = TextAnchor.MiddleCenter;
                string buttonText = selectedMode == Mode.Restore ? "LoadGameButton".Translate() : "OverwriteButton".Translate();
                AcceptanceReport report;

                if (Widgets.ButtonText(buttonCell, buttonText))
                {
                    switch (selectedMode)
                    {
                        case Mode.Restore:
                            report = saveFile.TryRestore();
                            ShowFileOperationFinishMessage(report, "LoadGameButton".Translate(), saveFile.NameWithExtension);
                            break;
                        case Mode.Save:
                            
                            report = saveFile.TryWrite();
                            ShowFileOperationFinishMessage(report, "OverwriteButton".Translate(), saveFile.NameWithExtension);
                            break;
                    }
                    Close();
                }

                Rect deleteCell = new Rect(column.x + dataWidth, column.y, deleteCellWidth, column.height);
                if (Widgets.ButtonImage(deleteCell, DeleteX, Color.white, GenUI.SubtleMouseoverColor, true))
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(saveFile.NameWithExtension), delegate
                    {
                        report = saveFile.TryDelete();
                        ShowFileOperationFinishMessage(report, "WC_FileDelete".Translate(), saveFile.NameWithExtension);
                        RefreshSaveFiles();
                    }, true, null));
                }
            }
            Widgets.EndScrollView();
            GUI.color = guiBK;
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private string GetInputablePatternString(params char[] args)
        {
            List<char> invalids = new List<char>();
            invalids.AddRange(Path.GetInvalidFileNameChars());
            foreach (char invalidChar in args)
            {
                if (!invalids.Contains(invalidChar))
                {
                    invalids.Add(invalidChar);
                }
            }
            return String.Format(PATTERN_FORMAT, Regex.Escape(String.Join("", invalids)));
        }

        public Regex InputablePattern
        {
            get
            {
                if (inputablePattern == null)
                {
                    inputablePattern = new Regex(GetInputablePatternString('^', '|'));
                }
                return inputablePattern;
            }
        }

        private const string PATTERN_FORMAT = @"^[^{0}]*$";
        private const int FILE_NAME_MAX_LENGTH = 20;

        private static readonly Color DefaultFileTextColor = new Color(1f, 1f, 0.6f);
        private static readonly Color activeColor = new Color(Widgets.WindowBGFillColor.r * 1.5f, Widgets.WindowBGFillColor.g * 1.5f, Widgets.WindowBGFillColor.b * 1.5f);
        private static readonly Color passiveColor = new Color(Widgets.WindowBGFillColor.r * 0.5f, Widgets.WindowBGFillColor.g * 0.5f, Widgets.WindowBGFillColor.b * 0.5f);
        private static string inputText = "";
        private Regex inputablePattern = null;
        private Mode selectedMode = Mode.Restore;
        protected Vector2 scrollPosition = Vector2.zero;
        protected List<WC_SaveFile> files = new List<WC_SaveFile>();
        private static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);

        private enum Mode
        {
            Restore,
            Save
        }
    }
}
