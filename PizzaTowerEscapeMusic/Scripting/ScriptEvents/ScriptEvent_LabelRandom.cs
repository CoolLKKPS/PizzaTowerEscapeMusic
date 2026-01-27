using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    public class ScriptEvent_LabelRandom : ScriptEvent
    {
        private static Queue<QueuedLabelRandom> pendingEvents = new Queue<QueuedLabelRandom>();
        private static bool queueProcessing = false;
        private static bool subscribedToSeedReceived = false;

        private struct QueuedLabelRandom
        {
            public Script script;
            public string group;
            public string[] labels;
            public List<(string label, float weight)> entries;
            public float totalWeight;
        }

        public override void Run(Script script)
        {
            if (string.IsNullOrEmpty(this.group))
            {
                PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError("LabelRandom: group must be specified and non-empty");
                return;
            }
            if (this.labels.Length == 0)
            {
                return;
            }

            var entries = new List<(string label, float weight)>();
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

            if (PizzaTowerEscapeMusicManager.Configuration != null && PizzaTowerEscapeMusicManager.Configuration.useRandomMapSeed != null && PizzaTowerEscapeMusicManager.Configuration.useRandomMapSeed.Value)
            {
                if (StartOfRound.Instance == null)
                {
                    PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogDebug("LabelRandom: StartOfRound instance is null, skipping");
                    return;
                }

                bool seedReady = (StartOfRound.Instance.IsHost && StartOfRound.Instance.IsServer) || (Networking.SeedSyncService.SeedReceived && GameEventListener.SyncedrandomMapSeed) || (PizzaTowerEscapeMusicManager.Configuration.dontQueue != null && PizzaTowerEscapeMusicManager.Configuration.dontQueue.Value);
                if (seedReady)
                {
                    ExecuteWithDelay(script, this.group, this.labels, entries, totalWeight);
                }
                else
                {
                    EnqueueEvent(script, this.group, this.labels, entries, totalWeight);
                }
            }
            else
            {
                float randomValue = UnityEngine.Random.Range(0f, totalWeight);
                SelectLabel(script, this.group, entries, totalWeight, randomValue);
            }
        }

        private static void EnqueueEvent(Script script, string group, string[] labels, List<(string label, float weight)> entries, float totalWeight)
        {
            var queued = new QueuedLabelRandom
            {
                script = script,
                group = group,
                labels = labels,
                entries = entries,
                totalWeight = totalWeight
            };
            pendingEvents.Enqueue(queued);
            PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogDebug($"LabelRandom: event queued (waiting for seed). Queue size: {pendingEvents.Count}");

            if (!subscribedToSeedReceived)
            {
                Networking.SeedSyncService.OnSeedReceived += OnSeedReceived;
                subscribedToSeedReceived = true;
            }
        }

        private static void OnSeedReceived()
        {
            PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogDebug("LabelRandom: OnSeedReceived");
            if (GameEventListener.SyncedrandomMapSeed)
            {
                ProcessQueue();
            }
            else
            {
                var gameEventListener = global::PizzaTowerEscapeMusic.GameEventListener.Instance ?? UnityEngine.Object.FindObjectOfType<global::PizzaTowerEscapeMusic.GameEventListener>();
                if (gameEventListener == null)
                {
                    PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError("LabelRandom: cannot find GameEventListener instance, cannot wait for sync");
                    return;
                }
                gameEventListener.StartCoroutine(WaitForSyncedThenProcess());
            }
        }

        private static IEnumerator WaitForSyncedThenProcess()
        {
            while (!GameEventListener.SyncedrandomMapSeed)
            {
                yield return new WaitForSeconds(0.2f);
            }
            PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogDebug("LabelRandom: SyncedrandomMapSeed");
            ProcessQueue();
        }

        private static void ProcessQueue()
        {
            if (queueProcessing) return;
            var gameEventListener = global::PizzaTowerEscapeMusic.GameEventListener.Instance ?? UnityEngine.Object.FindObjectOfType<global::PizzaTowerEscapeMusic.GameEventListener>();
            if (gameEventListener == null)
            {
                PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError("LabelRandom: cannot find GameEventListener instance, cannot start delay coroutine");
                return;
            }
            gameEventListener.StartCoroutine(ProcessQueueWithDelay());
        }

        private static IEnumerator ProcessQueueWithDelay()
        {
            queueProcessing = true;
            yield return new WaitForSeconds(0.2f);
            while (pendingEvents.Count > 0)
            {
                var queued = pendingEvents.Dequeue();
                ExecuteWithDelay(queued.script, queued.group, queued.labels, queued.entries, queued.totalWeight);
            }
            queueProcessing = false;
            PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogDebug("LabelRandom: queue processed");
        }

        private static void ExecuteWithDelay(Script script, string group, string[] labels, List<(string label, float weight)> entries, float totalWeight)
        {
            var gameEventListener = global::PizzaTowerEscapeMusic.GameEventListener.Instance ?? UnityEngine.Object.FindObjectOfType<global::PizzaTowerEscapeMusic.GameEventListener>();
            if (gameEventListener == null)
            {
                PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogError("LabelRandom: cannot find GameEventListener to start coroutine, falling back to immediate selection");
                SelectLabelWithSeed(script, group, labels, entries, totalWeight);
                return;
            }
            gameEventListener.StartCoroutine(DelayedSelection(script, group, labels, entries, totalWeight));
        }

        private static IEnumerator DelayedSelection(Script script, string group, string[] labels, List<(string label, float weight)> entries, float totalWeight)
        {
            yield return new WaitForSeconds(0.2f);
            SelectLabelWithSeed(script, group, labels, entries, totalWeight);
        }

        private static void SelectLabelWithSeed(Script script, string group, string[] labels, List<(string label, float weight)> entries, float totalWeight)
        {
            int labelSeed = StartOfRound.Instance.randomMapSeed;
            int eventHash = group.GetHashCode();
            foreach (string lbl in labels)
                eventHash ^= lbl.GetHashCode();
            int combinedSeed = labelSeed ^ eventHash;
            System.Random eventRandom = new System.Random(combinedSeed);
            float randomValue = (float)(eventRandom.NextDouble() * totalWeight);
            SelectLabel(script, group, entries, totalWeight, randomValue);
        }

        private static void SelectLabel(Script script, string group, List<(string label, float weight)> entries, float totalWeight, float randomValue)
        {
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
                string groupKey = string.IsNullOrEmpty(group) ? "" : group;
                script.selectedLabelsByGroup[groupKey] = selected;
                PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogDebug($"LabelRandom: selected label '{selected}' for group '{groupKey}'");
            }
        }

        public static void ClearQueueAndFlags()
        {
            pendingEvents.Clear();
            queueProcessing = false;
            subscribedToSeedReceived = false;
            Networking.SeedSyncService.OnSeedReceived -= OnSeedReceived;
            PizzaTowerEscapeMusicManager.ScriptManager.Logger.LogDebug("LabelRandom: queue and flags cleared");
        }

        [JsonRequired]
        public string group = string.Empty;

        [JsonRequired]
        public string[] labels = Array.Empty<string>();
    }
}