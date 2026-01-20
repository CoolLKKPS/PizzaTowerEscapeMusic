using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class ConditionConverter : JsonConverter<Condition>
    {
        public override Condition ReadJson(JsonReader reader, Type objectType, Condition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jobject = JObject.Load(reader);
            JToken jtoken;
            if (!jobject.TryGetValue("conditionType", out jtoken))
            {
                throw new Exception("Condition type is null!");
            }
            string text = jtoken.Value<string>();
            Condition condition;
            switch (text)
            {
                case "And":
                    condition = new Condition_And();
                    break;
                case "Or":
                    condition = new Condition_Or();
                    break;
                case "Not":
                    condition = new Condition_Not();
                    break;
                case "Weather":
                    condition = new Condition_Weather();
                    break;
                case "PlayerLocation":
                    condition = new Condition_PlayerLocation();
                    break;
                case "PlayerAlive":
                    condition = new Condition_PlayerAlive();
                    break;
                case "PlayerHealth":
                    condition = new Condition_PlayerHealth();
                    break;
                case "PlayerCrouching":
                    condition = new Condition_PlayerCrouching();
                    break;
                case "PlayerInsanity":
                    condition = new Condition_PlayerInsanity();
                    break;
                case "ShipLanded":
                    condition = new Condition_ShipLanded();
                    break;
                case "ShipLeavingAlertCalled":
                    condition = new Condition_ShipLeavingAlertCalled();
                    break;
                case "MusicWithTagPlaying":
                    condition = new Condition_MusicWithTagPlaying();
                    break;
                case "CurrentMoon":
                    condition = new Condition_CurrentMoon();
                    break;
                case "Timer":
                    condition = new Condition_Timer();
                    break;
                case "Random":
                    condition = new Condition_Random();
                    break;
                case "ApparatusDocked":
                    condition = new Condition_ApparatusDocked();
                    break;
                case "TimeOfDay":
                    condition = new Condition_TimeOfDay();
                    break;
                default:
                    throw new Exception(string.Format("Condition type \"{0}\" does not exist", jtoken));
            }
            serializer.Populate(jobject.CreateReader(), condition);
            return condition;
        }

        public override void WriteJson(JsonWriter writer, Condition value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
    }
}
