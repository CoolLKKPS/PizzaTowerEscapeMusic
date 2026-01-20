using Newtonsoft.Json;
using System.Collections.Generic;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_CurrentMoon : Condition
    {
        public override bool Check(Script script)
        {
            if (TimeOfDay.Instance == null)
            {
                return false;
            }
            if (StartOfRound.Instance == null)
            {
                return false;
            }
            if (this.isDisabled)
            {
                return false;
            }
            int num;
            if (!Condition_CurrentMoon.moonNameToId.TryGetValue(this.moonName, out num))
            {
                foreach (SelectableLevel selectableLevel in StartOfRound.Instance.levels)
                {
                    if (!Condition_CurrentMoon.moonNameToId.ContainsKey(selectableLevel.PlanetName))
                    {
                        Condition_CurrentMoon.moonNameToId.Add(selectableLevel.PlanetName, selectableLevel.levelID);
                    }
                }
                if (!Condition_CurrentMoon.moonNameToId.TryGetValue(this.moonName, out num))
                {
                    PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError("From CurrentMoon condition: Found no existing level with the name \"" + this.moonName + "\"");
                    this.isDisabled = true;
                    return false;
                }
            }
            return TimeOfDay.Instance.currentLevel.levelID == num;
        }

        private static readonly Dictionary<string, int> moonNameToId = new Dictionary<string, int>();

        [JsonRequired]
        public string moonName = string.Empty;

        private bool isDisabled;
    }
}
