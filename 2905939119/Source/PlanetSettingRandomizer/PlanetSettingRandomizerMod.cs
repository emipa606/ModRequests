/*
# Planet Settings Randomizer Rimworld Mod
Author: Dr. Plantabyte (aka Christopher C. Hall)
## CC BY 4.0

This work is licensed on the [Attribution 4.0 International (CC BY 4.0)](https://creativecommons.org/licenses/by/4.0/) Creative Commons License.


### You are free to:

* **Share** — copy and redistribute the material in any medium or format
* **Adapt** — remix, transform, and build upon the material
    for any purpose, even commercially.


### Under the following terms:

* **Attribution** — You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.

* **No additional restrictions** — You may not apply legal terms or technological measures that legally restrict others from doing anything the license permits.

### Guarentees:

The licensor cannot revoke these freedoms as long as you follow the license terms.
 */


using HarmonyLib;
using HugsLib.Logs;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.SceneManagement;
using Verse;

namespace PlanetSettingRandomizer
{

    public class PlanetSettingRandomizerMod : HugsLib.ModBase
    {

        public PlanetSettingRandomizerMod()
        {
            // constructor (invoked by reflection, do not add parameters)
        }
        public override string ModIdentifier
        {
            /*
Each ModBase class needs to have a unique identifier. Provide yours by overriding the ModIdentifier property. The identifier will be used in the settings XML to store your settings, so avoid spaces and special characters. You will get an exception if you provide an improper identifier.
             */
            get
            {
                return "PlanetSettingsRandomizer";
            }
        }
    }

}