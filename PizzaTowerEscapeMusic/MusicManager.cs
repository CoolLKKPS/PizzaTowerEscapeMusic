using BepInEx.Logging;
using PizzaTowerEscapeMusic.Scripting;
using PizzaTowerEscapeMusic.Scripting.ScriptEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PizzaTowerEscapeMusic
{
    public class MusicManager : MonoBehaviour
    {
        private Dictionary<string, float> lastGetIsMusicPlayingLogTimeByTag = new Dictionary<string, float>();

        private bool enablelogCooldown = true;

        private void Awake()
        {
            this.logger = BepInEx.Logging.Logger.CreateLogSource("PizzaTowerEscapeMusic MusicManager");
        }

        private void Update()
        {
            if (StartOfRound.Instance == null)
            {
                return;
            }
            for (int i = MusicManager.musicInstances.Count - 1; i >= 0; i--)
            {
                MusicManager.musicInstances[i].Update(Time.deltaTime);
            }
            bool flag = false;
            using (List<MusicManager.MusicInstance>.Enumerator enumerator = MusicManager.musicInstances.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.musicEvent.silenceGameMusic)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                if (SoundManager.Instance.playingOutsideMusic && this.GetIsMusicPlaying(null))
                {
                    SoundManager.Instance.playingOutsideMusic = false;
                    this.logger.LogInfo("Silenced the outside music because alternate music is playing");
                }
                if (TimeOfDay.Instance.TimeOfDayMusic.isPlaying && this.GetIsMusicPlaying(null))
                {
                    TimeOfDay.Instance.TimeOfDayMusic.Stop();
                    this.logger.LogInfo("Silenced the time of day music because alternate music is playing");
                }
            }
        }

        public bool GetIsMusicPlaying(string tag = null)
        {
            if (tag == null)
            {
                return MusicManager.musicInstances.Count > 0;
            }
            List<MusicManager.MusicInstance> list;
            if (MusicManager.musicInstancesByTag.TryGetValue(tag, out list))
            {
                bool shouldLog = true;
                if (this.enablelogCooldown)
                {
                    float lastLogTime;
                    if (!this.lastGetIsMusicPlayingLogTimeByTag.TryGetValue(tag, out lastLogTime))
                    {
                        lastLogTime = -30f;
                    }
                    if (UnityEngine.Time.time - lastLogTime >= 30f)
                    {
                        this.lastGetIsMusicPlayingLogTimeByTag[tag] = UnityEngine.Time.time;
                    }
                    else
                    {
                        shouldLog = false;
                    }
                }
                if (shouldLog)
                {
                    this.logger.LogDebug(string.Format("GetIsMusicPlaying says there's {0} music instance(s) with the tag \"{1}\"", list.Count, tag));
                }
                return list.Count > 0;
            }
            bool shouldLog2 = true;
            if (this.enablelogCooldown)
            {
                float lastLogTime2;
                if (!this.lastGetIsMusicPlayingLogTimeByTag.TryGetValue(tag, out lastLogTime2))
                {
                    lastLogTime2 = -30f;
                }
                if (UnityEngine.Time.time - lastLogTime2 >= 30f)
                {
                    this.lastGetIsMusicPlayingLogTimeByTag[tag] = UnityEngine.Time.time;
                }
                else
                {
                    shouldLog2 = false;
                }
            }
            if (shouldLog2)
            {
                this.logger.LogDebug("GetIsMusicPlaying says there was no music instance list for tag \"" + tag + "\"");
            }
            return false;
        }

        public void PlayMusic(Script script, ScriptEvent_PlayMusic musicEvent)
        {
            this.logger.LogDebug(string.Concat(new string[]
            {
                "PlayMusic called\nTag:                         ",
                musicEvent.tag,
                string.Format("\nOverlap handling:            {0}", musicEvent.overlapHandling),
                string.Format("\nAny music playing?:          {0}", this.GetIsMusicPlaying(null)),
                string.Format("\nAny music playing with tag?: {0}", this.GetIsMusicPlaying(musicEvent.tag))
            }));
            string mainMusicName = musicEvent.musicNames[global::UnityEngine.Random.Range(0, musicEvent.musicNames.Length)];
            string introMusicName = null;
            if (musicEvent.introMusicNames != null && musicEvent.introMusicNames.Length > 0)
            {
                introMusicName = musicEvent.introMusicNames[global::UnityEngine.Random.Range(0, musicEvent.introMusicNames.Length)];
            }
            if (!this.musicLoaded)
            {
                this.pendingMusicPlays.Enqueue(new PendingMusicPlay
                {
                    script = script,
                    musicEvent = musicEvent,
                    musicName = mainMusicName
                });
                this.logger.LogDebug($"Music not loaded yet, queued play request for '{mainMusicName}' (queue size: {this.pendingMusicPlays.Count})");
                return;
            }
            if (musicEvent.overlapHandling == ScriptEvent_PlayMusic.OverlapHandling.IgnoreAll && this.GetIsMusicPlaying(null))
            {
                this.logger.LogDebug("PlayMusic canceled because other music was playing");
                return;
            }
            if (musicEvent.overlapHandling == ScriptEvent_PlayMusic.OverlapHandling.IgnoreTag && this.GetIsMusicPlaying(musicEvent.tag))
            {
                this.logger.LogDebug("PlayMusic canceled because other music with the tag \"" + musicEvent.tag + "\" was playing");
                return;
            }
            switch (musicEvent.overlapHandling)
            {
                case ScriptEvent_PlayMusic.OverlapHandling.OverrideAll:
                    this.StopMusic(null);
                    break;
                case ScriptEvent_PlayMusic.OverlapHandling.OverrideTag:
                    this.StopMusic(musicEvent.tag);
                    break;
                case ScriptEvent_PlayMusic.OverlapHandling.OverrideFadeAll:
                    this.FadeStopMusic(null);
                    break;
                case ScriptEvent_PlayMusic.OverlapHandling.OverrideFadeTag:
                    this.FadeStopMusic(musicEvent.tag);
                    break;
            }
            AudioClip mainClip;
            this.loadedMusic.TryGetValue(mainMusicName, out mainClip);
            AudioClip introClip = null;
            if (introMusicName != null)
            {
                this.loadedMusic.TryGetValue(introMusicName, out introClip);
            }
            if (mainClip != null)
            {
                new MusicManager.MusicInstance(script, musicEvent, this.GetAudioSource(), mainClip, introClip);
                this.logger.LogInfo("Playing music (" + mainMusicName + ")" + (introMusicName != null ? " with intro (" + introMusicName + ")" : ""));
                return;
            }
            this.logger.LogWarning("Music (" + mainMusicName + ") is null, cannot play. Maybe it wasn't loaded correctly?");
        }

        private void ProcessPendingMusicPlays()
        {
            if (!this.musicLoaded) return;
            while (this.pendingMusicPlays.Count > 0)
            {
                var pending = this.pendingMusicPlays.Dequeue();
                AudioClip mainClip;
                this.loadedMusic.TryGetValue(pending.musicName, out mainClip);
                AudioClip introClip = null;
                if (pending.musicEvent.introMusicNames != null && pending.musicEvent.introMusicNames.Length > 0)
                {
                    string introMusicName = pending.musicEvent.introMusicNames[global::UnityEngine.Random.Range(0, pending.musicEvent.introMusicNames.Length)];
                    this.loadedMusic.TryGetValue(introMusicName, out introClip);
                }
                if (mainClip != null)
                {
                    new MusicManager.MusicInstance(pending.script, pending.musicEvent, this.GetAudioSource(), mainClip, introClip);
                    this.logger.LogInfo("Playing queued music (" + pending.musicName + ")" + (introClip != null ? " with intro" : ""));
                }
                else
                {
                    this.logger.LogWarning("Queued music (" + pending.musicName + ") is null, cannot play. Maybe it wasn't loaded correctly?");
                }
            }
        }

        private void RemovePendingMusicPlays(string targetTag = null)
        {
            if (this.pendingMusicPlays.Count == 0) return;
            var remaining = new Queue<PendingMusicPlay>();
            while (this.pendingMusicPlays.Count > 0)
            {
                var pending = this.pendingMusicPlays.Dequeue();
                bool shouldRemove = false;
                if (targetTag == null)
                {
                    shouldRemove = true;
                }
                else if (pending.musicEvent.tag == targetTag)
                {
                    shouldRemove = true;
                }
                if (!shouldRemove)
                {
                    remaining.Enqueue(pending);
                }
                else
                {
                    this.logger.LogDebug($"Removed queued music play for '{pending.musicName}' (tag: {pending.musicEvent.tag ?? "null"})");
                }
            }
            this.pendingMusicPlays = remaining;
        }

        public void StopMusic(string targetTag = null)
        {
            this.RemovePendingMusicPlays(targetTag);
            foreach (MusicManager.MusicInstance musicInstance in new List<MusicManager.MusicInstance>(MusicManager.musicInstances))
            {
                if (targetTag == null || !(musicInstance.musicEvent.tag != targetTag))
                {
                    musicInstance.StopCompletely();
                }
            }
        }

        public void FadeStopMusic(string targetTag = null)
        {
            this.RemovePendingMusicPlays(targetTag);
            foreach (MusicManager.MusicInstance musicInstance in new List<MusicManager.MusicInstance>(MusicManager.musicInstances))
            {
                if (targetTag == null || !(musicInstance.musicEvent.tag != targetTag))
                {
                    musicInstance.FadeStop();
                }
            }
        }

        private AudioSource GetAudioSource()
        {
            AudioSource audioSource;
            if (MusicManager.audioSourcePool.Count > 0)
            {
                audioSource = MusicManager.audioSourcePool.Pop();
            }
            else
            {
                audioSource = base.gameObject.AddComponent<AudioSource>();
            }
            return audioSource;
        }

        public async void LoadNecessaryMusicClips()
        {
            if (PizzaTowerEscapeMusicManager.ScriptManager.loadedScripts.Count == 0)
            {
                logger.LogError("No scripts are loaded, cannot load their music!");
                return;
            }
            UnloadMusicClips();
            foreach (Script loadedScript in PizzaTowerEscapeMusicManager.ScriptManager.loadedScripts)
            {
                ScriptEvent[] scriptEvents = loadedScript.scriptEvents;
                for (int i = 0; i < scriptEvents.Length; i++)
                {
                    if (!(scriptEvents[i] is ScriptEvent_PlayMusic scriptEvent_PlayMusic))
                    {
                        continue;
                    }
                    string[] musicNames = scriptEvent_PlayMusic.musicNames;
                    foreach (string musicName in musicNames)
                    {
                        if (!loadedMusic.ContainsKey(musicName))
                        {
                            AudioClip audioClip = await LoadMusicClip(musicName);
                            if (!(audioClip == null))
                            {
                                loadedMusic.Add(musicName, audioClip);
                            }
                        }
                    }
                    string[] introMusicNames = scriptEvent_PlayMusic.introMusicNames;
                    foreach (string introMusicName in introMusicNames)
                    {
                        if (!loadedMusic.ContainsKey(introMusicName))
                        {
                            AudioClip audioClip = await LoadMusicClip(introMusicName);
                            if (!(audioClip == null))
                            {
                                loadedMusic.Add(introMusicName, audioClip);
                            }
                        }
                    }
                }
            }
            logger.LogInfo("Music clips done loading");
            this.musicLoaded = true;
            this.ProcessPendingMusicPlays();
        }

        public void UnloadMusicClips()
        {
            foreach (AudioClip audioClip in this.loadedMusic.Values)
            {
                audioClip.UnloadAudioData();
            }
            this.loadedMusic.Clear();
            this.musicLoaded = false;
            this.pendingMusicPlays.Clear();
            this.logger.LogInfo("All music clips unloaded");
        }

        private async Task<AudioClip> LoadMusicClip(string musicFileName)
        {
            AudioType audioType;
            string text;
            this.InterpretMusicFileName(musicFileName, out audioType, out text);
            string path = "file:///" + CustomManager.GetFilePath("Music/" + text, "DefaultMusic/" + text);
            AudioClip audioClip;
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
            {
                request.SendWebRequest();
                while (!request.isDone)
                {
                    await Task.Delay(50);
                }
                if (request.result == UnityWebRequest.Result.Success)
                {
                    this.logger.LogInfo("Loaded music (" + musicFileName + ") from file");
                    AudioClip content = DownloadHandlerAudioClip.GetContent(request);
                    content.name = musicFileName;
                    audioClip = content;
                }
                else
                {
                    this.logger.LogError(string.Concat(new string[]
                    {
                        string.Format("Failed to load music ({0}) from file as audio type {1}, if the file extension and the audio type do not match the file extension may not be supported.", musicFileName, audioType),
                        "\n- Path: ",
                        path,
                        "\n- Error: ",
                        request.error
                    }));
                    audioClip = null;
                }
            }
            return audioClip;
        }

        private void InterpretMusicFileName(string musicFileName, out AudioType audioType, out string finalFileName)
        {
            if (!musicFileName.Contains('.'))
            {
                audioType = AudioType.WAV;
                finalFileName = musicFileName + ".wav";
                return;
            }
            string text = musicFileName.Split('.').Last<string>().ToLower();
            AudioType audioType2;
            if (!(text == "ogg"))
            {
                if (!(text == "mp3"))
                {
                    audioType2 = AudioType.WAV;
                }
                else
                {
                    audioType2 = AudioType.MPEG;
                }
            }
            else
            {
                audioType2 = AudioType.OGGVORBIS;
            }
            audioType = audioType2;
            finalFileName = musicFileName;
        }

        private ManualLogSource logger;

        private static readonly List<MusicManager.MusicInstance> musicInstances = new List<MusicManager.MusicInstance>();

        private static readonly Dictionary<string, List<MusicManager.MusicInstance>> musicInstancesByTag = new Dictionary<string, List<MusicManager.MusicInstance>>();

        private static readonly Stack<AudioSource> audioSourcePool = new Stack<AudioSource>();

        private readonly Dictionary<string, AudioClip> loadedMusic = new Dictionary<string, AudioClip>();

        private bool musicLoaded = false;

        private Queue<PendingMusicPlay> pendingMusicPlays = new Queue<PendingMusicPlay>();

        private struct PendingMusicPlay
        {
            public Script script;
            public ScriptEvent_PlayMusic musicEvent;
            public string musicName;
        }

        private class MusicInstance
        {
            public MusicInstance(Script script, ScriptEvent_PlayMusic musicEvent, AudioSource audioSource, AudioClip musicClip, AudioClip introClip = null)
            {
                this.script = script;
                this.musicEvent = musicEvent;
                this.audioSource = audioSource;
                this.introClip = introClip;
                this.mainClip = musicClip;
                this.isIntroPlaying = introClip != null;
                if (this.isIntroPlaying)
                {
                    audioSource.clip = introClip;
                    audioSource.loop = false;
                }
                else
                {
                    audioSource.clip = mainClip;
                    audioSource.loop = musicEvent.loop;
                }
                audioSource.Play();
                MusicManager.musicInstances.Add(this);
                if (musicEvent.tag != null)
                {
                    List<MusicManager.MusicInstance> list;
                    if (!MusicManager.musicInstancesByTag.TryGetValue(musicEvent.tag, out list))
                    {
                        list = new List<MusicManager.MusicInstance>(1);
                        MusicManager.musicInstancesByTag.Add(musicEvent.tag, list);
                    }
                    list.Add(this);
                }
                this.volumeGroup = script.TryGetVolumeGroupOrDefault(musicEvent.tag);
                this.volume = this.volumeGroup.GetVolume(script);
            }

            public float FadeSpeed { get; private set; }

            public void Update(float deltaTime)
            {
                bool wasJustSwitched = this.justSwitched;
                this.justSwitched = false;
                if (this.isIntroPlaying && !this.audioSource.isPlaying)
                {
                    this.isIntroPlaying = false;
                    this.audioSource.clip = this.mainClip;
                    this.audioSource.loop = this.musicEvent.loop;
                    this.audioSource.Play();
                    this.justSwitched = true;
                }
                float num = (this.isStopping ? 0f : this.volumeGroup.GetVolume(this.script));
                float baseSpeed = (this.isStopping ? this.volumeGroup.stoppingVolumeLerpSpeed : this.volumeGroup.volumeLerpSpeed);
                float speedScale = (this.isStopping ? this.volumeGroup.GetStoppingVolumeLerpSpeedScale(this.script) : this.volumeGroup.GetVolumeLerpSpeedScale(this.script));
                float effectiveSpeed = baseSpeed * speedScale;
                this.volume = Mathf.Lerp(this.volume, num, effectiveSpeed * deltaTime);
                this.audioSource.volume = this.volume * PizzaTowerEscapeMusicManager.Configuration.volumeMaster.Value;
                if ((!this.audioSource.isPlaying && !wasJustSwitched) || (this.isStopping && this.audioSource.volume < 0.005f))
                {
                    this.StopCompletely();
                }
            }

            public void FadeStop()
            {
                if (this.isStopping)
                {
                    return;
                }
                this.isStopping = true;
                // this.FadeSpeed = this.volumeGroup.stoppingVolumeLerpSpeed;
                float speedScale = this.volumeGroup.GetStoppingVolumeLerpSpeedScale(this.script);
                this.FadeSpeed = this.volumeGroup.stoppingVolumeLerpSpeed * speedScale;
            }

            public void StopCompletely()
            {
                this.audioSource.Stop();
                MusicManager.audioSourcePool.Push(this.audioSource);
                MusicManager.musicInstances.Remove(this);
                List<MusicManager.MusicInstance> list;
                if (this.musicEvent.tag != null && MusicManager.musicInstancesByTag.TryGetValue(this.musicEvent.tag, out list))
                {
                    list.Remove(this);
                }
            }

            public Script script;

            public ScriptEvent_PlayMusic musicEvent;

            public AudioSource audioSource;

            public Script.VolumeGroup volumeGroup;

            private bool isStopping;

            private AudioClip introClip;

            private AudioClip mainClip;
            
            private bool isIntroPlaying;
            
            private bool justSwitched;

            private float volume;
        }
    }
}
