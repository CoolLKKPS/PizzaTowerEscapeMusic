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

            ConditionComparableNumber.ComparisonType comparison = this.timerComparisonType ?? ConditionComparableNumber.ComparisonType.GreaterThanOrEquals;
            bool result = false;
            switch (comparison)
            {
                case ConditionComparableNumber.ComparisonType.Equals:
                    result = timer.time == this.timeGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.NotEquals:
                    result = timer.time != this.timeGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThan:
                    result = timer.time > this.timeGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThan:
                    result = timer.time < this.timeGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThanOrEquals:
                    result = timer.time >= this.timeGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThanOrEquals:
                    result = timer.time <= this.timeGoal;
                    break;
            }
            return result;
        }

        [JsonRequired]
        public string timerName = string.Empty;

        [JsonRequired]
        public float timeGoal;

        public ConditionComparableNumber.ComparisonType? timerComparisonType;
    }
}
