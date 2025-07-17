using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace THVanillaPatches
{
    public class THPatchDef: Def
    {
        public bool IsEnabled => PatchGenerator.IsEnabled;
        
        public Type patchSource;
        public ToggleablePatchParent PatchGenerator => _patchGenerator ??= (ToggleablePatchParent)Activator.CreateInstance(patchSource, this);
        private ToggleablePatchParent _patchGenerator;
        public bool enabledByDefault = true;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError;
            }
            if (patchSource == null)
            {
                yield return "Patch source class not found.";
            }
        }
    }
}