using Newtonsoft.Json;
using System;

namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    public class ScriptEvent_PlayMusic : ScriptEvent
    {
        public override void Run(Script script)
        {
            if (this.musicNames.Length == 0)
            {
                return;
            }
            PizzaTowerEscapeMusicManager.MusicManager.PlayMusic(script, this);
        }

        public bool loop;

        public bool silenceGameMusic = true;

        [JsonRequired]
        public ScriptEvent_PlayMusic.OverlapHandling overlapHandling;

        public string tag;

        [JsonRequired]
        public string[] musicNames = Array.Empty<string>();

        public string[] introMusicNames = Array.Empty<string>();

        public enum OverlapHandling
        {
            IgnoreAll,
            IgnoreTag,
            OverrideAll,
            OverrideTag,
            OverrideFadeAll,
            OverrideFadeTag,
            Overlap
        }
    }
}
