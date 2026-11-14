using System;

namespace UINotIncluded
{
    public enum DesignationConfig : byte
    {
        hidden,
        left,
        main,
        right
    }

    public static class DesignationConfigExtension
    {
        public static string ToStringHuman(this DesignationConfig designation)
        {
            switch (designation)
            {
                case DesignationConfig.hidden:
                    return "Hidden";

                case DesignationConfig.left:
                    return "Left Side";

                case DesignationConfig.main:
                    return "Main";

                case DesignationConfig.right:
                    return "Right Side";

                default:
                    throw new NotImplementedException();
            }
        }
    }
}