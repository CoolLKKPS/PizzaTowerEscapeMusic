using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_SelectedLabel : Condition
    {
        public override bool Check(Script script)
        {
            return script.selectedLabel == this.label;
        }

        [JsonRequired]
        public string label = string.Empty;
    }
}