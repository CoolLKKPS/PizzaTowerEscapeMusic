using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_PlayerFearLevel : ConditionComparableNumber
    {
        public override bool Check(Script script)
        {
            if (StartOfRound.Instance == null)
            {
                return false;
            }
            float fear = StartOfRound.Instance.fearLevel;
            bool flag;
            switch (this.comparisonType)
            {
                case ConditionComparableNumber.ComparisonType.Equals:
                    flag = this.level == fear;
                    break;
                case ConditionComparableNumber.ComparisonType.NotEquals:
                    flag = this.level != fear;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThan:
                    flag = this.level > fear;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThan:
                    flag = this.level < fear;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThanOrEquals:
                    flag = this.level >= fear;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThanOrEquals:
                    flag = this.level <= fear;
                    break;
                default:
                    flag = false;
                    break;
            }
            return flag;
        }

        [JsonRequired]
        public float level;
    }
}