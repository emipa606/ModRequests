using RimWorld;
using Verse;

namespace Necro;

public class CompProperties_ReloadableWeapon : CompProperties_EquippableAbilityReloadable
{

    
    public CompProperties_ReloadableWeapon()
    {
        this.compClass = typeof(CompReloadable_Weapon);
    }

    public bool destroyOnEmpty;
    public bool displayGizmoWhileUndrafted = true;
    public bool displayGizmoWhileDrafted = true;
}
