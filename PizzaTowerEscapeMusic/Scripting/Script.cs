using BepInEx.Logging;
using Newtonsoft.Json;
using PizzaTowerEscapeMusic.Scripting.Conditions;
using PizzaTowerEscapeMusic.Scripting.ScriptEvents;
using System;
using System.Collections.Generic;

namespace PizzaTowerEscapeMusic.Scripting
{
    public class Script
    {
        [JsonIgnore]
        public static Script.VolumeGroup DefaultVolumeGroup { get; private set; } = new Script.VolumeGroup();

        public void Initialise(ManualLogSource logger)
        {
            foreach (ScriptEvent scriptEvent in this.scriptEvents)
            {
                List<ScriptEvent> list;
                if (!this.loadedScriptEvents.TryGetValue(scriptEvent.gameEventType, out list))
                {
                    list = new List<ScriptEvent>(1);
                    this.loadedScriptEvents.Add(scriptEvent.gameEventType, list);
                }
                list.Add(scriptEvent);
            }
            foreach (Script.VolumeGroup volumeGroup in this.volumeGroups)
            {
                if (!this.loadedScriptVolumeGroups.ContainsKey(volumeGroup.tag))
                {
                    this.loadedScriptVolumeGroups.Add(volumeGroup.tag, volumeGroup);
                }
                else
                {
                    logger.LogError("Volume group tag \"" + volumeGroup.tag + "\" was already declared, you cannot have two volume groups with the same tag");
                }
            }
        }

        public Script.VolumeGroup TryGetVolumeGroupOrDefault(string tag)
        {
            if (tag == null || !this.loadedScriptVolumeGroups.ContainsKey(tag))
            {
                return Script.DefaultVolumeGroup;
            }
            return this.loadedScriptVolumeGroups[tag];
        }

        public bool TryGetVolumeGroup(string tag, out Script.VolumeGroup volumeGroup)
        {
            if (tag == null || !this.loadedScriptVolumeGroups.ContainsKey(tag))
            {
                volumeGroup = null;
                return false;
            }
            volumeGroup = this.loadedScriptVolumeGroups[tag];
            return true;
        }

        public void UpdateTimers(float deltaTime)
        {
            foreach (Script.Timer timer in this.activeTimers.Values)
            {
                timer.time += deltaTime;
            }
        }

        public void ClearTimers()
        {
            this.activeTimers.Clear();
        }

        internal void ClearTimer(string timerName)
        {
            this.activeTimers.Remove(timerName);
        }

        public string comment = string.Empty;

        public bool isAddon;

        public Script.VolumeGroup[] volumeGroups = Array.Empty<Script.VolumeGroup>();

        [JsonRequired]
        public ScriptEvent[] scriptEvents = Array.Empty<ScriptEvent>();

        [JsonIgnore]
        public readonly Dictionary<ScriptEvent.GameEventType, List<ScriptEvent>> loadedScriptEvents = new Dictionary<ScriptEvent.GameEventType, List<ScriptEvent>>();

        [JsonIgnore]
        public readonly Dictionary<string, Script.VolumeGroup> loadedScriptVolumeGroups = new Dictionary<string, Script.VolumeGroup>();

        [JsonIgnore]
        public readonly Dictionary<string, Script.Timer> activeTimers = new Dictionary<string, Script.Timer>();

        [JsonIgnore]
        public string selectedLabel
        {
            get
            {
                string value;
                if (this.selectedLabelsByGroup.TryGetValue("", out value))
                    return value;
                return string.Empty;
            }
            set
            {
                this.selectedLabelsByGroup[""] = value;
            }
        }

        [JsonIgnore]
        public readonly Dictionary<string, string> selectedLabelsByGroup = new Dictionary<string, string>();

        public class VolumeRule
        {
            public string comment = string.Empty;

            [JsonRequired]
            public float volume;

            public Condition condition;
        }

        public class VolumeModifier
        {
            public string comment = string.Empty;

            [JsonRequired]
            public float volumeScale;

            public float volumeLerpSpeedScale = 1f;

            public float stoppingVolumeLerpSpeedScale = 1f;

            public Condition condition;
        }

        public class VolumeGroup
        {
            public float GetVolume(Script script)
            {
                float num = 1f;
                foreach (Script.VolumeRule volumeRule in this.volumeRules)
                {
                    if (volumeRule.condition == null || volumeRule.condition.Check(script))
                    {
                        num = volumeRule.volume;
                        break;
                    }
                }
                foreach (Script.VolumeModifier volumeModifier in this.volumeModifiers)
                {
                    if (volumeModifier.condition == null || volumeModifier.condition.Check(script))
                    {
                        num *= volumeModifier.volumeScale;
                    }
                }
                return num * this.masterVolume;
            }

            public float GetVolumeLerpSpeedScale(Script script)
            {
                float scale = 1f;
                foreach (Script.VolumeModifier volumeModifier in this.volumeModifiers)
                {
                    if (volumeModifier.condition == null || volumeModifier.condition.Check(script))
                    {
                        scale *= volumeModifier.volumeLerpSpeedScale;
                    }
                }
                return scale;
            }

            public float GetStoppingVolumeLerpSpeedScale(Script script)
            {
                float scale = 1f;
                foreach (Script.VolumeModifier volumeModifier in this.volumeModifiers)
                {
                    if (volumeModifier.condition == null || volumeModifier.condition.Check(script))
                    {
                        scale *= volumeModifier.stoppingVolumeLerpSpeedScale;
                    }
                }
                return scale;
            }

            public string comment = string.Empty;

            public string tag = string.Empty;

            public float volumeLerpSpeed = 1f;

            public float stoppingVolumeLerpSpeed = 1f;

            public float masterVolume = 1f;

            public Script.VolumeRule[] volumeRules = Array.Empty<Script.VolumeRule>();

            public Script.VolumeModifier[] volumeModifiers = Array.Empty<Script.VolumeModifier>();
        }

        public class Timer
        {
            public Timer(string name)
            {
                this.name = name;
            }

            public string name;

            public float time;
        }
    }
}
