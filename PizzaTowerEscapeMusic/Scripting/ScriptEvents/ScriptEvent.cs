using Newtonsoft.Json;
using PizzaTowerEscapeMusic.Scripting.Conditions;
using System;
using System.Linq;

namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    [JsonConverter(typeof(ScriptEventConverter))]
    public abstract class ScriptEvent
    {
        public bool CheckConditions(Script script)
        {
            return !this.conditions.Any((Condition c) => !c.Check(script));
        }

        public abstract void Run(Script script);

        public string comment = string.Empty;

        [JsonRequired]
        public string scriptEventType = string.Empty;

        [JsonRequired]
        public ScriptEvent.GameEventType gameEventType;

        public Condition[] conditions = Array.Empty<Condition>();

        public enum GameEventType
        {
            FrameUpdated,
            ShipLanded,
            ShipTakeOff,
            ShipLeavingAlertCalled,
            PlayerDamaged,
            PlayerDied,
            PlayerEnteredFacility,
            PlayerExitedFacility,
            PlayerEnteredShip,
            PlayerExitedShip,
            ApparatusTaken,
            CurrentMoonChanged,
            MeltdownStarted,
            ShipInOrbit,
            ShipNotInOrbit
        }
    }
}
