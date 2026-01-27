namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_AllPlayersDead : Condition
    {
        public override bool Check(Script script)
        {
            return StartOfRound.Instance != null && StartOfRound.Instance.allPlayersDead;
        }
    }
}