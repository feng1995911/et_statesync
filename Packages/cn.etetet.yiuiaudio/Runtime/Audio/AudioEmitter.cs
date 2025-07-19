using System;
using UnityEngine;
using UnityEngine.Audio;

namespace ET
{
    /// <summary>
    /// 声音发射器
    /// </summary>
    internal class AudioEmitter : IDisposable
    {
        private Transform m_RootTransform;

        private AudioSource m_AudioSource;

        /// <summary>
        /// 声音跟随脚本
        /// </summary>
        private AudioEmitterFollow m_FollowComponent;

        internal void Init()
        {
            if (m_RootTransform == null)
            {
                var go = new GameObject();
                m_AudioSource             = go.AddComponent<AudioSource>();
                m_AudioSource.playOnAwake = false;
                m_FollowComponent         = go.AddComponent<AudioEmitterFollow>();
                m_RootTransform           = go.transform;
            }
        }

        /// <summary>
        /// 设置位置坐标
        /// </summary>
        /// <param name="worldPosition"></param>
        internal void SetPosition(Vector3 worldPosition)
        {
            m_RootTransform.position = worldPosition;
        }

        /// <summary>
        /// 改变
        /// </summary>
        /// <param name="parent"></param>
        internal void SetParent(Transform parent)
        {
            if (m_RootTransform == null)
                return;

            if (m_RootTransform.parent == parent)
                return;

            m_RootTransform.SetParent(parent, true);
        }

        /// <summary>
        /// 设置音源名称
        /// </summary>
        /// <param name="emitterName"></param>
        internal void SetEmitterName(string emitterName)
        {
            if (m_RootTransform == null)
                return;

            this.m_RootTransform.name = emitterName;
        }

        /// <summary>
        /// 播放
        /// </summary>
        internal void Play()
        {
            m_AudioSource.time = 0;
            m_AudioSource.Play();
        }

        /// <summary>
        /// 设置音频
        /// </summary>
        /// <param name="audioClip"></param>
        internal void SetAudioClip(AudioClip audioClip)
        {
            if (m_AudioSource == null)
                return;

            m_AudioSource.clip    = audioClip;
            m_AudioSource.enabled = true;
        }

        internal void Stop()
        {
            if (m_AudioSource != null)
            {
                m_AudioSource.Stop();
            }
        }

        /// <summary>
        /// 停止发射器
        /// </summary>
        internal void StopEmitter()
        {
            if (m_AudioSource != null)
            {
                m_AudioSource.Stop();
                m_AudioSource.clip                  = null;
                m_AudioSource.loop                  = false;
                m_AudioSource.enabled               = false;
                m_AudioSource.outputAudioMixerGroup = null;

                m_AudioSource.priority = 128;
                m_AudioSource.volume   = 1f;
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="parent"></param>
        internal void Reset(Transform parent)
        {
            this.StopEmitter();

            this.m_RootTransform.SetParent(parent);
            this.m_RootTransform.localPosition = Vector3.zero;

            m_FollowComponent.ResetComponent();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        internal void Pause()
        {
            if (m_AudioSource != null)
            {
                m_AudioSource.Pause();
            }
        }

        /// <summary>
        /// 恢复
        /// </summary>
        internal void UnPause()
        {
            if (m_AudioSource != null)
            {
                m_AudioSource.UnPause();
            }
        }

        /// <summary>
        /// 获取音效播放时长
        /// </summary>
        /// <param name="defaultLength">默认时长</param>
        /// <returns></returns>
        public float GetClipLength(float defaultLength)
        {
            return m_AudioSource.clip == null ? defaultLength : m_AudioSource.clip.length;
        }

        /// <summary>
        /// 尝试获取音效长度
        /// </summary>
        /// <param name="clipLength"></param>
        /// <returns></returns>
        internal bool TryGetClipLength(out float clipLength)
        {
            if (m_AudioSource != null && m_AudioSource.clip != null)
            {
                clipLength = m_AudioSource.clip.length;
                return true;
            }

            clipLength = -1;
            return false;
        }

        /// <summary>
        /// 设置循环
        /// </summary>
        /// <param name="isLoop"></param>
        internal void SetLoop(bool isLoop)
        {
            m_AudioSource.loop = isLoop;
        }

        /// <summary>
        /// 设置混音器
        /// </summary>
        /// <param name="mixerGroup"></param>
        internal void SetOutputAudioMixerGroup(AudioMixerGroup mixerGroup)
        {
            m_AudioSource.outputAudioMixerGroup = mixerGroup;
        }

        /// <summary>
        /// 设置权值
        /// </summary>
        /// <param name="priority"></param>
        internal void SetPriority(int priority = 128)
        {
            m_AudioSource.priority = priority;
        }

        /// <summary>
        /// 设置SpatialBlend
        /// </summary>
        /// <param name="spatialBlend"></param>
        internal void SetSpatialBlend(float spatialBlend = 0f)
        {
            m_AudioSource.spatialBlend = spatialBlend;
        }

        /// <summary>
        /// 设置音效播放距离
        /// </summary>
        /// <param name="minDistance"></param>
        /// <param name="maxDistance"></param>
        internal void SetAttenuationDistance(float minDistance, float maxDistance)
        {
            // 1、设置的最小值如果小于当前的最大值--设置的值最小值将不生效
            // 2、设置的最大值如果小于当前的最小值--设置的值最大值将不生效
            // 即：设置的max 要大于 当前的min值，设置的min 要小于 当前的max值 才会生效
            // 故此处冗余设置一次最大值，用于保证正确
            m_AudioSource.maxDistance = maxDistance;
            m_AudioSource.minDistance = minDistance;
            m_AudioSource.maxDistance = maxDistance;
            m_AudioSource.rolloffMode = AudioRolloffMode.Linear; // 固定设置为直线衰减，默认的衰减方式会导致音效和配置的最大距离不匹配
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        internal void SetVolume(float volume)
        {
            m_AudioSource.volume = volume;
        }

        /// <summary>
        /// 设置跟随信息
        /// </summary>
        /// <param name="audioKey">音效Key</param>
        /// <param name="targetTrans">跟随目标</param>
        /// <param name="offsetPos">位置偏移</param>
        /// <param name="isAutoRelease">目标丢失时，是否自动释放</param>
        /// <param name="isWorking">是否工作</param>
        internal void SetFollow(AudioEmitterController controller, int audioKey, Transform targetTrans, Vector3 offsetPos, bool isAutoRelease = true, bool isWorking = false)
        {
            m_FollowComponent.SetFollow(controller, audioKey, targetTrans, offsetPos, isAutoRelease, isWorking);
        }

        private bool m_IsDisposed = false;

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
                return;

            m_IsDisposed = true;

            StopEmitter();

            if (this.m_RootTransform != null)
            {
                DestroyGameObject(this.m_RootTransform.gameObject);
                this.m_RootTransform = null;
            }

            this.m_AudioSource     = null;
            this.m_FollowComponent = null;
        }

        private void DestroyGameObject(GameObject go)
        {
            #if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(go);
            #else
            UnityEngine.Object.Destroy(go);
            #endif
        }
    }
}