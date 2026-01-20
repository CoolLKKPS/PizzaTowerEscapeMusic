using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_Not : Condition
    {
        public override bool Check(Script script)
        {
            return this.condition == null || !this.condition.Check(script);
        }

        [JsonRequired]
        public Condition condition;
    }
}
