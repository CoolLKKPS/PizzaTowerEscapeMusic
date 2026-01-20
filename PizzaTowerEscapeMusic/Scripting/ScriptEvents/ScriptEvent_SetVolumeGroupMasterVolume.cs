using Newtonsoft.Json;
using System;

namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    public class ScriptEvent_SetVolumeGroupMasterVolume : ScriptEvent
    {
        public override void Run(Script script)
        {
            if (this.targetTags.Length == 0)
            {
                Script.DefaultVolumeGroup.masterVolume = this.masterVolume;
                return;
            }
            foreach (string text in this.targetTags)
            {
                Script.VolumeGroup volumeGroup;
                if (!script.loadedScriptVolumeGroups.TryGetValue(text, out volumeGroup))
                {
                    PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError("Script Event SetVolumeGroupMasterVolume was called for volume group with tag \"" + text + "\", but there is no volume group of that tag");
                }
                else
                {
                    volumeGroup.masterVolume = this.masterVolume;
                }
            }
        }

        public string[] targetTags = Array.Empty<string>();

        [JsonRequired]
        public float masterVolume;
    }
}
