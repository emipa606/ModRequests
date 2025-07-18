using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Kraltech_Industries;

[StaticConstructorOnStartup]
public class WeatherEvent_Mjolnir_ThunderStrike : WeatherEvent_LightningFlash
{
    private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

    private Mesh boltMesh;

    private IntVec3 strikeLoc = IntVec3.Invalid;

    static WeatherEvent_Mjolnir_ThunderStrike()
    {
    }

    public WeatherEvent_Mjolnir_ThunderStrike(Map map) : base(map)
    {
    }

    public WeatherEvent_Mjolnir_ThunderStrike(Map map, IntVec3 forcedStrikeLoc) : base(map)
    {
        strikeLoc = forcedStrikeLoc;
    }

    public override void FireEvent()
    {
        WeatherEvent_LightningStrike.DoStrike(strikeLoc, map, ref boltMesh);
    }

    public static void DoStrike(IntVec3 strikeLoc, Map map, ref Mesh boltMesh)
    {
        SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);
        if (!strikeLoc.IsValid) strikeLoc = CellFinderLoose.RandomCellWith(sq => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);
        boltMesh = LightningBoltMeshPool.RandomBoltMesh;
        if (!strikeLoc.Fogged(map))
        {
            GenExplosion.DoExplosion(strikeLoc, map, 1.9f, DamageDefOf.ThunderWaveUlt, null);
            var loc = strikeLoc.ToVector3Shifted();
            for (var i = 0; i < 4; i++)
            {
                FleckMaker.ThrowSmoke(loc, map, 1.5f);
                FleckMaker.ThrowMicroSparks(loc, map);
                FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
            }
        }

        var info = SoundInfo.InMap(new TargetInfo(strikeLoc, map));
        SoundDefOf.Thunder_OnMap.PlayOneShot(info);
    }

    public override void WeatherEventDraw()
    {
        Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, LightningBrightness), 0);
    }
}