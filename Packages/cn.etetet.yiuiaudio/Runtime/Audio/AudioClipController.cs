using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class AudioClipController : IDisposable
    {
        /// <summary>
        /// 声音片段状态
        /// </summary>
        enum EAudioClipStatus
        {
            None,

            /// <summary>
            /// 加载中
            /// </summary>
            Loading,

            /// <summary>
            /// 加载完成
            /// </summary>
            Completed,

            /// <summary>
            /// 等待被释放
            /// </summary>
            WaitingRelease,
        }

        /// <summary>
        /// 缓存的声音片段数据
        /// </summary>
        class AudioClipCacheData : IDisposable
        {
            /// <summary>
            /// 引用计数
            /// </summary>
            public int ReferencedCount { get; set; }

            /// <summary>
            /// 音效Key
            /// </summary>
            public string AudioKey { get; set; }

            /// <summary>
            /// 音效资源
            /// </summary>
            public AudioClip AudioClip { get; set; }

            /// <summary>
            /// 状态
            /// </summary>
            public EAudioClipStatus Status = EAudioClipStatus.None;

            /// <summary>
            /// 完成回调
            /// </summary>
            public Action<AudioClip> LoadCompleted;

            /// <summary>
            /// 等待清理时间点
            /// </summary>
            public float WaitReleaseTime = 0f;

            public void Dispose()
            {
                AudioKey        = string.Empty;
                ReferencedCount = 0;
                Status          = EAudioClipStatus.None;
                LoadCompleted   = null;

                if (AudioClip != null)
                {
                    AudioClip.UnloadAudioData();
                    AudioClip = null;
                }
            }
        }

        private bool m_IsDisposed = false;

        /// <summary>
        /// 释放后保留时间
        /// </summary>
        public float KeepAliveTime = 5f;

        private readonly Dictionary<string, AudioClipCacheData> m_AudioClipCacheData = new();

        private IAudioClipLoader m_AudioClipLoader;

        public bool CanLoadAudioClip()
        {
            return m_AudioClipLoader != null;
        }

        public void SetAudioClipLoader(IAudioClipLoader audioClipLoader)
        {
            if (audioClipLoader == null)
            {
                Debug.LogError("AudioClipManager 中的 AudioClipLoader 不允许设置未空!");
                return;
            }

            if (m_AudioClipLoader != null && m_AudioClipLoader != audioClipLoader)
            {
                Debug.LogError("AudioClipManager 中的 AudioClipLoader 不允许被重复设置!");
                return;
            }

            m_AudioClipLoader = audioClipLoader;
        }

        /// <summary>
        /// 加载音效文件
        /// </summary>
        public void LoadAudioClipAsync(string clipName, Action<AudioClip> loadCompleted)
        {
            if (m_IsDisposed)
            {
                loadCompleted?.Invoke(null);
                return;
            }

            if (!m_AudioClipCacheData.TryGetValue(clipName, out AudioClipCacheData audioClipCache))
            {
                audioClipCache          = new AudioClipCacheData();
                audioClipCache.AudioKey = clipName;

                m_AudioClipCacheData.Add(clipName, audioClipCache);
                audioClipCache.Status = EAudioClipStatus.Loading;
                InternalLoadAudioClipAsync(clipName);
            }

            audioClipCache.ReferencedCount++;

            switch (audioClipCache.Status)
            {
                case EAudioClipStatus.Loading:
                {
                    if (audioClipCache.LoadCompleted == null)
                    {
                        audioClipCache.LoadCompleted = loadCompleted;
                    }
                    else
                    {
                        audioClipCache.LoadCompleted += loadCompleted;
                    }

                    return;
                }
                case EAudioClipStatus.Completed:
                {
                    loadCompleted?.Invoke(audioClipCache.AudioClip);
                    return;
                }
                case EAudioClipStatus.WaitingRelease:
                {
                    audioClipCache.Status = EAudioClipStatus.Completed;
                    loadCompleted?.Invoke(audioClipCache.AudioClip);
                    return;
                }
            }
        }

        private void InternalLoadAudioClipAsync(string clipName)
        {
            void CallBack(AudioClip audioClip)
            {
                if (this.m_IsDisposed)
                {
                    if (audioClip != null)
                    {
                        audioClip.UnloadAudioData();
                    }

                    this.m_AudioClipLoader?.UnloadAudioClip(audioClip);
                    return;
                }

                // 如果没有找到，说明已经释放掉了，不需要再设置
                if (this.m_AudioClipCacheData.TryGetValue(clipName, out var cacheData))
                {
                    cacheData.AudioClip = audioClip;
                    if (cacheData.Status == EAudioClipStatus.Loading)
                    {
                        cacheData.Status = EAudioClipStatus.Completed;
                        cacheData.LoadCompleted?.Invoke(audioClip);
                        cacheData.LoadCompleted = null;
                    }
                }
            }

            m_AudioClipLoader.LoadAudioClip(clipName, CallBack);
        }

        /// <summary>
        /// 释放音效文件        
        /// </summary>
        /// <param name="clipName"></param>
        public void Recycle(string clipName)
        {
            if (m_IsDisposed)
                return;

            if (m_AudioClipCacheData.TryGetValue(clipName, out AudioClipCacheData audioClipCache))
            {
                audioClipCache.ReferencedCount--;

                if (audioClipCache.ReferencedCount == 0)
                {
                    if (audioClipCache.Status != EAudioClipStatus.WaitingRelease)
                    {
                        audioClipCache.Status          = EAudioClipStatus.WaitingRelease;
                        audioClipCache.WaitReleaseTime = Time.realtimeSinceStartup + KeepAliveTime;
                        m_WaitingReleaseAudioClips.Enqueue(clipName);
                    }
                }
                else if (audioClipCache.ReferencedCount < 0)
                {
                    Debug.LogError("存在多次释放音效的情况，导致加载数量和释放数量不配对！");
                }
            }
        }

        private readonly Queue<string> m_WaitingReleaseAudioClips = new Queue<string>();

        private int m_WaitingCount;

        public void UpdateHandle()
        {
            m_WaitingCount = m_WaitingReleaseAudioClips.Count;
            while (m_WaitingCount > 0)
            {
                m_WaitingCount--;
                string clipName = m_WaitingReleaseAudioClips.Dequeue();
                if (!m_AudioClipCacheData.TryGetValue(clipName, out AudioClipCacheData cacheData))
                {
                    return;
                }

                if (cacheData.Status != EAudioClipStatus.WaitingRelease)
                {
                    return;
                }

                if (cacheData.WaitReleaseTime < Time.realtimeSinceStartup)
                {
                    // 字典移除掉
                    m_AudioClipCacheData.Remove(clipName);

                    // 资源管理器中释放
                    m_AudioClipLoader.UnloadAudioClip(cacheData.AudioClip);

                    // 释放音源内存
                    cacheData.Dispose();
                    return;
                }
                else
                {
                    m_WaitingReleaseAudioClips.Enqueue(clipName);
                }
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
                return;
            m_IsDisposed = true;

            foreach (var cacheData in m_AudioClipCacheData.Values)
            {
                if (cacheData != null && cacheData.AudioClip != null)
                {
                    // 资源管理器中释放
                    m_AudioClipLoader.UnloadAudioClip(cacheData.AudioClip);
                    cacheData.Dispose();
                }
            }

            m_AudioClipCacheData.Clear();
            m_AudioClipLoader = null;
        }
    }
}