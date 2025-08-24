using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;
using Verse;

namespace StuffableCore.Settings
{
    internal interface IAnim<T>
    {
        void Update(Listing_Standard listing_Standard);

        T CurrentFrame();
    }

    internal class Anim01 : IAnim<string>
    {
        private int tick = 0;
        private int tickRate = 50;

        private int step = 0;
        private string frame = "";
        private static int loopMax = 3;

        public Anim01() { }

        public void Update(Listing_Standard listing_Standard)
        {
            if (tick == tickRate)
            {
                frame += " > ";
                step++;
                if (step > loopMax)
                {
                    frame = "";
                    step = 0;
                }
                tick = 0;
            }
            tick++;
        }

        public string CurrentFrame()
        {
            return frame;
        }
    }

    internal class LandingSettingsPage : BaseSettings, ISettings
    {
        IAnim<string> anim1 = new Anim01();

        public void GetSettings(Listing_Standard listing_Standard)
        {
            listing_Standard.Label(anim1.CurrentFrame());
            listing_Standard.Label("Welcome!");
            listing_Standard.Gap();
            listing_Standard.Label("Select tab to get started.");
            listing_Standard.Gap();
            GetSettingsHeader(listing_Standard);
            anim1.Update(listing_Standard);
        }
    }
}
