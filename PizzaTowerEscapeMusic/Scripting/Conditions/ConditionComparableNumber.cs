using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public abstract class ConditionComparableNumber : Condition
    {
        [JsonRequired]
        public ConditionComparableNumber.ComparisonType comparisonType;

        public enum ComparisonType
        {
            Equals,
            NotEquals,
            GreaterThan,
            LessThan,
            GreaterThanOrEquals,
            LessThanOrEquals
        }
    }
}
