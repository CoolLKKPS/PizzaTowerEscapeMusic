using Newtonsoft.Json;
using System;
using System.Linq;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_And : Condition
    {
        public override bool Check(Script script)
        {
            return !this.conditions.Any((Condition c) => !c.Check(script));
        }

        [JsonRequired]
        public Condition[] conditions = Array.Empty<Condition>();
    }
}
