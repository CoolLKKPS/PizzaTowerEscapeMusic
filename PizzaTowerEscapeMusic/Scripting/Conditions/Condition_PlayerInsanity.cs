using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_PlayerInsanity : ConditionComparableNumber
    {
        public override bool Check(Script script)
        {
            if (GameNetworkManager.Instance == null)
            {
                return false;
            }
            if (GameNetworkManager.Instance.localPlayerController == null)
            {
                return false;
            }
            float num = GameNetworkManager.Instance.localPlayerController.insanityLevel / GameNetworkManager.Instance.localPlayerController.maxInsanityLevel;
            bool flag;
            switch (this.comparisonType)
            {
                case ConditionComparableNumber.ComparisonType.Equals:
                    flag = this.level == num;
                    break;
                case ConditionComparableNumber.ComparisonType.NotEquals:
                    flag = this.level != num;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThan:
                    flag = this.level > num;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThan:
                    flag = this.level < num;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThanOrEquals:
                    flag = this.level >= num;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThanOrEquals:
                    flag = this.level <= num;
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
