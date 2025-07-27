using System;
using System.Diagnostics;
using Verse;

namespace CM_Callouts
{
	// Token: 0x0200000B RID: 11
	public static class Logger
	{
		// Token: 0x06000021 RID: 33 RVA: 0x000031AC File Offset: 0x000013AC
		public static void MessageFormat(object caller, string message, params object[] stuff)
		{
			bool showDebugLogMessages = CalloutMod.settings.showDebugLogMessages;
			if (showDebugLogMessages)
			{
				message = string.Concat(new string[]
				{
					caller.GetType().ToString(),
					".",
					new StackTrace().GetFrame(1).GetMethod().Name,
					" - ",
					message
				});
				Log.Message(string.Format(message, stuff));
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00003220 File Offset: 0x00001420
		public static void MessageFormat(string message, params object[] stuff)
		{
			bool showDebugLogMessages = CalloutMod.settings.showDebugLogMessages;
			if (showDebugLogMessages)
			{
				message = new StackTrace().GetFrame(1).GetMethod().Name + " - " + message;
				Log.Message(string.Format(message, stuff));
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003270 File Offset: 0x00001470
		public static void WarningFormat(object caller, string message, params object[] stuff)
		{
			bool warningEnabled = Logger.WarningEnabled;
			if (warningEnabled)
			{
				message = string.Concat(new string[]
				{
					caller.GetType().ToString(),
					".",
					new StackTrace().GetFrame(1).GetMethod().Name,
					" - ",
					message
				});
				Log.Warning(string.Format(message, stuff));
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000032E0 File Offset: 0x000014E0
		public static void ErrorFormat(object caller, string message, params object[] stuff)
		{
			bool errorEnabled = Logger.ErrorEnabled;
			if (errorEnabled)
			{
				message = string.Concat(new string[]
				{
					caller.GetType().ToString(),
					".",
					new StackTrace().GetFrame(1).GetMethod().Name,
					" - ",
					message
				});
				Log.Error(string.Format(message, stuff));
			}
		}

		// Token: 0x04000039 RID: 57
		public static bool WarningEnabled = true;

		// Token: 0x0400003A RID: 58
		public static bool ErrorEnabled = true;
	}
}
