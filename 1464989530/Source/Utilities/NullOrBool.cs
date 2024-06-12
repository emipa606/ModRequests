// Nightvision NightVision NullOrBool.cs
// 
// 19 10 2018
// 
// 19 10 2018

namespace NightVision
{
    /// <summary>
    /// Three valued bool
    /// </summary>
    public enum TriBool : sbyte
    {
        False = 0,
        True = 1,
        Undefined = -1
    }

    /// <summary>
    /// Extensions for the TriBool enum.
    /// </summary>
    public static class TriBoolExtensions
    {
        public static bool IsUndefined(this TriBool val)
        {
            return val == TriBool.Undefined;
        }

        public static bool IsTrue(this TriBool val)
        {
            return val == TriBool.True;
        }

        /// <returns>True if val == (False OR Undefined)</returns>
        public static bool IsNotTrue(this TriBool val)
        {
            return val != TriBool.True;
        }

        public static bool IsFalse(this TriBool val)
        {
            return val == TriBool.False;
        }

        /// <returns>True if val == (True OR Undefined)</returns>
        public static bool IsNotFalse(this TriBool val)
        {
            return val != TriBool.False;
        }

        public static void MakeTrue(this ref TriBool val)
        {
            val = TriBool.True;
        }

        public static void MakeFalse(this ref TriBool val)
        {
            val = TriBool.False;
        }
    }
}