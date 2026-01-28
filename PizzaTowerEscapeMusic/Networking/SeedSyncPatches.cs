using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;

namespace PizzaTowerEscapeMusic.Networking
{
    [HarmonyPatch]
    internal static class SeedSyncPatches
    {
        private static ManualLogSource logger = Logger.CreateLogSource("PizzaTowerEscapeMusic SeedSyncPatches");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "OnPlayerConnectedClientRpc", new Type[] {
            typeof(ulong), typeof(int), typeof(ulong[]), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool)
        })]
        private static void OnPlayerConnectedClientRpc_Postfix(ulong clientId, int connectedPlayers, ulong[] connectedPlayerIdsOrdered, int assignedPlayerObjectId, int serverMoneyAmount, int levelID, int profitQuota, int timeUntilDeadline, int quotaFulfilled, int randomSeed, bool isChallenge)
        {
            logger.LogDebug($"OnPlayerConnectedClientRpc captured randomSeed: {randomSeed}");
            SeedSyncService.SetSeedReceived(randomSeed);
        }
        internal static void ApplyPatches()
        {
            try
            {
                var harmony = new Harmony("com.pizzatowerescapemusic.seedsync");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                logger.LogInfo("SeedSync Harmony patches applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to apply SeedSync Harmony patches: {ex}");
            }
        }

        internal static void RemovePatches()
        {
            try
            {
                var harmony = new Harmony("com.pizzatowerescapemusic.seedsync");
                harmony.UnpatchSelf();
                logger.LogInfo("SeedSync Harmony patches removed");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to remove SeedSync Harmony patches: {ex}");
            }
        }
    }
}