using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_TimeOfDay : ConditionComparableNumber
    {
        public override bool Check(Script script)
        {
            if (TimeOfDay.Instance == null)
            {
                return false;
            }
            float num = TimeOfDay.Instance.currentDayTime / TimeOfDay.Instance.totalTime;
            bool flag;
            switch (this.comparisonType)
            {
                case ConditionComparableNumber.ComparisonType.Equals:
                    flag = num == this.time;
                    break;
                case ConditionComparableNumber.ComparisonType.NotEquals:
                    flag = num != this.time;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThan:
                    flag = num > this.time;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThan:
                    flag = num < this.time;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThanOrEquals:
                    flag = num >= this.time;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThanOrEquals:
                    flag = num <= this.time;
                    break;
                default:
                    flag = false;
                    break;
            }
            return flag;
        }

        [JsonRequired]
        public float time;
    }
}
