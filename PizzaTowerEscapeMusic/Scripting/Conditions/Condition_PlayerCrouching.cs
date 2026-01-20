namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_PlayerCrouching : Condition
    {
        public override bool Check(Script script)
        {
            return !(GameNetworkManager.Instance == null) && !(GameNetworkManager.Instance.localPlayerController == null) && GameNetworkManager.Instance.localPlayerController.isCrouching;
        }
    }
}
