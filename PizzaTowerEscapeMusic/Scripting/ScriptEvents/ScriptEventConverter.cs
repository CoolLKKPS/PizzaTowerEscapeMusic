using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PizzaTowerEscapeMusic.Scripting.ScriptEvents
{
    public class ScriptEventConverter : JsonConverter<ScriptEvent>
    {
        public override ScriptEvent ReadJson(JsonReader reader, Type objectType, ScriptEvent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jobject = JObject.Load(reader);
            JToken jtoken;
            if (!jobject.TryGetValue("scriptEventType", out jtoken))
            {
                throw new Exception("scriptEventType type is null!");
            }
            string text = jtoken.Value<string>();
            ScriptEvent scriptEvent;
            if (!(text == "PlayMusic"))
            {
                if (!(text == "StopMusic"))
                {
                    if (!(text == "ResetTimers"))
                    {
                        if (!(text == "ResetCounters"))
                        {
                            if (!(text == "LabelRandom"))
                            {
                                if (!(text == "SetVolumeGroupMasterVolume"))
                                {
                                    throw new Exception(string.Format("Condition type \"{0}\" does not exist", jtoken));
                                }
                                scriptEvent = new ScriptEvent_SetVolumeGroupMasterVolume();
                            }
                            else
                            {
                                scriptEvent = new ScriptEvent_LabelRandom();
                            }
                        }
                        else
                        {
                            scriptEvent = new ScriptEvent_ResetCounters();
                        }
                    }
                    else
                    {
                        scriptEvent = new ScriptEvent_ResetTimers();
                    }
                }
                else
                {
                    scriptEvent = new ScriptEvent_StopMusic();
                }
            }
            else
            {
                scriptEvent = new ScriptEvent_PlayMusic();
            }
            ScriptEvent scriptEvent2 = scriptEvent;
            serializer.Populate(jobject.CreateReader(), scriptEvent2);
            return scriptEvent2;
        }

        public override void WriteJson(JsonWriter writer, ScriptEvent value, JsonSerializer serializer)
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
