namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_ShipLeavingAlertCalled : Condition
    {
        public override bool Check(Script script)
        {
            return !(TimeOfDay.Instance == null) && TimeOfDay.Instance.shipLeavingAlertCalled;
        }
    }
}
