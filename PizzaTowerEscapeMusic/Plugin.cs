using BepInEx;
using UnityEngine;

namespace PizzaTowerEscapeMusic
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Networking.SeedSyncPatches.ApplyPatches();
            DespawnPropsPatches.ApplyPatches();
            GameObject gameObject = new GameObject("PizzaTowerEscapeMusic Manager");
            gameObject.AddComponent<PizzaTowerEscapeMusicManager>().Initialise(base.Logger, base.Config);
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        // public const string GUID = "bgn.pizzatowerescapemusic";
    }
}
