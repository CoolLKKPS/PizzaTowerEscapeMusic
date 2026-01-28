namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_PlayerAlone : Condition
    {
        public override bool Check(Script script)
        {
            return !(GameNetworkManager.Instance == null) && !(GameNetworkManager.Instance.localPlayerController == null) && GameNetworkManager.Instance.localPlayerController.isPlayerAlone;
        }
    }
}