using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;

namespace PizzaTowerEscapeMusic
{
    [HarmonyPatch]
    internal static class DespawnPropsPatches
    {
        private static ManualLogSource logger = Logger.CreateLogSource("PizzaTowerEscapeMusic DespawnPropsPatches");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        private static void DespawnPropsAtEndOfRound_Postfix(bool despawnAllItems)
        {
            logger.LogDebug($"DespawnPropsAtEndOfRound called with despawnAllItems = {despawnAllItems}");
            GameEventListener.DespawnPropsCalled = true;
            GameEventListener.LastDespawnAllItems = despawnAllItems;
        }

        internal static void ApplyPatches()
        {
            try
            {
                var harmony = new Harmony("com.pizzatowerescapemusic.despawnprops");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                logger.LogInfo("DespawnProps Harmony patches applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to apply DespawnProps Harmony patches: {ex}");
            }
        }

        internal static void RemovePatches()
        {
            try
            {
                var harmony = new Harmony("com.pizzatowerescapemusic.despawnprops");
                harmony.UnpatchSelf();
                logger.LogInfo("DespawnProps Harmony patches removed");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to remove DespawnProps Harmony patches: {ex}");
            }
        }
    }
}