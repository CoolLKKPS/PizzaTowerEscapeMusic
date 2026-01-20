namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    public class ScriptEvent_StopMusic : ScriptEvent
    {
        public override void Run(Script script)
        {
            if (this.targetTags != null)
            {
                foreach (string text in this.targetTags)
                {
                    if (this.instant)
                    {
                        PizzaTowerEscapeMusicManager.MusicManager.StopMusic(text);
                    }
                    else
                    {
                        PizzaTowerEscapeMusicManager.MusicManager.FadeStopMusic(text);
                    }
                }
                return;
            }
            if (this.instant)
            {
                PizzaTowerEscapeMusicManager.MusicManager.StopMusic(null);
                return;
            }
            PizzaTowerEscapeMusicManager.MusicManager.FadeStopMusic(null);
        }

        public string[] targetTags;

        public bool instant;
    }
}
