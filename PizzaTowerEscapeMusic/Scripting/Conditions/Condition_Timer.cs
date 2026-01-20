using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_Timer : Condition
    {
        public override bool Check(Script script)
        {
            Script.Timer timer;
            if (!script.activeTimers.TryGetValue(this.timerName, out timer))
            {
                timer = new Script.Timer(this.timerName);
                script.activeTimers.Add(this.timerName, timer);
            }
            if (timer.time >= this.timeGoal)
            {
                if (this.resetsTimer)
                {
                    timer.time = 0f;
                }
                return true;
            }
            return false;
        }

        [JsonRequired]
        public string timerName = string.Empty;

        [JsonRequired]
        public float timeGoal;

        public bool resetsTimer = true;
    }
}
