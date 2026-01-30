namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    public class ScriptEvent_ResetCounters : ScriptEvent
    {
        public override void Run(Script script)
        {
            if (this.targetCounterNames == null)
            {
                script.ClearCounters();
                return;
            }
            foreach (string counterName in this.targetCounterNames)
            {
                script.ClearCounter(counterName);
            }
        }

        public string[] targetCounterNames;
    }
}