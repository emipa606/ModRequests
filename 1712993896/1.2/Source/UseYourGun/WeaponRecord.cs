using System;

/// <summary>
/// Thank you to RunAndGun author "roolo" on Steam for allowing me to use his code!
/// Anything below this point is his code merely slightly changed to work with my assembly!
/// </summary>

namespace UseYourGun
{
    public class WeaponRecord
    {
        public bool isSelected = false;
        public bool isException = false;
        public String label = "";
        public WeaponRecord(bool isSelected, bool isException, String label)
        {
            this.isException = isException;
            this.isSelected = isSelected;
            this.label = label;
        }
        public override string ToString()
        {
            return this.isSelected + "," + this.isException + "," + this.label;
        }
    }
}
