using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_SelectedLabel : Condition
    {
        public override bool Check(Script script)
        {
            if (string.IsNullOrEmpty(this.group))
            {
                PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError("Condition_SelectedLabel: group must be specified and non-empty");
                return false;
            }
            string selectedLabel;
            if (script.selectedLabelsByGroup.TryGetValue(this.group, out selectedLabel))
            {
                return selectedLabel == this.label;
            }
            return false;
        }

        [JsonRequired]
        public string label = string.Empty;

        [JsonRequired]
        public string group = string.Empty;
    }
}