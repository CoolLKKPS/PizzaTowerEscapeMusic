using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PizzaTowerEscapeMusic
{
    public class GameEventListener : MonoBehaviour
    {
        private void Awake()
        {
            this.logger = BepInEx.Logging.Logger.CreateLogSource("PizzaTowerEscapeMusic GameEventListener");
            this.OnShipLanded = (Action)Delegate.Combine(this.OnShipLanded, new Action(this.FindDockedApparatus));
        }

        private void FindDockedApparatus()
        {
            this.logger.LogDebug("Checking for docked Apparatus...");
            foreach (LungProp lungProp in global::UnityEngine.Object.FindObjectsOfType<LungProp>())
            {
                if (lungProp.isLungDocked)
                {
                    this.logger.LogDebug("Found docked Apparatus");
                    GameEventListener.dockedApparatus = lungProp;
                    return;
                }
            }
            this.logger.LogDebug("Could not find docked Apparatus");
        }

        public static bool IsApparatusDocked()
        {
            return GameEventListener.dockedApparatus != null;
        }

        private void Update()
        {
            if (SoundManager.Instance != null)
            {
                this.OnFrameUpdate();
            }
            this.CheckSoundManager();
            this.CheckDungeonDoneGenerating();
            this.CheckShipLanded();
            this.CheckShipReturnToOrbit();
            this.CheckShipInOrbit();
            this.CheckShipLeavingAlertCalled();
            this.CheckPlayerDamaged();
            this.CheckPlayerDeath();
            this.CheckPlayerInsideFacility();
            this.CheckPlayerInsideShip();
            this.CheckApparatusTaken();
            this.CheckCurrentMoonChanged();
        }

        private T UpdateCached<T>(string key, T currentValue, T defaultValue)
        {
            object obj;
            if (!this.previousValues.TryGetValue(key, out obj))
            {
                obj = defaultValue;
            }
            this.previousValues[key] = currentValue;
            return (T)((object)obj);
        }

        private void CheckSoundManager()
        {
            bool flag = SoundManager.Instance != null;
            if (this.UpdateCached<bool>("SoundManager", flag, false) == flag)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Sound Manager created");
                this.OnSoundManagerCreated();
                return;
            }
            this.logger.LogDebug("Sound Manager destroyed");
            this.OnSoundManagerDestroyed();
        }

        private void CheckDungeonDoneGenerating()
        {
            bool flag = RoundManager.Instance != null && RoundManager.Instance.dungeonCompletedGenerating;
            bool flag2 = this.UpdateCached<bool>("DungeonDoneGenerating", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Dungeon done generating");
                this.OnDungeonDoneGenerating();
            }
        }

        private void CheckShipLanded()
        {
            bool flag = StartOfRound.Instance != null && StartOfRound.Instance.shipHasLanded;
            bool flag2 = this.UpdateCached<bool>("ShipLanded", flag, true);
            if (flag == flag2)
            {
                return;
            }
            if (StartOfRound.Instance == null)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Ship has landed");
                this.OnShipLanded();
                return;
            }
            this.logger.LogDebug("Ship has taken off");
            this.OnShipTakeOff();
        }

        private void CheckShipReturnToOrbit()
        {
            bool flag = StartOfRound.Instance == null || (!StartOfRound.Instance.shipHasLanded && !StartOfRound.Instance.shipIsLeaving);
            bool flag2 = this.UpdateCached<bool>("ShipReturnToOrbit", flag, true);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Ship returned to orbit");
                this.OnShipReturnToOrbit();
            }
        }

        private void CheckShipInOrbit()
        {
            bool flag = StartOfRound.Instance != null && StartOfRound.Instance.inShipPhase;
            bool flag2 = this.UpdateCached<bool>("ShipInOrbit", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Ship is in orbit");
                this.OnShipInOrbit();
            }
        }

        private void CheckShipLeavingAlertCalled()
        {
            bool flag = TimeOfDay.Instance != null && TimeOfDay.Instance.shipLeavingAlertCalled;
            bool flag2 = this.UpdateCached<bool>("ShipLeavingAlertCalled", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Ship leaving alert called");
                this.OnShipLeavingAlertCalled();
            }
        }

        private void CheckPlayerDamaged()
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }
            int health = GameNetworkManager.Instance.localPlayerController.health;
            int num = this.UpdateCached<int>("PlayerDamaged", health, 100);
            if (health < num)
            {
                this.logger.LogDebug(string.Format("Player took damage (Health: {0})", GameNetworkManager.Instance.localPlayerController.health));
                this.OnPlayerDamaged();
            }
        }

        private void CheckPlayerDeath()
        {
            bool flag = GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null && GameNetworkManager.Instance.localPlayerController.isPlayerDead;
            bool flag2 = this.UpdateCached<bool>("PlayerDeath", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Player has died");
                this.OnPlayerDeath();
            }
        }

        private void CheckPlayerInsideFacility()
        {
            bool flag = GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null && GameNetworkManager.Instance.localPlayerController.isInsideFactory;
            bool flag2 = this.UpdateCached<bool>("PlayerInsideFacility", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Player entered facility");
                this.OnPlayerEnteredFacility();
                return;
            }
            this.logger.LogDebug("Player exited facility");
            this.OnPlayerExitedFacility();
        }

        private void CheckPlayerInsideShip()
        {
            bool flag = GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null && GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom;
            bool flag2 = this.UpdateCached<bool>("PlayerInsideShip", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                this.logger.LogDebug("Player entered ship");
                this.OnPlayerEnteredShip();
                return;
            }
            this.logger.LogDebug("Player exited ship");
            this.OnPlayerExitedShip();
        }

        private void CheckApparatusTaken()
        {
            bool flag = GameEventListener.dockedApparatus != null && !GameEventListener.dockedApparatus.isLungDocked;
            bool flag2 = this.UpdateCached<bool>("ApparatusTaken", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                GameEventListener.dockedApparatus = null;
                this.logger.LogDebug("Apparatus was taken");
                this.OnApparatusTaken();
            }
        }

        private void CheckCurrentMoonChanged()
        {
            TimeOfDay instance = TimeOfDay.Instance;
            SelectableLevel selectableLevel = ((instance != null) ? instance.currentLevel : null);
            SelectableLevel selectableLevel2 = this.UpdateCached<SelectableLevel>("CurrentMoon", selectableLevel, null);
            if (selectableLevel == selectableLevel2)
            {
                return;
            }
            this.logger.LogDebug("Level has changed to " + ((selectableLevel != null) ? selectableLevel.PlanetName : null));
            this.OnCurrentMoonChanged(selectableLevel);
        }

        private ManualLogSource logger;

        public Action OnFrameUpdate = delegate
        {
        };

        public Action OnSoundManagerCreated = delegate
        {
        };

        public Action OnSoundManagerDestroyed = delegate
        {
        };

        public Action OnDungeonDoneGenerating = delegate
        {
        };

        public Action OnShipLanded = delegate
        {
        };

        public Action OnShipTakeOff = delegate
        {
        };

        public Action OnShipReturnToOrbit = delegate
        {
        };

        public Action OnShipInOrbit = delegate
        {
        };

        public Action OnShipLeavingAlertCalled = delegate
        {
        };

        public Action OnPlayerDamaged = delegate
        {
        };

        public Action OnPlayerDeath = delegate
        {
        };

        public Action OnPlayerEnteredFacility = delegate
        {
        };

        public Action OnPlayerExitedFacility = delegate
        {
        };

        public Action OnPlayerEnteredShip = delegate
        {
        };

        public Action OnPlayerExitedShip = delegate
        {
        };

        public Action OnApparatusTaken = delegate
        {
        };

        public Action<SelectableLevel> OnCurrentMoonChanged = delegate (SelectableLevel l)
        {
        };

        private readonly Dictionary<string, object> previousValues = new Dictionary<string, object>();

        private static LungProp dockedApparatus;

        public Action OnMeltdownStarted = delegate
        {
        };
    }
}
