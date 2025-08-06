using RimWorld;
using Verse;

[DefOf]
public static class MaliSoundDefOf
{
	public static SoundDef LightFire;
	public static SoundDef Extinguish;
	static MaliSoundDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MaliSoundDefOf));
	}
}
