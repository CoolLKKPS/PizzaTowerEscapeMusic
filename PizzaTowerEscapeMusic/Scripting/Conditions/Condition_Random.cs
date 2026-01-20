using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_Random : Condition
    {
        public override bool Check(Script script)
        {
            return global::UnityEngine.Random.Range(0f, 1f) <= this.chance;
        }

        [JsonRequired]
        public float chance;
    }
}
