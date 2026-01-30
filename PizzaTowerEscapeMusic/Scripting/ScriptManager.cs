using BepInEx.Logging;
using Newtonsoft.Json;
using PizzaTowerEscapeMusic.Scripting.ScriptEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PizzaTowerEscapeMusic.Scripting
{
    public class ScriptManager
    {
        public ScriptManager(string[] scriptNames, GameEventListener gameEventListener)
        {
            this.Logger = BepInEx.Logging.Logger.CreateLogSource("PizzaTowerEscapeMusic ScriptManager");
            this.gameEventListener = gameEventListener;
            Script script = new Script();
            this.loadedScripts.Add(script);
            foreach (string text in scriptNames)
            {
                Script script2 = this.DeserializeScript(text);
                if (script2 != null)
                {
                    script2.Initialise(this.Logger);
                    if (script2.isAddon)
                    {
                        List<Script.VolumeGroup> list = script.volumeGroups.ToList<Script.VolumeGroup>();
                        list.AddRange(script2.volumeGroups);
                        script.volumeGroups = list.ToArray();
                        List<ScriptEvent> list2 = script.scriptEvents.ToList<ScriptEvent>();
                        list2.AddRange(script2.scriptEvents);
                        script.scriptEvents = list2.ToArray();
                    }
                    else
                    {
                        this.loadedScripts.Add(script2);
                    }
                    if (script2.isAddon)
                    {
                        this.Logger.LogInfo("Script (" + text + ") loaded as addon");
                    }
                    else
                    {
                        this.Logger.LogInfo("Script (" + text + ") loaded");
                    }
                }
            }
            script.Initialise(this.Logger);
            gameEventListener.OnFrameUpdate = (Action)Delegate.Combine(gameEventListener.OnFrameUpdate, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.FrameUpdated);
            }));
            gameEventListener.OnShipLanded = (Action)Delegate.Combine(gameEventListener.OnShipLanded, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.ShipLanded);
            }));
            gameEventListener.OnShipTakeOff = (Action)Delegate.Combine(gameEventListener.OnShipTakeOff, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.ShipTakeOff);
            }));
            gameEventListener.OnShipInOrbit = (Action)Delegate.Combine(gameEventListener.OnShipInOrbit, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.ShipInOrbit);
            }));
            gameEventListener.OnShipInOrbit = (Action)Delegate.Combine(gameEventListener.OnShipInOrbit, new Action(delegate
            {
                if (this.pendingManualLabelSelection)
                {
                    this.pendingManualLabelSelection = false;
                    this.ApplySelectedLabels();
                }
            }));
            gameEventListener.OnShipLeavingAlertCalled = (Action)Delegate.Combine(gameEventListener.OnShipLeavingAlertCalled, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.ShipLeavingAlertCalled);
            }));
            gameEventListener.OnPlayerDamaged = (Action)Delegate.Combine(gameEventListener.OnPlayerDamaged, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.PlayerDamaged);
            }));
            gameEventListener.OnPlayerDeath = (Action)Delegate.Combine(gameEventListener.OnPlayerDeath, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.PlayerDied);
            }));
            gameEventListener.OnPlayerEnteredFacility = (Action)Delegate.Combine(gameEventListener.OnPlayerEnteredFacility, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.PlayerEnteredFacility);
            }));
            gameEventListener.OnPlayerExitedFacility = (Action)Delegate.Combine(gameEventListener.OnPlayerExitedFacility, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.PlayerExitedFacility);
            }));
            gameEventListener.OnPlayerEnteredShip = (Action)Delegate.Combine(gameEventListener.OnPlayerEnteredShip, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.PlayerEnteredShip);
            }));
            gameEventListener.OnPlayerExitedShip = (Action)Delegate.Combine(gameEventListener.OnPlayerExitedShip, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.PlayerExitedShip);
            }));
            gameEventListener.OnApparatusTaken = (Action)Delegate.Combine(gameEventListener.OnApparatusTaken, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.ApparatusTaken);
            }));
            gameEventListener.OnMeltdownStarted = (Action)Delegate.Combine((Delegate)gameEventListener.OnMeltdownStarted, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.MeltdownStarted);
            }));
            gameEventListener.OnLevelLoaded = (Action)Delegate.Combine(gameEventListener.OnLevelLoaded, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.LevelLoaded);
            }));
            gameEventListener.OnEndOfGame = (Action)Delegate.Combine(gameEventListener.OnEndOfGame, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.EndOfGame);
            }));
            gameEventListener.OnFiringPlayers = (Action)Delegate.Combine(gameEventListener.OnFiringPlayers, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.FiringPlayers);
            }));
            gameEventListener.OnGameOver = (Action)Delegate.Combine(gameEventListener.OnGameOver, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.GameOver);
            }));
            gameEventListener.OnShipNotInOrbit = (Action)Delegate.Combine(gameEventListener.OnShipNotInOrbit, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.ShipNotInOrbit);
            }));
            gameEventListener.OnCurrentMoonChanged = (Action<SelectableLevel>)Delegate.Combine(gameEventListener.OnCurrentMoonChanged, new Action<SelectableLevel>(delegate (SelectableLevel l)
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.CurrentMoonChanged);
            }));
            this.Logger.LogInfo("Done loading scripts");
            this.ApplySelectedLabels();
        }

        private GameEventListener gameEventListener;

        private bool pendingManualLabelSelection = false;

        private bool selectLabelManuallyValid = true;

        public bool SelectLabelManuallyValid => selectLabelManuallyValid;

        public ManualLogSource Logger { get; private set; }

        private void CheckScriptEvents(ScriptEvent.GameEventType eventType)
        {
            foreach (Script script in this.loadedScripts)
            {
                List<ScriptEvent> list;
                if (script.loadedScriptEvents.TryGetValue(eventType, out list))
                {
                    foreach (ScriptEvent scriptEvent in list)
                    {
                        if (scriptEvent.CheckConditions(script))
                        {
                            bool shouldLog = true;
                            if (this.enablelogCooldown)
                            {
                                float lastLogTime;
                                if (!this.lastLogTimeByEvent.TryGetValue(eventType, out lastLogTime))
                                {
                                    lastLogTime = -15f;
                                }
                                if (UnityEngine.Time.time - lastLogTime >= 15f)
                                {
                                    this.lastLogTimeByEvent[eventType] = UnityEngine.Time.time;
                                }
                                else
                                {
                                    shouldLog = false;
                                }
                            }
                            if (shouldLog)
                            {
                                this.Logger.LogDebug(string.Concat(new string[]
                                {
                                    "Conditions for a script event have been met!\n Script Event Type: ",
                                    scriptEvent.scriptEventType,
                                    string.Format("\n   Game Event Type: {0}", scriptEvent.gameEventType),
                                    "\n           Comment: ",
                                    scriptEvent.comment
                                }));
                            }
                            scriptEvent.Run(script);
                        }
                    }
                }
            }
        }

        private Script DeserializeScript(string name)
        {
            string filePath = CustomManager.GetFilePath("Scripts/" + name + ".json", "DefaultScripts/" + name + ".json");
            if (!File.Exists(filePath))
            {
                this.Logger.LogError("Script \"" + name + "\" does not exist! Make sure you spelt it right the config, and make sure its file extension is \".json\"");
                return null;
            }
            string text = File.ReadAllText(filePath);
            Script script;
            try
            {
                script = JsonConvert.DeserializeObject<Script>(text);
            }
            catch (Exception ex)
            {
                this.Logger.LogError("Failed to deserialize script \"" + name + "\":\n" + ex.Message);
                script = null;
            }
            return script;
        }

        public void UpdateAllScriptTimers(float deltaTime)
        {
            foreach (Script script in this.loadedScripts)
            {
                script.UpdateTimers(deltaTime);
            }
        }

        public void ClearAllScriptTimers()
        {
            foreach (Script script in this.loadedScripts)
            {
                script.ClearTimers();
            }
        }

        public void ClearAllScriptCounters()
        {
            foreach (Script script in this.loadedScripts)
            {
                script.ClearCounters();
            }
        }

        private Dictionary<ScriptEvent.GameEventType, float> lastLogTimeByEvent = new Dictionary<ScriptEvent.GameEventType, float>();

        private bool enablelogCooldown = true;

        public readonly List<Script> loadedScripts = new List<Script>();

        public void ApplySelectedLabels()
        {
            if (StartOfRound.Instance != null && !StartOfRound.Instance.inShipPhase)
            {
                this.pendingManualLabelSelection = true;
                this.Logger.LogInfo("Cannot apply manual label selection when not in ship phase, Changes pending");
                return;
            }
            this.pendingManualLabelSelection = false;
            string configValue = PizzaTowerEscapeMusicManager.Configuration?.selectLabelManually?.Value;
            if (string.IsNullOrWhiteSpace(configValue))
            {
                this.selectLabelManuallyValid = true;
                return;
            }
            var entries = configValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var groupToLabel = new Dictionary<string, string>();
            bool isValid = true;
            foreach (var entry in entries)
            {
                var parts = entry.Split(new char[] { ':' }, 2);
                if (parts.Length != 2)
                {
                    this.Logger.LogWarning($"Invalid selectLabelManually entry: '{entry}', expected Group:Label");
                    isValid = false;
                    continue;
                }
                string group = parts[0].Trim();
                string label = parts[1].Trim();
                if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(label))
                {
                    this.Logger.LogWarning($"Empty group or label in entry: '{entry}'");
                    isValid = false;
                    continue;
                }
                if (groupToLabel.ContainsKey(group))
                {
                    this.Logger.LogWarning($"Duplicate group '{group}' in selectLabelManually configuration, will use label '{label}' (previous label '{groupToLabel[group]}')");
                }
                groupToLabel[group] = label;
            }
            this.selectLabelManuallyValid = isValid;
            foreach (var kvp in groupToLabel)
            {
                string group = kvp.Key;
                string label = kvp.Value;
                foreach (var script in this.loadedScripts)
                {
                    script.selectedLabelsByGroup[group] = label;
                }
                this.Logger.LogDebug($"Applied manual label selection: group='{group}', label='{label}'");
            }
        }
    }
}
