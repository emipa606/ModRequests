using HugsLib;
using HugsLib.Utils;

namespace KriilMod_CD
{
	public class CombatTrainingController : ModBase
	{
		public static ModLogger staticLogger;

		public override string ModIdentifier => "CombatTrainingFixed";

		protected override bool HarmonyAutoPatch => false;

		public static new ModLogger Logger
		{
			get
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Expected O, but got Unknown
				//IL_0021: Expected O, but got Unknown
				ModLogger result;
				if ((result = staticLogger) == null)
				{
					ModLogger val = new ModLogger("CombatTrainingFixed");
					staticLogger = (ModLogger)(object)val;
					result = (ModLogger)(object)val;
				}
				return result;
			}
		}

		public CombatTrainingController()
		{
		}
	}
}
