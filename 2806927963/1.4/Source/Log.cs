using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanielRenner.ScenarioLetters
{
	static class Log
	{
		const string LOG_PREFIX = "DanielRenner.Cravings: ";
		static List<int> onceDebugKeys = new List<int>();

		[System.Diagnostics.Conditional("DEBUG")]
		public static void Debug(string message)
		{
			Verse.Log.Message(LOG_PREFIX + message);
		}
		/** sends a log message only for debug compiled versions and only once per start of the software **/
		[System.Diagnostics.Conditional("DEBUG")]
		public static void DebugOnce(string message, object key = null)
		{
			// generate a good hash for the "once"
			int hash;
			if (key != null)
            {
				hash = key.GetHashCode();

			}
			else
            {
				StackFrame caller = new StackFrame(1);
				string alternativeHashSource = message + caller.GetMethod().DeclaringType.Name + caller.GetMethod().Name;
				hash = alternativeHashSource.GetHashCode();
			}
			// only log if not already logged
			if (!Log.onceDebugKeys.Contains(hash))
			{
				Verse.Log.Message(LOG_PREFIX + message);
				Log.onceDebugKeys.Add(hash);
			}
		}

		public static void Message(string message)
		{
			Verse.Log.Message(LOG_PREFIX + message);
		}
		public static void Warning(string message)
		{
			Verse.Log.Warning(LOG_PREFIX + message);
		}
		public static void Error(string message)
		{
			Verse.Log.Error(LOG_PREFIX + message);
		}
		public static void ErrorOnce(string message, int key)
		{
			Verse.Log.ErrorOnce(LOG_PREFIX + message, key);
		}
	}
}
