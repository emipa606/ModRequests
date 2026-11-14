using RimWorld;
using UINotIncluded.Widget.Workers;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget.Configs
{
    public class ButtonConfig : ElementConfig
    {
        private string _iconPath;
        private string _label;
        private Button_Worker _worker;

        public string defName;
        public bool forceShow;
        public bool hideLabel;

        private MainButtonDef mainButtonDef;
        public bool minimized;

        public ButtonConfig(string defName)
        {
            this.defName = defName;
            minimized = false;
        }

        public ButtonConfig(MainButtonDef def)
        {
            defName = def.defName;
            mainButtonDef = def;
            Label = def.label;
            IconPath = def.iconPath;
            minimized = def.minimized;
        }

        public ButtonConfig()
        {
        } //Empty constructor to load from ExposeData

        public Texture2D Icon { get; private set; }

        public string IconPath
        {
            get => _iconPath;

            set
            {
                _iconPath = value;
                RefreshIcon();
            }
        }

        public string Label
        {
            get => _label;

            set
            {
                _label = value.CapitalizeFirst();
                RefreshCache();
            }
        }

        public float LabelWidth { get; private set; } = -1f;

        public string ShortenedLabel { get; private set; }

        public float ShortenedLabelWidth { get; private set; } = -1f;

        public override bool Configurable => true;

        public override string SettingLabel
        {
            get
            {
                if (mainButtonDef.buttonVisible) return defName;
                if (forceShow) return defName + " (forced)";
                return defName + " (hidden)";
            }
        }

        public override WidgetWorker Worker
        {
            get
            {
                if (_worker == null) _worker = new Button_Worker(this);
                return _worker;
            }
        }

        public virtual Def Def
        {
            get
            {
                if (mainButtonDef == null) mainButtonDef = DefDatabase<MainButtonDef>.GetNamedSilentFail(defName);
                return mainButtonDef;
            }
        }

        public void RefreshIcon()
        {
            if (_iconPath != null) Icon = ContentFinder<Texture2D>.Get(_iconPath);
            else Icon = null;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref _iconPath, "iconPath");
            Scribe_Values.Look(ref _label, "label");
            Scribe_Values.Look(ref minimized, "minimized");
            Scribe_Values.Look(ref hideLabel, "hideLabel");
            Scribe_Values.Look(ref forceShow, "forceShow");
        }

        public void RefreshCache()
        {
            ShortenedLabel = _label.Shorten();
            var font = Text.Font;
            Text.Font = GameFont.Small;
            LabelWidth = Text.CalcSize(Label).x;
            ShortenedLabelWidth = Text.CalcSize(ShortenedLabel).x;
            Text.Font = font;
        }

        public override void Reset()
        {
            IconPath = ((MainButtonDef)Def).iconPath;
            Label = ((MainButtonDef)Def).label;
            minimized = ((MainButtonDef)Def).minimized;
            hideLabel = false;
            RefreshCache();
        }

        public override bool Equivalent(ElementConfig other)
        {
            if (other.GetType() != typeof(ButtonConfig)) return false;

            return ((ButtonConfig)other).defName == defName;
        }
    }
}