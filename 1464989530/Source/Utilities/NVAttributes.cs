using System;

namespace NightVision
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NVHasSettingsDependentFieldAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NVSettingsDependentFieldAttribute : Attribute
    {

    }
}
