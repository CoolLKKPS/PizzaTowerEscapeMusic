using BepInEx.Logging;
using System;

namespace PizzaTowerEscapeMusic.Networking
{
    public static class SeedSyncService
    {
        private static ManualLogSource logger = Logger.CreateLogSource("PizzaTowerEscapeMusic SeedSyncService");

        private static bool seedReceived = false;
        public static bool SeedReceived => seedReceived;
        public static event Action OnSeedReceived;

        public static void SetSeedReceived()
        {
            if (!seedReceived)
            {
                seedReceived = true;
                logger.LogDebug("SeedReceived flag set.");
                OnSeedReceived?.Invoke();
            }
        }

        public static void Reset()
        {
            if (seedReceived)
            {
                seedReceived = false;
                logger.LogDebug("SeedReceived flag reset.");
            }
        }

        public static void LogStatus()
        {
            logger.LogDebug($"SeedReceived = {SeedReceived}");
        }
    }
}