namespace CompTanker
{
    [HotSwappable]
    public static class ExtensionMethods
    {
        public static string NoModIdSuffix(this string modId)
        {
            while (true)
            {
                if (modId.EndsWith("_steam"))
                {
                    modId = modId.Substring(0, modId.Length - "_steam".Length);
                    continue;
                }

                if (modId.EndsWith("_copy"))
                {
                    modId = modId.Substring(0, modId.Length - "_copy".Length);
                    continue;
                }

                return modId;
            }
        }
    }
}
