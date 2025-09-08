namespace CustomPlaystylePresets
{
    using System;
    using System.Reflection;
    using Verse;

    /// <summary>
    /// A static class for reflection type functions
    /// </summary>
    public static class Reflection
    {
        public static void CopyFieldsTo(this object source, object destination)
        {
            try
            {
                Type sourceType = source.GetType();
                Type targetType = destination.GetType();
                var targetFields = targetType.GetFields();
                foreach (var targetField in targetFields)
                {
                    if (targetField.IsLiteral && targetField.IsInitOnly is false) // is const
                    {
                        continue;
                    }
                    try
                    {
                        var sourceField = sourceType.GetField(targetField.Name);
                        if (sourceField == null)
                        {
                            continue;
                        }
                        targetField.SetValue(destination, sourceField.GetValue(source));
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}

