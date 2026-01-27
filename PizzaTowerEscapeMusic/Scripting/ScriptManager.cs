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
            gameEventListener.OnShipNotInOrbit = (Action)Delegate.Combine(gameEventListener.OnShipNotInOrbit, new Action(delegate
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.ShipNotInOrbit);
            }));
            gameEventListener.OnCurrentMoonChanged = (Action<SelectableLevel>)Delegate.Combine(gameEventListener.OnCurrentMoonChanged, new Action<SelectableLevel>(delegate (SelectableLevel l)
            {
                this.CheckScriptEvents(ScriptEvent.GameEventType.CurrentMoonChanged);
            }));
            this.Logger.LogInfo("Done loading scripts");
        }

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

        private Dictionary<ScriptEvent.GameEventType, float> lastLogTimeByEvent = new Dictionary<ScriptEvent.GameEventType, float>();

        private bool enablelogCooldown = true;

        public readonly List<Script> loadedScripts = new List<Script>();
    }
}
