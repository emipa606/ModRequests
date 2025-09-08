using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CustomPlaystylePresets
{
	public class Dialog_SetPresetName : Dialog_Rename<PresetStorage>
	{
        public Dialog_SetPresetName(PresetStorage renaming) : base(renaming)
        {
        }

        public override AcceptanceReport NameIsValid(string name)
		{
			AcceptanceReport result = base.NameIsValid(name);
			if (!result.Accepted)
			{
				return result;
			}
			if (CustomPlaystylePresetsMod.settings.presets?.Any((PresetStorage a) => a != renaming && a.name == name) ?? false)
			{
				return "NameIsInUse".Translate();
			}
			return true;
		}
	}
}