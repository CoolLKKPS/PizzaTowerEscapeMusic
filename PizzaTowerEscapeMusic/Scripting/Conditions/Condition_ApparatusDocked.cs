namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_ApparatusDocked : Condition
    {
        public override bool Check(Script script)
        {
            return GameEventListener.IsApparatusDocked();
        }
    }
}
