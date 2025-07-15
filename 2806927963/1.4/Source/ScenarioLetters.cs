using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace DanielRenner.ScenarioLetters
{
    [StaticConstructorOnStartup]
    public class ScenarioLetters
    {
        static ScenarioLetters()
        {
            Verse.Log.Message("Mod 'Scenario Letters': loaded");
        }
    }
}
