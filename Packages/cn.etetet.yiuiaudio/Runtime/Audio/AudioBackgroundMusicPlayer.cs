using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 背景音乐播放器
    /// </summary>
    public class AudioBackgroundMusicPlayer : IDisposable
    {
        private AudioEmitterController m_OwnerController;

        /// <summary>
        /// 背景音乐数据
        /// </summary>
        public class BGMData : IPriorityData<string>
        {
            public int Priority { get; set; }

            public string DataKey { get; set; }

            /// <summary>
            /// 音量大小
            /// </summary>
            public float SoundVolume { get; set; } = 1f;

            /// <summary>
            /// 音效片段名称
            /// </summary>
            public readonly List<string> AudioClipNames = new List<string>();
        }

        private readonly PriorityData<string, BGMData> m_BGMMusicData = new PriorityData<string, BGMData>();

        /// <summary>
        /// 当前音效Id
        /// </summary>
        private int m_CurrentMusicId;

        private float m_FadeInTime  = 0.5f;
        private float m_FadeOutTime = 0.5f;

        class FadeMusicData
        {
            public bool  IsFadeOut     = true;
            public float StartTime     = 0;
            public float EndTime       = 0;
            public float VolumeFrom    = 0;
            public float VolumeTo      = 0;
            public float CurrentVolume = 0;
            public int   MusicId       = 0;
            public float Duration      = 0;
        }

        readonly List<FadeMusicData> m_FadeMusicDatas = new();

        public AudioBackgroundMusicPlayer(AudioEmitterController controller)
        {
            m_OwnerController = controller;
            m_BGMMusicData.SetDataChangedHandler(OnHandleActiveDataChanged);
        }

        private void OnHandleActiveDataChanged(BGMData data)
        {
            PlayMusic(data);
        }

        void PlayMusic(BGMData data)
        {
            float  delayTime        = 0;
            string currentMusicName = string.Empty;
            string nextMusicName    = string.Empty;
            if (data != null)
            {
                nextMusicName = GetPlayingMusicName(data);
            }

            if (m_CurrentMusicId != 0)
            {
                var findItem = m_FadeMusicDatas.Find((t) => t.MusicId == m_CurrentMusicId);
                if (findItem == null)
                {
                    findItem = new FadeMusicData() { MusicId = m_CurrentMusicId, VolumeTo = 0 };
                    m_FadeMusicDatas.Add(findItem);
                    m_OwnerController.TryGetAudioVolume(m_CurrentMusicId, out float currVolume);
                    findItem.VolumeFrom = currVolume;
                }
                else
                {
                    findItem.VolumeFrom = findItem.CurrentVolume;
                }

                findItem.IsFadeOut = true;
                findItem.StartTime = Time.realtimeSinceStartup;
                findItem.Duration  = m_FadeOutTime;
                findItem.EndTime   = findItem.StartTime + m_FadeOutTime;
                delayTime          = m_FadeOutTime;
                m_OwnerController.TryGetAudioClipName(m_CurrentMusicId, out currentMusicName);

                // 如果即将播放的音乐是原来的音乐，则不需要处理音乐渐变
                if (currentMusicName == nextMusicName)
                {
                    findItem.VolumeTo = findItem.VolumeFrom;
                }

                m_CurrentMusicId   = 0;
                m_CanPlayNextMusic = false;
            }

            if (!string.IsNullOrEmpty(nextMusicName))
            {
                m_CurrentMusicId = InternalPlayBGM(nextMusicName, delayTime, 0);

                if (m_CurrentMusicId != 0) // 播放成功
                {
                    var fadeIn = new FadeMusicData() { MusicId = m_CurrentMusicId, VolumeFrom = nextMusicName == currentMusicName ? data.SoundVolume : 0, VolumeTo = data.SoundVolume, CurrentVolume = nextMusicName == currentMusicName ? data.SoundVolume : 0, IsFadeOut = false };
                    fadeIn.StartTime = Time.realtimeSinceStartup + delayTime;
                    fadeIn.Duration  = m_FadeInTime;
                    fadeIn.EndTime   = fadeIn.StartTime + m_FadeInTime;
                    if (m_OwnerController.TryGetAudioClipLength(m_CurrentMusicId, out float audioLength))
                    {
                        NextSwithTime      = Time.realtimeSinceStartup + audioLength - m_FadeOutTime;
                        m_CanPlayNextMusic = true;
                    }

                    m_FadeMusicDatas.Add(fadeIn);
                }
                else // 播放失败，间隔1s后再尝试
                {
                    NextSwithTime      = Time.realtimeSinceStartup + 1f;
                    m_CanPlayNextMusic = true;
                }
            }
        }

        private int InternalPlayBGM(string musicName, float delayTime, float volume)
        {
            if (m_OwnerController != null)
            {
                using var soundDataParams = AudioDataParams.Fetch();

                soundDataParams.SoundType = (int)EAudioType.BGM;
                soundDataParams.SoundKey  = musicName;
                soundDataParams.AudioClipNames.Add(musicName);
                soundDataParams.DelayTimeMin = delayTime;
                soundDataParams.DelayTimeMax = delayTime;
                soundDataParams.SoundVolume  = volume;
                soundDataParams.OnStarted    = OnHandleBGMPlayStart;

                return m_OwnerController.PlaySound(soundDataParams);
            }

            return 0;
        }

        public string GetPlayingMusicName(BGMData data)
        {
            if (data.AudioClipNames.Count == 0)
            {
                return string.Empty;
            }
            else if (data.AudioClipNames.Count == 1)
            {
                return data.AudioClipNames[0];
            }
            else
            {
                System.Random random = new System.Random();
                var           index  = random.Next(0, data.AudioClipNames.Count);
                return data.AudioClipNames[index];
            }
        }

        private bool  m_CanPlayNextMusic = false;
        private float NextSwithTime      = 0;

        private void OnHandleBGMPlayStart(int audioId)
        {
            if (m_CurrentMusicId == audioId)
            {
                if (m_OwnerController.TryGetAudioClipLength(audioId, out float audioLength))
                {
                    NextSwithTime      = Time.realtimeSinceStartup + audioLength - m_FadeOutTime;
                    m_CanPlayNextMusic = true;
                }
            }
        }

        public void UpdateHandle()
        {
            float currTime = Time.realtimeSinceStartup;

            if (m_FadeMusicDatas.Count > 0)
            {
                for (int i = m_FadeMusicDatas.Count - 1; i >= 0; i--)
                {
                    var fadeData = m_FadeMusicDatas[i];
                    if (currTime < fadeData.EndTime)
                    {
                        fadeData.CurrentVolume = Mathf.Lerp(fadeData.VolumeFrom, fadeData.VolumeTo, Mathf.Clamp01((currTime - fadeData.StartTime) / fadeData.Duration));
                        m_OwnerController?.UpdateSoundVolume(fadeData.MusicId, fadeData.CurrentVolume);
                    }
                    else
                    {
                        fadeData.CurrentVolume = fadeData.VolumeTo;
                        m_OwnerController?.UpdateSoundVolume(fadeData.MusicId, fadeData.CurrentVolume);
                        if (fadeData.IsFadeOut)
                        {
                            m_OwnerController?.StopAudio(fadeData.MusicId);
                        }

                        m_FadeMusicDatas.RemoveAt(i);
                    }
                }
            }

            if (m_CanPlayNextMusic && currTime >= NextSwithTime)
            {
                PlayMusic(m_BGMMusicData.CurrentActiveData);
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="musicName"></param>
        /// <param name="priority"></param>
        public void PlayMusic(string musicName, List<string> audioClipNames, int priority, float soundVolume = 1f)
        {
            var bgmData = new BGMData();

            bgmData.DataKey = musicName;
            bgmData.AudioClipNames.AddRange(audioClipNames);
            bgmData.Priority    = priority;
            bgmData.SoundVolume = soundVolume;

            m_BGMMusicData.TryAddData(bgmData);
        }

        public void StopMusic(string musicName, int priority)
        {
            m_BGMMusicData.TryRemoveData(musicName, priority);
        }

        public void Dispose()
        {
            m_OwnerController = null;
        }
    }
}