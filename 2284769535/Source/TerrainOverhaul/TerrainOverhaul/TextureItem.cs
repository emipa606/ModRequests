using System;
using System.Collections.Generic;
using System.IO;

namespace TerrainOverhaul
{
    public class TextureItem
    {
        public bool IsEnabled;
        public string RelativePath;
        public string RelativePathDisabled;
        public string EnabledPath;
        public string DisabledPath;

        public bool IsChangePending;

        public string NiceFolderFull;
        public string NiceName;

        private readonly bool initialEnabled;

        public TextureItem(bool enabled)
        {
            initialEnabled = enabled;
            this.IsEnabled = enabled;
        }

        public void CreateNiceNames()
        {
            NiceFolderFull = MakeNiceFolder();
            NiceName = SplitCamelCase(RelativePath.Substring(RelativePath.LastIndexOf('/') + 1));
        }

        private string MakeNiceFolder()
        {
            string[] parts = RelativePath.Split('/');
            return string.Join(" / ", parts, 0, parts.Length - 1) + " /";
        }

        public void Delete()
        {
            try
            {
                if (IsEnabled)
                    File.Delete(EnabledPath);
                else
                    File.Delete(DisabledPath);
            }
            catch { }
        }

        public string ApplyState(HashSet<string> states, out bool didChange)
        {
            bool enabledExists  = File.Exists(EnabledPath);
            bool disabledExists = File.Exists(DisabledPath);

            if (!enabledExists && !disabledExists)
            {
                // Well this is a problem...
                didChange = false;
                return "Texture is missing! Please re-install mod.";
            }

            if (IsEnabled)
            {
                if (states.Contains(RelativePath))
                    states.Remove(RelativePath);

                // Check if enabled is already where it should be.
                if (enabledExists)
                {
                    didChange = false;
                    return null;
                }

                // Turn the disabled path into the enabled path. (re-name)
                File.Move(DisabledPath, EnabledPath);
            }
            else
            {
                if (!states.Contains(RelativePath))
                    states.Add(RelativePath);

                if (disabledExists)
                {
                    didChange = false;
                    return null;
                }

                // Turn the enabled path into disabled.
                File.Move(EnabledPath, DisabledPath);
            }

            IsChangePending = IsEnabled != initialEnabled;
            didChange = true;
            return null;
        }

        public static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Replace('_', ' ').Trim();
        }
    }
}
