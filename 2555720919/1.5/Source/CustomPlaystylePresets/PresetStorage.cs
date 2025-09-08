using RimWorld;
using System.Linq;
using Verse;

namespace CustomPlaystylePresets
{
    public class PresetStorage : IExposable, IRenameable
    {
        public string name;
        public Difficulty difficulty;
        public int lastHashCode;
        public bool isDefault;
        public string DefName => "CPP_" + name.Replace(" ", string.Empty);

        public string RenamableLabel
        {
            get
            {
                return name ?? BaseLabel;
            }
            set
            {
                name = value;
                CustomPlaystylePresetsMod.settings.SetDefaultPreset(this);
                var customDifficultyDef = CustomPlaystylePresetsMod.CreateNewDifficultyDef(this);
                DefDatabase<DifficultyDef>.Add(customDifficultyDef);
                CustomPlaystylePresetsMod.settings.AddNewDifficulty(this);
                DrawStorytellerSelectionInterface_Patch.customStorage = this;
                DrawStorytellerSelectionInterface_Patch.customDifficultyDef = customDifficultyDef;
            }
        }

        public string BaseLabel
        {
            get
            {
                var baseName = "Preset";
                int counter = 1;
                while (true)
                {
                    var name = baseName + " " + counter;
                    if (CustomPlaystylePresetsMod.settings.presets?.Any((PresetStorage a) => a.name == name) ?? false)
                    {
                        counter++;
                    }
                    else
                    {
                        return name;
                    }
                }
            }
        }

        public string InspectLabel => RenamableLabel;

        public PresetStorage()
        {

        }
        public PresetStorage(Difficulty source)
        {
            this.difficulty = CopyOf(source);
        }

        public Difficulty CopyOf(Difficulty source)
        {
            var difficulty = new Difficulty();
            source.CopyFieldsTo(difficulty);
            difficulty.anomalyPlaystyleDef = source.anomalyPlaystyleDef;
            difficulty.SetMinThreatPointsCurve();
            this.lastHashCode = source.GetHashCode();
            return difficulty;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Deep.Look(ref difficulty, "difficulty");
            Scribe_Values.Look(ref isDefault, "isDefault");
        }
    }
}

