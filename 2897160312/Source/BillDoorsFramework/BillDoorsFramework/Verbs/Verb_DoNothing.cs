using Verse;

namespace BillDoorsFramework
{
    public class Verb_DoNothing : Verb
    {
        protected override bool TryCastShot()
        {
            return true;
        }
    }
}
