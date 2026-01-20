using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_MusicWithTagPlaying : Condition
    {
        public override bool Check(Script script)
        {
            return PizzaTowerEscapeMusicManager.MusicManager.GetIsMusicPlaying(this.tag);
        }

        [JsonRequired]
        public string tag = string.Empty;
    }
}
