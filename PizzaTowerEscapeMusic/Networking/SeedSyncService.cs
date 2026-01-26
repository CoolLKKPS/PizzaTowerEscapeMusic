using BepInEx.Logging;
using System;

namespace PizzaTowerEscapeMusic.Networking
{
    public static class SeedSyncService
    {
        private static ManualLogSource logger = Logger.CreateLogSource("PizzaTowerEscapeMusic SeedSyncService");

        private static bool seedReceived = false;

        public static bool SeedReceived => seedReceived;

        private static int capturedSeed = -1;

        public static int CapturedSeed => capturedSeed;

        public static event Action OnSeedReceived;

        public static void SetSeedReceived(int seed = -1)
        {
            if (!seedReceived)
            {
                seedReceived = true;
                capturedSeed = seed;
                logger.LogDebug($"SeedReceived flag set with seed: {seed}");
                OnSeedReceived?.Invoke();
            }
        }

        public static void Reset()
        {
            if (seedReceived)
            {
                capturedSeed = -1;
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