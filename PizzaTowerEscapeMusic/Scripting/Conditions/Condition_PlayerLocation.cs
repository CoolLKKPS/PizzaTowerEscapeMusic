using Newtonsoft.Json;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_PlayerLocation : Condition
    {
        public override bool Check(Script script)
        {
            if (GameNetworkManager.Instance == null)
            {
                return false;
            }
            if (GameNetworkManager.Instance.localPlayerController == null)
            {
                return false;
            }
            Condition_PlayerLocation.Location location = this.location;
            bool flag;
            if (location != Condition_PlayerLocation.Location.Ship)
            {
                flag = location == Condition_PlayerLocation.Location.Facility && GameNetworkManager.Instance.localPlayerController.isInsideFactory;
            }
            else
            {
                flag = GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom;
            }
            return flag;
        }

        [JsonRequired]
        public Condition_PlayerLocation.Location location;

        public enum Location
        {
            Ship,
            Facility
        }
    }
}
