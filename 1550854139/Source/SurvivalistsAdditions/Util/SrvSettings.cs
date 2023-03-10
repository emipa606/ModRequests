using RimWorld;
using Verse;

namespace SurvivalistsAdditions {

  public class SrvSettings : ModSettings {

    internal static int VinegarBarrel_MaxCapacity = 25;
    internal static int VinegarBarrel_FermentDays = 10;

    internal static int CheeseBarrel_MaxCapacity = 25;
    internal static int CheeseBarrel_AgingDays = 12;

    internal static int Smoker_MaxCapacity = 60;
    internal static int Smoker_SmokeHours = 24;
    internal static int Smoker_TendHours = 2;

    internal static int CharcoalPit_MaxCapacity = 25;
    internal static int CharcoalPit_BurnHours = 8;
    internal static float CharcoalPit_CharcoalPerWoodLog = 3f;

		internal static int Snare_FailChance = 10;
		internal static int Snare_BreakChance = 2;
		internal static bool Snare_AllowPositiveNotification = true;
		internal static bool Snare_AllowNegativeNotification = true;
		internal static NotificationType Snare_NotificationType = NotificationType.Letter;

		internal static float GenStep_PlantDensity = 1f;

    public static int VinegarBarrel_FermentTicks {
      get { return GenDate.TicksPerDay * VinegarBarrel_FermentDays; }
    }

    public static int CheeseBarrel_AgingTicks {
      get { return GenDate.TicksPerDay * CheeseBarrel_AgingDays; }
    }

    public static int Smoker_SmokeTicks {
      get { return GenDate.TicksPerHour * Smoker_SmokeHours; }
    }

    public static int Smoker_TendTicks {
      get { return GenDate.TicksPerHour * Smoker_TendHours; }
    }

    public static int CharcoalPit_BurnTicks {
      get { return GenDate.TicksPerHour * CharcoalPit_BurnHours; }
    }


    public override void ExposeData() {
      base.ExposeData();
      Scribe_Values.Look(ref VinegarBarrel_MaxCapacity, "VinegarBarrel_MaxCapacity", 25);
      Scribe_Values.Look(ref VinegarBarrel_FermentDays, "VinegarBarrel_FermentDays", 10);

      Scribe_Values.Look(ref CheeseBarrel_MaxCapacity, "CheeseBarrel_MaxCapacity", 25);
      Scribe_Values.Look(ref CheeseBarrel_AgingDays, "CheeseBarrel_AgingDays", 12);

      Scribe_Values.Look(ref Smoker_MaxCapacity, "Smoker_MaxCapacity", 60);
      Scribe_Values.Look(ref Smoker_SmokeHours, "Smoker_SmokeHours", 24);
      Scribe_Values.Look(ref Smoker_TendHours, "Smoker_TendHours", 2);

      Scribe_Values.Look(ref CharcoalPit_MaxCapacity, "CharcoalPit_MaxCapacity", 25);
      Scribe_Values.Look(ref CharcoalPit_BurnHours, "CharcoalPit_BurnHours", 8);
      Scribe_Values.Look(ref CharcoalPit_CharcoalPerWoodLog, "CharcoalPit_CharcoalPerWoodLog", 3f);

			Scribe_Values.Look(ref Snare_FailChance, "Snare_FailChance", 10);
			Scribe_Values.Look(ref Snare_BreakChance, "Snare_BreakChance", 2);
			Scribe_Values.Look(ref Snare_AllowPositiveNotification, "Snare_AllowPositiveNotification", true);
			Scribe_Values.Look(ref Snare_AllowNegativeNotification, "Snare_AllowNegativeNotification", true);
			Scribe_Values.Look(ref Snare_NotificationType, "Snare_NotificationType", NotificationType.Letter);

			Scribe_Values.Look(ref GenStep_PlantDensity, "GenStep_PlantDensity", 1f);
    }
  }
}
