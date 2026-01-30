using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_Counter : ConditionComparableNumber
    {
        public override bool Check(Script script)
        {
            Script.Counter counter;
            if (!script.activeCounters.TryGetValue(this.counterName, out counter))
            {
                counter = new Script.Counter(this.counterName);
                script.activeCounters.Add(this.counterName, counter);
            }
            counter.count++;

            ConditionComparableNumber.ComparisonType comparison = this.counterComparisonType ?? ConditionComparableNumber.ComparisonType.GreaterThanOrEquals;
            bool result = false;
            switch (comparison)
            {
                case ConditionComparableNumber.ComparisonType.Equals:
                    result = counter.count == this.counterGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.NotEquals:
                    result = counter.count != this.counterGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThan:
                    result = counter.count > this.counterGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThan:
                    result = counter.count < this.counterGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThanOrEquals:
                    result = counter.count >= this.counterGoal;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThanOrEquals:
                    result = counter.count <= this.counterGoal;
                    break;
            }
            return result;
        }

        [JsonRequired]
        public string counterName = string.Empty;

        [JsonRequired]
        public int counterGoal;

        public ConditionComparableNumber.ComparisonType? counterComparisonType;
    }
}
