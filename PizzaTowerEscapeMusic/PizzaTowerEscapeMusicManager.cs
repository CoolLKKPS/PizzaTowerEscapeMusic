using BepInEx.Configuration;
using BepInEx.Logging;
using PizzaTowerEscapeMusic.Scripting;
using System;
using UnityEngine;

namespace PizzaTowerEscapeMusic
{
    public class PizzaTowerEscapeMusicManager : MonoBehaviour
    {
        public static Configuration Configuration { get; private set; }

        public static ScriptManager ScriptManager { get; private set; }

        public static MusicManager MusicManager { get; private set; }

        public void Initialise(ManualLogSource logger, ConfigFile config)
        {
            this.logger = logger;
            PizzaTowerEscapeMusicManager.Configuration = new Configuration(config);
            PizzaTowerEscapeMusicManager.MusicManager = base.gameObject.AddComponent<MusicManager>();
            this.gameEventListener = base.gameObject.AddComponent<GameEventListener>();
            GameEventListener gameEventListener = this.gameEventListener;
            gameEventListener.OnSoundManagerCreated = (Action)Delegate.Combine(gameEventListener.OnSoundManagerCreated, new Action(PizzaTowerEscapeMusicManager.MusicManager.LoadNecessaryMusicClips));
            GameEventListener gameEventListener2 = this.gameEventListener;
            gameEventListener2.OnSoundManagerDestroyed = (Action)Delegate.Combine(gameEventListener2.OnSoundManagerDestroyed, new Action(delegate
            {
                PizzaTowerEscapeMusicManager.MusicManager.StopMusic(null);
            }));
            GameEventListener gameEventListener3 = this.gameEventListener;
            gameEventListener3.OnSoundManagerDestroyed = (Action)Delegate.Combine(gameEventListener3.OnSoundManagerDestroyed, new Action(PizzaTowerEscapeMusicManager.MusicManager.UnloadMusicClips));
            PizzaTowerEscapeMusicManager.ScriptManager = new ScriptManager(PizzaTowerEscapeMusicManager.Configuration.scriptingScripts.Value.Split(new char[] { ',' }, StringSplitOptions.None), this.gameEventListener);
            GameEventListener gameEventListener4 = this.gameEventListener;
            gameEventListener4.OnSoundManagerDestroyed = (Action)Delegate.Combine(gameEventListener4.OnSoundManagerDestroyed, new Action(PizzaTowerEscapeMusicManager.ScriptManager.ClearAllScriptTimers));
            try
            {
                base.gameObject.AddComponent<FacilityMeltdownIntegration>().Initialize(logger, this.gameEventListener);
                logger.LogInfo("FacilityMeltdown integration initialized.");
            }
            catch (Exception ex)
            {
                logger.LogWarning("Failed to initialize FacilityMeltdown integration (this is normal if FacilityMeltdown is not installed): " + ex.Message);
            }
            base.gameObject.hideFlags = HideFlags.HideAndDontSave;
            logger.LogInfo("Plugin bgn.pizzatowerescapemusic is loaded!");
        }

        private void Update()
        {
            PizzaTowerEscapeMusicManager.ScriptManager.UpdateAllScriptTimers(Time.deltaTime);
        }

        private GameEventListener gameEventListener;

        private ManualLogSource logger;
    }
}
