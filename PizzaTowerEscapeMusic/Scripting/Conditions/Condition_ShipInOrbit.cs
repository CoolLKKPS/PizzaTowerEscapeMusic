namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_ShipInOrbit : Condition
    {
        public override bool Check(Script script)
        {
            return !(StartOfRound.Instance == null) && StartOfRound.Instance.inShipPhase;
        }
    }
}