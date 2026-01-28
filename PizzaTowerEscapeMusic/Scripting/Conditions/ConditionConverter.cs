using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PizzaTowerEscapeMusic.Scripting.Conditions;
using System;

public class ConditionConverter : JsonConverter<Condition>
{
    public override bool CanWrite => false;

    public override Condition ReadJson(JsonReader reader, Type objectType, Condition existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jObject = JObject.Load(reader);
        if (!jObject.TryGetValue("conditionType", out JToken value))
        {
            throw new Exception("Condition type is null!");
        }
        Condition condition = value.Value<string>() switch
        {
            "And" => new Condition_And(),
            "Or" => new Condition_Or(),
            "Not" => new Condition_Not(),
            "Weather" => new Condition_Weather(),
            "PlayerLocation" => new Condition_PlayerLocation(),
            "PlayerAlive" => new Condition_PlayerAlive(),
            "AllPlayersDead" => new Condition_AllPlayersDead(),
            "PlayerHealth" => new Condition_PlayerHealth(),
            "PlayerCrouching" => new Condition_PlayerCrouching(),
            "PlayerInsanity" => new Condition_PlayerInsanity(),
            "PlayerAlone" => new Condition_PlayerAlone(),
            "FiringPlayers" => new Condition_FiringPlayers(),
            "ShipLanded" => new Condition_ShipLanded(),
            "ShipInOrbit" => new Condition_ShipInOrbit(),
            "ShipLeavingAlertCalled" => new Condition_ShipLeavingAlertCalled(),
            "MusicWithTagPlaying" => new Condition_MusicWithTagPlaying(),
            "CurrentMoon" => new Condition_CurrentMoon(),
            "Timer" => new Condition_Timer(),
            "Random" => new Condition_Random(),
            "ApparatusDocked" => new Condition_ApparatusDocked(),
            "TimeOfDay" => new Condition_TimeOfDay(),
            "SelectedLabel" => new Condition_SelectedLabel(),
            _ => throw new Exception($"Condition type \"{value}\" does not exist"),
        };
        serializer.Populate(jObject.CreateReader(), condition);
        return condition;
    }

    public override void WriteJson(JsonWriter writer, Condition value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
