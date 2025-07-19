using System;
using System.Collections.Generic;
using UnityEngine;
using YIUIFramework;

namespace ET
{
    /// <summary>
    /// 声音管理器
    /// </summary>
    [YIUISingleton]
    [AddComponentMenu("")]
    public partial class AudioMgr : YIUIMonoSingleton<AudioMgr>
    {
        private AudioClipController m_audioClipController;

        private AudioBackgroundMusicPlayer m_BackgroudMusicPlayer;

        private AudioEmitterController m_audioEmitterController;

        private Transform m_RootTransform;

        protected override void OnInitSingleton()
        {
            m_audioEmitterController = new();
            m_audioClipController = new();
            InitRootTrans();

            m_audioClipController.SetAudioClipLoader(new AudioClipLoader(Entity));
            m_audioEmitterController.InitManager(m_RootTransform, m_audioClipController);
            m_BackgroudMusicPlayer = new AudioBackgroundMusicPlayer(m_audioEmitterController);

            InitAudio();
        }

        partial void InitAudio();

        void InitRootTrans()
        {
            m_RootTransform = new GameObject($"[{nameof(AudioMgr)}]").transform;
            m_RootTransform.position = Vector3.zero;
            m_RootTransform.rotation = Quaternion.identity;
            m_RootTransform.localScale = Vector3.one;
            DontDestroyOnLoad(m_RootTransform);
        }

        public void SetKeepAliveTime(int aliveTime)
        {
            if (m_audioClipController != null)
            {
                m_audioClipController.KeepAliveTime = aliveTime;
            }
        }

        public IAudioConfig GetAudioConfig(string audioName)
        {
            return EventSystem.Instance.Invoke<YIUIInvokeGetAudioConfig, IAudioConfig>(new YIUIInvokeGetAudioConfig
            {
                AudioName = audioName
            });
        }

        public void SetSoundEmitterKeepMaxCount(int maxCount)
        {
            m_audioEmitterController?.SetSoundEmitterKeepMaxCount(maxCount);
        }

        private void Update()
        {
            m_audioClipController?.UpdateHandle();
            m_audioEmitterController?.UpdateHandle();
            m_BackgroudMusicPlayer?.UpdateHandle();
        }

        protected override void OnDestroy()
        {
            if (YIUISingletonHelper.IsQuitting) return;

            // 背景音乐释放掉
            m_BackgroudMusicPlayer.Dispose();

            // 释放掉音源池子
            m_audioEmitterController.DisposeManager();

            // 释放掉音频池子
            m_audioClipController.Dispose();

            // 释放根节点
            if (m_RootTransform != null)
            {
                Destroy(m_RootTransform.gameObject);
                m_RootTransform = null;
            }
        }

        /// <summary>
        /// 播放背景音
        /// </summary>
        /// <param name="soundKey">音效Key</param>
        /// <param name="priority">权值</param>
        /// <param name="isConfig">是否配置(是：SoundKey=配置Id 否：SoundKey = 音效片段名称)</param>
        public void PlayMusic(string soundKey, int priority = 0, bool isConfig = false)
        {
            if (isConfig)
            {
                var config = GetAudioConfig(soundKey);
                if (config == null)
                {
                    Debug.LogError($"通过配置[{soundKey}]播放背景音乐时，未找到对于的配置信息！请检查对应的配置文件！");
                    return;
                }

                PlayMusic(config.GetSoundKey(), config.GetAudioClipNames(), priority, config.GetVolume());
            }
            else
            {
                var list = ListComponent<string>.Create();
                list.Add(soundKey);
                PlayMusic(soundKey, list, priority);
                list.Dispose();
            }
        }

        /// <summary>
        /// 播放背景音
        /// </summary>
        /// <param name="soundKey"></param>
        /// <param name="audioClipNames"></param>
        /// <param name="priority"></param>
        /// <param name="soundVolume"></param>
        internal void PlayMusic(string soundKey, List<string> audioClipNames, int priority = 0, float soundVolume = 1f)
        {
            m_BackgroudMusicPlayer.PlayMusic(soundKey, audioClipNames, priority, soundVolume);
        }

        /// <summary>
        /// 停止背景音
        /// </summary>
        /// <param name="musicName"></param>
        /// <param name="priority"></param>
        /// <param name="isConfig"></param>
        public void StopMusic(string musicName, int priority = 0, bool isConfig = false)
        {
            if (isConfig)
            {
                var config = GetAudioConfig(musicName);
                if (config == null)
                {
                    Debug.LogError($"通过配置[{musicName}]播放背景音乐时，未找到对于的配置信息！请检查对应的配置文件！");
                    return;
                }

                m_BackgroudMusicPlayer.StopMusic(config.GetSoundKey(), priority);
            }
            else
            {
                m_BackgroudMusicPlayer.StopMusic(musicName, priority);
            }
        }

        /// <summary>
        /// 播放2D音
        /// </summary>
        /// <param name="soundKey">音效Key</param>
        /// <param name="soundVolume">音量大小(如果是配置音，此选项无效)</param>
        /// <param name="isConfig">是否配置(是：SoundKey=配置Id 否：SoundKey = 音效片段名称)</param>
        /// <returns></returns>
        public int Play2DAudio(string soundKey, float soundVolume = 1f, bool isConfig = false)
        {
            if (isConfig)
            {
                return Play2DAudioByConfigId(soundKey);
            }
            else
            {
                return Play2DAudioByAudioClip(soundKey, soundVolume);
            }
        }

        private int Play2DAudioByAudioClip(string soundKey, float soundVolume)
        {
            using var soundDataParams = AudioDataParams.Fetch();

            soundDataParams.SoundType = (int)EAudioType.Sound;
            soundDataParams.SoundKey = soundKey;
            soundDataParams.AudioClipNames.Add(soundKey);
            soundDataParams.SoundVolume = soundVolume;
            soundDataParams.SpatialBlend = 0f;

            return PlaySoundByParams(soundDataParams);
        }

        private int Play2DAudioByConfigId(string configId)
        {
            var config = GetAudioConfig(configId);
            if (config == null)
            {
                Debug.LogError($"通过配置[{configId}]播放背景音乐时，未找到对于的配置信息！请检查对应的配置文件！");
                return 0;
            }

            using var soundDataParam = AudioDataParams.Fetch();

            SetSoundDataParamsByConfig(soundDataParam, config);

            return PlaySoundByParams(soundDataParam);
        }

        /// <summary>
        /// 播放3D音
        /// </summary>
        /// <param name="soundKey">音效Key</param>
        /// <param name="attachTrans">依附对象</param>
        /// <param name="soundVolume">音量大小(如果是配置音，此选项无效)</param>
        /// <param name="offsetPosition">偏移值</param>
        /// <param name="isConfig">是否配置(是：SoundKey=配置Id 否：SoundKey = 音效片段名称)</param>
        /// <returns></returns>
        public int Play3DAudio(string soundKey, Transform attachTrans, float soundVolume = 1f, Vector3 offsetPosition = default(Vector3), bool isConfig = false)
        {
            if (isConfig)
            {
                return Play3DAudioByConfigId(soundKey, attachTrans, offsetPosition);
            }
            else
            {
                return Play3DAudioByClipName(soundKey, attachTrans, soundVolume, offsetPosition);
            }
        }

        private int Play3DAudioByClipName(string audioClipName, Transform transform, float volume, Vector3 position)
        {
            using var soundDataParams = AudioDataParams.Fetch();

            soundDataParams.SoundType = (int)EAudioType.Sound;
            soundDataParams.SoundKey = audioClipName;
            soundDataParams.AudioClipNames.Add(audioClipName);
            soundDataParams.SoundVolume = volume;
            soundDataParams.SpatialBlend = 0f;
            soundDataParams.AudioSyncRootTrans = transform;
            soundDataParams.LocalPosition = position;

            return PlaySoundByParams(soundDataParams);
        }

        private int Play3DAudioByConfigId(string configId, Transform transform, Vector3 position = default(Vector3))
        {
            var config = GetAudioConfig(configId);
            if (config == null)
            {
                Debug.LogError($"通过配置[{configId}]播放背景音乐时，未找到对于的配置信息！请检查对应的配置文件！");
                return 0;
            }

            using var soundDataParam = AudioDataParams.Fetch();

            SetSoundDataParamsByConfig(soundDataParam, config);

            soundDataParam.AudioSyncRootTrans = transform;
            soundDataParam.LocalPosition = position;

            return PlaySoundByParams(soundDataParam);
        }

        /// <summary>
        /// 停止声音
        /// </summary>
        /// <param name="audioKey">播放声音时返回的audioKey</param>
        public void StopAudio(int audioKey)
        {
            m_audioEmitterController.StopAudio(audioKey);
        }

        public int PlaySoundByParams(AudioDataParams audioDataParams)
        {
            return m_audioEmitterController.PlaySound(audioDataParams);
        }

        #region 按配置播放

        private static void SetSoundDataParamsByConfig(AudioDataParams audioDataParam, IAudioConfig config)
        {
            audioDataParam.SoundKey = config.GetSoundKey();

            int playCount = config.GetEmitterCount();
            var audioClipNames = config.GetAudioClipNames();

            if (playCount == 1 && audioClipNames.Count == 1)
            {
                audioDataParam.AudioClipNames.AddRange(audioClipNames);
            }
            else
            {
                audioDataParam.AudioClipNames.AddRange(RandomList(audioClipNames, playCount));
            }

            audioDataParam.SoundVolume = config.GetVolume();
            audioDataParam.NextTimeMin = config.GetNextTimeMin();
            audioDataParam.NextTimeMax = config.GetNextTimeMax();
            audioDataParam.DelayTimeMin = config.GetIntervalTimeMin();
            audioDataParam.DelayTimeMax = config.GetIntervalTimeMax();
            audioDataParam.DistanceMin = config.GetDistanceMin();
            audioDataParam.DistanceMax = config.GetDistanceMax();
            audioDataParam.SpatialBlend = config.GetSpatialBlend();
        }

        private static List<T> RandomList<T>(List<T> source, int resultCount)
        {
            List<T> result = new List<T>();

            System.Random random = new System.Random((int)TimeInfo.Instance.ClientNow());

            while (resultCount-- >= 0)
            {
                int randomIndex = random.Next(0, source.Count);
                result.Add(source[randomIndex]);
            }

            return result;
        }

        #endregion
    }
}