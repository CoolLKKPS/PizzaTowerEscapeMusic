using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;

namespace PizzaTowerEscapeMusic
{
    [HarmonyPatch]
    internal static class EndOfGamePatches
    {
        private static ManualLogSource logger = Logger.CreateLogSource("PizzaTowerEscapeMusic EndOfGamePatches");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        private static void EndOfGame_Postfix(int bodiesInsured, int connectedPlayersOnServer, int scrapCollected)
        {
            logger.LogDebug($"EndOfGame called with bodiesInsured={bodiesInsured}, connectedPlayersOnServer={connectedPlayersOnServer}, scrapCollected={scrapCollected}");
            GameEventListener.EndOfGameCalled = true;
        }

        internal static void ApplyPatches()
        {
            try
            {
                var harmony = new Harmony("com.pizzatowerescapemusic.endofgame");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                logger.LogInfo("EndOfGame Harmony patches applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to apply EndOfGame Harmony patches: {ex}");
            }
        }

        internal static void RemovePatches()
        {
            try
            {
                var harmony = new Harmony("com.pizzatowerescapemusic.endofgame");
                harmony.UnpatchSelf();
                logger.LogInfo("EndOfGame Harmony patches removed");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to remove EndOfGame Harmony patches: {ex}");
            }
        }
    }
}