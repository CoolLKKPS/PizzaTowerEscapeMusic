using BepInEx;
using UnityEngine;

namespace PizzaTowerEscapeMusic
{
    [BepInPlugin("bgn.pizzatowerescapemusic", "PizzaTowerEscapeMusic", "2.4.3")]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            GameObject gameObject = new GameObject("PizzaTowerEscapeMusic Manager");
            gameObject.AddComponent<PizzaTowerEscapeMusicManager>().Initialise(base.Logger, base.Config);
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        public const string GUID = "bgn.pizzatowerescapemusic";
    }
}
