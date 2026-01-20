using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    [JsonConverter(typeof(ConditionConverter))]
    public abstract class Condition
    {
        public abstract bool Check(Script script);

        [JsonRequired]
        public string conditionType = string.Empty;
    }
}
