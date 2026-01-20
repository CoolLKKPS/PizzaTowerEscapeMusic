using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_PlayerHealth : ConditionComparableNumber
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
            int health = GameNetworkManager.Instance.localPlayerController.health;
            bool flag;
            switch (this.comparisonType)
            {
                case ConditionComparableNumber.ComparisonType.Equals:
                    flag = health == this.value;
                    break;
                case ConditionComparableNumber.ComparisonType.NotEquals:
                    flag = health != this.value;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThan:
                    flag = health > this.value;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThan:
                    flag = health < this.value;
                    break;
                case ConditionComparableNumber.ComparisonType.GreaterThanOrEquals:
                    flag = health >= this.value;
                    break;
                case ConditionComparableNumber.ComparisonType.LessThanOrEquals:
                    flag = health <= this.value;
                    break;
                default:
                    flag = false;
                    break;
            }
            return flag;
        }

        [JsonRequired]
        public int value;
    }
}
