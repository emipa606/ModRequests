using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using VREAndroids;
using Verse.AI;

namespace Psychic_Coiling_VRE_Addon
{
    [StaticConstructorOnStartup]
    public static class HandlePuppeteerCompatibilityConstructor
    {
        static HandlePuppeteerCompatibilityConstructor()
        {
            if (ModsConfig.IsActive("VanillaExpanded.VPE.Puppeteer"))
            {
                PuppeteerHandlerSlave.CheckForAndHandlePuppeteerMod();

                // NOTE: This sucks. This above here? This is garbage. This is the worst, unfortunately it's also the best way I could get this to not throw errors on startup.
                // I hate programming sometimes.
            }
        }
    }
}
