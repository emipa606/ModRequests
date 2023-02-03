namespace CaravanDetectedDontCare
{
    public static class Debug
    {
        public static void Log(string message)
        {
#if DEBUG
            Verse.Log.Message($"[{CaravanDetectedDontCareMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
