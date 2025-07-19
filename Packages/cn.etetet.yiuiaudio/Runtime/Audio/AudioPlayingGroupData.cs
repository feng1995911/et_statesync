using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 音效播放组数据
    /// </summary>
    class AudioPlayingGroupData
    {
        /// <summary>
        /// 音效Key
        /// </summary>
        public string SoundGroupKey { get; set; }

        /// <summary>
        /// 音效组Id
        /// </summary>
        public int Id;

        /// <summary>
        /// 音效类型
        /// </summary>
        public int SoundType;

        /// <summary>
        /// 音量
        /// </summary>
        public float SoundVolume { get; set; }

        /// <summary>
        /// 音效片段名
        /// </summary>
        public List<string> AudioClipNames = new List<string>();

        /// <summary>
        /// 关联的声音发射器
        /// </summary>
        public List<AudioPlayingData> PlayingDatas = new List<AudioPlayingData>();

        /// <summary>
        /// 音效播放开始
        /// </summary>
        public Action<int> OnStarted = null;

        /// <summary>
        /// 音效停止播放
        /// </summary>
        public Action<int> OnStopped = null;

        /// <summary>
        /// 处理音频开始播放
        /// </summary>
        public Action<int, string> OnOneClipStarted = null;

        /// <summary>
        /// 处理音频播放完成
        /// </summary>
        public Action<int, string> OnOneClipCompleted = null;

        /// <summary>
        /// 是否已开始播放
        /// </summary>
        private bool m_IsStartPlaying = false;

        /// <summary>
        /// 是否已完成
        /// </summary>
        internal bool IsCompleted { get => this.PlayingDatas.Count == 0; }

        /// <summary>
        /// 处理某个音频播放开始
        /// </summary>
        /// <param name="playingData"></param>
        internal void OnHandleOneStarted(AudioPlayingData playingData)
        {
            if (!m_IsStartPlaying)
            {
                m_IsStartPlaying = true;
                OnStarted?.Invoke(this.Id);
            }

            OnOneClipStarted?.Invoke(this.Id, playingData.AudioClipName);
        }

        /// <summary>
        /// 处理某个音频播放完成
        /// </summary>
        /// <param name="playingData"></param>
        internal void OnHandleOneCompleted(AudioPlayingData playingData)
        {
            OnOneClipCompleted?.Invoke(this.Id, playingData.AudioClipName);
        }

        /// <summary>
        /// 移除掉
        /// </summary>
        /// <param name="playingData"></param>
        internal void OnHandleOneRemove(AudioPlayingData playingData)
        {
            if (this.PlayingDatas.Remove(playingData))
            {
                if (IsCompleted)
                {
                    OnStopped?.Invoke(this.Id);
                }
            }
        }

        internal void InitBySoundParams(AudioDataParams audioParams)
        {
            this.SoundType     = audioParams.SoundType;
            this.SoundGroupKey = audioParams.SoundKey;
            this.OnStarted     = audioParams.OnStarted;
            this.OnStopped     = audioParams.OnStopped;
            this.SoundVolume   = audioParams.SoundVolume;
            this.AudioClipNames.AddRange(audioParams.AudioClipNames);
        }
    }
}