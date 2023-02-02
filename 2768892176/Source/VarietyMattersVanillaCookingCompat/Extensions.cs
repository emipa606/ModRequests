namespace VarietyMattersMoreCompat
{
    public static class Extensions
    {
        public static string NoModIdSuffix(this string modId)
        {
            const string steamSuffix = "_steam";
            const string copySuffix = "_copy";
            
            modId = modId.Trim();

            while (true)
            {
                if (modId.EndsWith(steamSuffix))
                {
                    modId = modId.Substring(0, modId.Length - steamSuffix.Length);
                    continue;
                }

                if (modId.EndsWith(copySuffix))
                {
                    modId = modId.Substring(0, modId.Length - copySuffix.Length);
                    continue;
                }

                return modId;
            }
        }
    }
}
