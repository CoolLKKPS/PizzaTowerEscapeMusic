namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    public class ScriptEvent_ResetTimers : ScriptEvent
    {
        public override void Run(Script script)
        {
            if (this.targetTimerNames == null)
            {
                script.ClearTimers();
                return;
            }
            foreach (string text in this.targetTimerNames)
            {
                script.ClearTimer(text);
            }
        }

        public string[] targetTimerNames;
    }
}
