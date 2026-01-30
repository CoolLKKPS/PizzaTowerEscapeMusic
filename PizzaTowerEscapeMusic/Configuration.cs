using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace PizzaTowerEscapeMusic
{
    public class Configuration
    {
        public Configuration(ConfigFile config)
        {
            this.config = config;
            this.useRandomMapSeed = config.Bind<bool>("General", "UseRandomMapSeed", false, new ConfigDescription("Whether to use the game's random map seed for randomization", null, Array.Empty<object>()));
            this.dontQueue = config.Bind<bool>("General", "DontQueue", true, new ConfigDescription("Whether to not queue randomization events even RandomMapSeed not fully ready, UseRandomMapSeed required", null, Array.Empty<object>()));
            this.scriptingScripts = config.Bind<string>("Scripting", "Scripts", "Default", new ConfigDescription("The names of the JSON script files that will be loaded (Separated by commas, do not put a space after the commas)", null, Array.Empty<object>()));
            this.volumeMaster = config.Bind<float>("Volume", "Master", 0.5f, new ConfigDescription("The volume of the music as a whole, all volumes are scaled by this value", null, Array.Empty<object>()));
            this.selectLabelManually = config.Bind<string>("LabelRandom", "SelectLabelManually", "", new ConfigDescription("Manually select label for groups. Format: Group1:Label1,Group2:Label2 (empty to skip)", null, Array.Empty<object>()));
            this.volumeMaster.SettingChanged += (sender, args) => { };
            this.selectLabelManually.SettingChanged += (sender, args) =>
            {
                PizzaTowerEscapeMusicManager.ScriptManager?.ApplySelectedLabels();
            };
            this.RemoveObsoleteEntries();
        }

        private void RemoveObsoleteEntry(string section, string key)
        {
            ConfigDefinition configDefinition = new ConfigDefinition(section, key);
            this.config.Bind<string>(configDefinition, "", null);
            this.config.Remove(configDefinition);
        }

        private void ReplaceObsoleteEntry<T>(string section, string key, ConfigEntry<T> replacement)
        {
            ConfigDefinition configDefinition = new ConfigDefinition(section, key);
            ConfigEntry<T> configEntry = this.config.Bind<T>(configDefinition, (T)((object)replacement.DefaultValue), null);
            if (!EqualityComparer<T>.Default.Equals(configEntry.Value, (T)((object)replacement.DefaultValue)))
            {
                replacement.Value = configEntry.Value;
            }
            this.config.Remove(configDefinition);
        }

        private void RemoveObsoleteEntries()
        {
            this.RemoveObsoleteEntry("Volume", "InsideFacility");
            this.RemoveObsoleteEntry("Volume", "OutsideFacility");
            this.RemoveObsoleteEntry("Volume", "InsideShip");
            this.RemoveObsoleteEntry("Volume", "CrouchingScale");
            this.RemoveObsoleteEntry("Music", "InsideFacility");
            this.RemoveObsoleteEntry("Music", "OutsideFacility");
            this.RemoveObsoleteEntry("Music", "HeavyWeather");
            this.ReplaceObsoleteEntry<string>("Scripting", "Script", this.scriptingScripts);
            this.config.Save();
        }

        private readonly ConfigFile config;

        internal ConfigEntry<bool> useRandomMapSeed;

        internal ConfigEntry<bool> dontQueue;

        internal ConfigEntry<string> scriptingScripts;

        internal ConfigEntry<float> volumeMaster;

        internal ConfigEntry<string> selectLabelManually;
    }
}
