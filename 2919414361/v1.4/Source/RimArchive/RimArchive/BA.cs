using RimWorld;
using HarmonyLib;
#pragma warning disable CS1591

namespace RimArchive
{
    /// <summary>
    /// Not what I had done. Maybe will be of use sometime later?
    /// 这是黑历史,别太在意;P
    /// </summary>
    public class BA_RaceProperties
    {
        public bool BA_Student
        {
            get
            {
                return this.intelligence >= Intelligence.BA_Student;
            }
        }

        public Intelligence intelligence { get; private set; }
        public enum Intelligence : byte
        {
            BA_Student
        }

    }

    
}
