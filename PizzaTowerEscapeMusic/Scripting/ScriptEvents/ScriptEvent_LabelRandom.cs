using Newtonsoft.Json;
using System;
using UnityEngine;

namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    public class ScriptEvent_LabelRandom : ScriptEvent
    {
        public override void Run(Script script)
        {
            if (this.labels.Length == 0)
            {
                return;
            }

            var entries = new System.Collections.Generic.List<(string label, float weight)>();
            float totalWeight = 0f;

            foreach (string entry in this.labels)
            {
                string[] parts = entry.Split(':');
                if (parts.Length != 2)
                {
                    PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError($"LabelRandom: malformed entry '{entry}', expected 'label:weight'");
                    continue;
                }

                string label = parts[0].Trim();
                if (string.IsNullOrEmpty(label))
                {
                    PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError($"LabelRandom: label is empty '{entry}'");
                    continue;
                }

                if (!float.TryParse(parts[1], out float weight))
                {
                    PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError($"LabelRandom: weight is not a number '{entry}'");
                    continue;
                }

                if (weight <= 0)
                {
                    PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError($"LabelRandom: weight must be positive '{entry}'");
                    continue;
                }

                entries.Add((label, weight));
                totalWeight += weight;
            }

            if (entries.Count == 0)
            {
                return;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float accumulated = 0f;
            string selected = null;

            foreach (var (label, weight) in entries)
            {
                accumulated += weight;
                if (randomValue <= accumulated)
                {
                    selected = label;
                    break;
                }
            }

            if (selected != null)
            {
                script.selectedLabel = selected;
                PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogDebug($"LabelRandom: selected label '{selected}'");
            }
        }

        [JsonRequired]
        public string[] labels = Array.Empty<string>();
    }
}