using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.Grammar;

namespace THVanillaPatches
{
    public abstract class ToggleablePatchParent
    {
        public static Harmony HarmonyInstance = new Harmony("ThatHitmann.THVanillaPatches");

        private THPatchDef _def;

        public ToggleablePatchParent(THPatchDef def)
        {
            _def = def;
        }
        
        protected abstract List<PatchInfo> Patches { get; }

        private bool _isEnabled;
        
        public bool IsEnabled 
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                if (value)
                {
                    EnablePatches();
                }
                else
                {
                    DisablePatches();
                }
            }  
        }


        protected virtual void EnablePatches()
        {
            Patches.Do(info => HarmonyInstance.Patch(info.Original, info.Prefix, info.Postfix));
        }

        protected virtual void DisablePatches()
        {
            Patches.Do(info =>
            {
                HarmonyInstance.Unpatch(info.Original, info.Prefix?.method);
                HarmonyInstance.Unpatch(info.Original, info.Postfix?.method);
            });
        }

        public void SyncWithSavedState()
        {
            Scribe_Values.Look(ref _isEnabled, _def.defName, _def.enabledByDefault);
        }

        public void SyncPostLoad()
        {
            if (_isEnabled)
            {
                EnablePatches();
            }
        }
    }

    public record PatchInfo(MethodBase Original, HarmonyMethod Prefix = null, HarmonyMethod Postfix = null);
}