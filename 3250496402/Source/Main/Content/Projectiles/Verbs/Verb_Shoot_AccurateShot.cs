using Verse;

namespace Kraltech_Industries;

public class Verb_Shoot_AccurateShot : Verb_Shoot
{
    public override float? AimAngleOverride
    {
        get
        {
            float? result;
            if (state != VerbState.Bursting)
                result = (currentTarget.CenterVector3 - caster.DrawPos).AngleFlat();
            else
                result = (currentTarget.CenterVector3 - caster.DrawPos).AngleFlat();
            return result;
        }
    }
}