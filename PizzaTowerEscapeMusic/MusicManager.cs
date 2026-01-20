using BepInEx.Logging;
using PizzaTowerEscapeMusic.Scripting;
using PizzaTowerEscapeMusic.Scripting.ScriptEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PizzaTowerEscapeMusic
{
    public class MusicManager : MonoBehaviour
    {
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
                this.logger.LogDebug(string.Format("GetIsMusicPlaying says there's {0} music instance(s) with the tag \"{1}\"", list.Count, tag));
                return list.Count > 0;
            }
            this.logger.LogDebug("GetIsMusicPlaying says there was no music instance list for tag \"" + tag + "\"");
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
            string text = musicEvent.musicNames[global::UnityEngine.Random.Range(0, musicEvent.musicNames.Length)];
            AudioClip audioClip;
            this.loadedMusic.TryGetValue(text, out audioClip);
            if (audioClip != null)
            {
                new MusicManager.MusicInstance(script, musicEvent, this.GetAudioSource(), audioClip);
                this.logger.LogInfo("Playing music (" + text + ")");
                return;
            }
            this.logger.LogWarning("Music (" + text + ") is null, cannot play. Maybe it wasn't loaded correctly?");
        }

        public void StopMusic(string targetTag = null)
        {
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
            if (MusicManager.audioSourcePool.Count > 0)
            {
                return MusicManager.audioSourcePool.Pop();
            }
            return base.gameObject.AddComponent<AudioSource>();
        }

        public async void LoadNecessaryMusicClips()
        {
            if (PizzaTowerEscapeMusicManager.ScriptManager.loadedScripts.Count == 0)
            {
                this.logger.LogError("No scripts are loaded, cannot load their music!");
            }
            else
            {
                this.UnloadMusicClips();
                foreach (Script script in PizzaTowerEscapeMusicManager.ScriptManager.loadedScripts)
                {
                    ScriptEvent[] array = script.scriptEvents;
                    for (int i = 0; i < array.Length; i++)
                    {
                        ScriptEvent_PlayMusic scriptEvent_PlayMusic = array[i] as ScriptEvent_PlayMusic;
                        if (scriptEvent_PlayMusic != null)
                        {
                            foreach (string musicName in scriptEvent_PlayMusic.musicNames)
                            {
                                if (!this.loadedMusic.ContainsKey(musicName))
                                {
                                    AudioClip audioClip = await this.LoadMusicClip(musicName);
                                    if (!(audioClip == null))
                                    {
                                        this.loadedMusic.Add(musicName, audioClip);
                                    }
                                }
                            }
                            //							string[] array2 = null;
                        }
                    }
                    array = null;
                }
                //				List<Script>.Enumerator enumerator = default(List<Script>.Enumerator);
                this.logger.LogInfo("Music clips done loading");
            }
        }

        public void UnloadMusicClips()
        {
            foreach (AudioClip audioClip in this.loadedMusic.Values)
            {
                audioClip.UnloadAudioData();
            }
            this.loadedMusic.Clear();
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
            string text = musicFileName.Split(new char[] { '.' }, StringSplitOptions.None).Last<string>().ToLower();
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

        private class MusicInstance
        {
            public MusicInstance(Script script, ScriptEvent_PlayMusic musicEvent, AudioSource audioSource, AudioClip musicClip)
            {
                this.script = script;
                this.musicEvent = musicEvent;
                this.audioSource = audioSource;
                audioSource.clip = musicClip;
                audioSource.loop = musicEvent.loop;
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
                float num = (this.isStopping ? 0f : this.volumeGroup.GetVolume(this.script));
                float num2 = (this.isStopping ? this.volumeGroup.stoppingVolumeLerpSpeed : this.volumeGroup.volumeLerpSpeed);
                this.volume = Mathf.Lerp(this.volume, num, num2 * deltaTime);
                this.audioSource.volume = this.volume * PizzaTowerEscapeMusicManager.Configuration.volumeMaster.Value;
                if (!this.audioSource.isPlaying || (this.isStopping && this.audioSource.volume < 0.005f))
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
                this.FadeSpeed = this.volumeGroup.stoppingVolumeLerpSpeed;
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

            private float volume;
        }
    }
}
