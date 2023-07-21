using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
    // Token: 0x02000A8D RID: 2701
    [StaticConstructorOnStartup]
    public class WeatherOverlay_fastWind : SkyOverlay
    {
        // Token: 0x06003F7D RID: 16253 RVA: 0x0015299C File Offset: 0x00150B9C
        public WeatherOverlay_fastWind()
        {
            this.worldOverlayMat = WeatherOverlay_fastWind.FogOverlayWorld;
            this.worldOverlayPanSpeed1 = 0.05f;
            this.worldOverlayPanSpeed2 = 0.04f;
            this.worldPanDir1 = new Vector2(1f, 1f);
            this.worldPanDir2 = new Vector2(1f, -1f);
        }

        // Token: 0x040024C3 RID: 9411
        private static readonly Material FogOverlayWorld = MatLoader.LoadMat("Weather/FogOverlayWorld", -1);
    }
}
