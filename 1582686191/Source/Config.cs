using Verse;

namespace FriendlyFireTweaks
{
    public class Config : ModSettings
    {
        internal bool ShootingLevelAffectsFriendlyFireChance;
        internal int PercentBonus = 5;
        internal bool ProtectPawns = true;
        internal bool ProtectAnimals = true;
        internal bool ProtectBuildings = false;
        internal bool ProtectMineables = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ShootingLevelAffectsFriendlyFireChance, $"Alx_FFT_{nameof(ShootingLevelAffectsFriendlyFireChance)}");
            Scribe_Values.Look(ref PercentBonus, $"Alx_FFT_{nameof(PercentBonus)}", 5);
            Scribe_Values.Look(ref ProtectPawns, $"Alx_FFT_{nameof(ProtectPawns)}", true);
            Scribe_Values.Look(ref ProtectAnimals, $"Alx_FFT_{nameof(ProtectAnimals)}", true);
            Scribe_Values.Look(ref ProtectBuildings, $"Alx_FFT_{nameof(ProtectBuildings)}");
            Scribe_Values.Look(ref ProtectMineables, $"Alx_FFT_{nameof(ProtectMineables)}");
        }
    }
}