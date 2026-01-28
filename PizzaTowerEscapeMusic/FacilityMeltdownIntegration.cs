using BepInEx.Logging;
using System;
using System.Reflection;
using UnityEngine;

namespace PizzaTowerEscapeMusic
{
    public class FacilityMeltdownIntegration : MonoBehaviour
    {
        public void Initialize(ManualLogSource logger, GameEventListener gameEventListener)
        {
            this.logger = logger;
            this.gameEventListener = gameEventListener;
            this.TryHookIntoFacilityMeltdown();
        }

        private void TryHookIntoFacilityMeltdown()
        {
            try
            {
                if (Type.GetType("FacilityMeltdown.MeltdownPlugin, FacilityMeltdown") == null)
                {
                    this.logger.LogInfo("Could not find FacilityMeltdown mod, skipping");
                }
                else
                {
                    this.logger.LogInfo("FacilityMeltdown detected, hooking into meltdown events...");
                    Type type = Type.GetType("FacilityMeltdown.API.MeltdownAPI, FacilityMeltdown");
                    if (type == null)
                    {
                        this.logger.LogWarning("Could not find MeltdownAPI type");
                    }
                    else
                    {
                        MethodInfo method = type.GetMethod("RegisterMeltdownListener", BindingFlags.Static | BindingFlags.Public);
                        if (method == null)
                        {
                            this.logger.LogWarning("Could not find RegisterMeltdownListener method in MeltdownAPI");
                        }
                        else
                        {
                            this.logger.LogInfo("Found RegisterMeltdownListener method in MeltdownAPI");
                            Action action = new Action(this.OnFacilityMeltdownStarted);
                            method.Invoke(null, new object[] { action });
                            this.registeredCallback = action;
                            this.logger.LogInfo("Successfully registered as a meltdown listener!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to hook into FacilityMeltdown: " + ex.Message);
                this.logger.LogDebug("Exception details: " + ex.ToString());
            }
        }

        private void OnFacilityMeltdownStarted()
        {
            this.logger.LogInfo("FacilityMeltdown meltdown event detected! Triggering music...");
            if (this.gameEventListener != null)
            {
                Action onMeltdownStarted = this.gameEventListener.OnMeltdownStarted;
                if (onMeltdownStarted == null)
                {
                    return;
                }
                onMeltdownStarted();
            }
        }

        private void OnDestroy()
        {
            this.logger.LogDebug("FacilityMeltdown integration being destroyed");
        }

        private ManualLogSource logger;

        private GameEventListener gameEventListener;

        private Action registeredCallback;
    }
}
