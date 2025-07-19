using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public enum EAudioSourceStatus
    {
        None          = 0,
        Loading       = 1,
        LoadCompleted = 2,
        Delaying      = 3,
        Playing       = 4,
        Pause         = 5,
        Completed     = 6,
    }

    public class AudioEmitterController
    {
        private AudioClipController m_AudioClipController = null;

        private AudioEmitterPool m_AudioEmitterPool = null;

        private Transform m_EmitterRoot = null;

        /// <summary>
        /// 初始化管理器
        /// </summary>
        public void InitManager(Transform emitterRoot, AudioClipController clipController, int idleKeepMaxCount = 10)
        {
            m_IsDisposed                     =  false;
            m_AudioClipController            =  clipController;
            m_EmitterRoot                    =  emitterRoot;
            m_AudioEmitterPool               =  new AudioEmitterPool(m_EmitterRoot, idleKeepMaxCount);
            AudioSystem.OnVolumeChangedEvent += SoundSystem_OnVolumeChangedEvent;
        }

        /// <summary>
        /// 释放管理器
        /// </summary>
        public void DisposeManager()
        {
            AudioSystem.OnVolumeChangedEvent -= SoundSystem_OnVolumeChangedEvent;
            m_IsDisposed                     =  true;

            // 释放掉音源池子
            m_AudioEmitterPool.Dispose();

            // 清理正在播放的声音数据
            foreach (var soundPlayingGroupData in m_SoundPlayingGroupDataDict.Values)
            {
                foreach (var soundPlayingData in soundPlayingGroupData.PlayingDatas)
                {
                    m_AudioClipController.Recycle(soundPlayingData.AudioClipName);
                    soundPlayingData.mAudioEmitter?.Dispose();
                }

                soundPlayingGroupData.PlayingDatas.Clear();
            }

            m_SoundPlayingGroupDataDict.Clear();
        }

        private void SoundSystem_OnVolumeChangedEvent(int soundType, float volume)
        {
            foreach (var soundPlayingGroupData in m_SoundPlayingGroupDataDict.Values)
            {
                if (soundPlayingGroupData.SoundType != soundType)
                {
                    continue;
                }

                float soundVolume = GetVolume(soundPlayingGroupData.SoundType, soundPlayingGroupData.SoundVolume);

                foreach (var soundPlayingData in soundPlayingGroupData.PlayingDatas)
                {
                    soundPlayingData.mAudioEmitter?.SetVolume(soundVolume);
                }
            }
        }

        public IAudioMixerGroupManager AudioMixerGroupManager { get; set; }

        /// <summary>
        /// 空闲声音发射器保留最大数量
        /// </summary>
        /// <param name="maxCount"></param>
        public void SetSoundEmitterKeepMaxCount(int maxCount)
        {
            if (maxCount < 0)
            {
                maxCount = 0;
            }

            if (m_AudioEmitterPool != null)
            {
                m_AudioEmitterPool.KeepMaxCount = maxCount;
            }
        }

        /// <summary>
        /// 正在播放的音效资源
        /// </summary>
        private readonly Dictionary<int, AudioPlayingGroupData> m_SoundPlayingGroupDataDict = new Dictionary<int, AudioPlayingGroupData>();

        private int m_PlayingAudioIndex = 0;

        /// <summary>
        /// 获取播放Id
        /// </summary>
        /// <returns></returns>
        private int GetPlayingId()
        {
            while (true)
            {
                m_PlayingAudioIndex++;
                if (m_PlayingAudioIndex > 1000000)
                {
                    m_PlayingAudioIndex = 1;
                }

                if (m_SoundPlayingGroupDataDict.ContainsKey(m_PlayingAudioIndex))
                {
                    continue;
                }

                return m_PlayingAudioIndex;
            }
        }

        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="audioParams">声音参数包</param>
        /// <returns></returns>
        public int PlaySound(AudioDataParams audioParams)
        {
            int playingId = 0;

            if (string.IsNullOrEmpty(audioParams.SoundKey))
                return playingId;

            if (audioParams.AudioClipNames.Count == 0)
                return playingId;

            if (!CanPlayAudio(audioParams.SoundKey))
                return playingId;

            playingId = GetPlayingId();

            var playingGroupData = new AudioPlayingGroupData();
            playingGroupData.Id = playingId;
            playingGroupData.InitBySoundParams(audioParams);
            float delayTime     = 0f;
            float emitterVolume = GetVolume(audioParams.SoundType, audioParams.SoundVolume);

            foreach (var audioClipName in playingGroupData.AudioClipNames)
            {
                var playingData = new AudioPlayingData();

                playingData.AudioClipName = audioClipName;

                delayTime                  += UnityEngine.Random.Range(audioParams.DelayTimeMin, audioParams.DelayTimeMax);
                playingData.DelayTimeRange =  new Vector2(audioParams.NextTimeMin, audioParams.NextTimeMax);

                playingData.LoopTimes = audioParams.LoopTimes;

                var soundEmitter = m_AudioEmitterPool.Fetch(audioClipName);
                playingData.mAudioEmitter = soundEmitter;

                soundEmitter.SetLoop(audioParams.LoopTimes == -1);
                soundEmitter.SetParent(m_EmitterRoot);
                soundEmitter.SetAttenuationDistance(audioParams.DistanceMin, audioParams.DistanceMax);
                soundEmitter.SetOutputAudioMixerGroup(AudioMixerGroupManager?.GetAudioMixerGroup(audioParams.AudioMixerGroupName));
                soundEmitter.SetVolume(emitterVolume);
                soundEmitter.SetFollow(this, playingId, audioParams.AudioSyncRootTrans, audioParams.LocalPosition, isAutoRelease: audioParams.IsAutoRelease, isWorking: audioParams.AudioSyncRootTrans != null);
                soundEmitter.SetPriority();
                soundEmitter.SetSpatialBlend(audioParams.SpatialBlend);

                playingData.StatusType = EAudioSourceStatus.Loading;

                playingGroupData.PlayingDatas.Add(playingData);
            }

            InternalPlayAudio(playingGroupData);

            m_SoundPlayingGroupDataDict[playingId] = playingGroupData;

            return playingId;
        }

        /// <summary>
        /// 更新单个音源声音大小
        /// </summary>
        /// <param name="audioId"></param>
        /// <param name="volume"></param>
        public void UpdateSoundVolume(int audioId, float volume)
        {
            if (m_SoundPlayingGroupDataDict.TryGetValue(audioId, out var soundPlayingGroupData))
            {
                soundPlayingGroupData.SoundVolume = volume;

                float soundVolume = GetVolume(soundPlayingGroupData.SoundType, soundPlayingGroupData.SoundVolume);

                foreach (var soundPlayingData in soundPlayingGroupData.PlayingDatas)
                {
                    soundPlayingData.mAudioEmitter?.SetVolume(soundVolume);
                }
            }
        }

        /// <summary>
        /// 获取音量大小
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        private float GetVolume(int soundType, float volume)
        {
            return AudioSystem.GetVolume(soundType, 1) * volume;
        }

        /// <summary>
        /// 尝试获取音效的时长
        /// </summary>
        /// <param name="audioId">音效Id</param>
        /// <param name="clipLenght">音频时长</param>
        /// <returns></returns>
        public bool TryGetAudioClipLength(int audioId, out float clipLenght)
        {
            if (m_SoundPlayingGroupDataDict.TryGetValue(audioId, out var playingGroupData))
            {
                if (playingGroupData.PlayingDatas.Count > 0)
                {
                    if (playingGroupData.PlayingDatas[0].mAudioEmitter.TryGetClipLength(out clipLenght))
                    {
                        return true;
                    }
                }
            }

            clipLenght = -1;
            return false;
        }

        /// <summary>
        /// 尝试获取对应的声音音量
        /// </summary>
        /// <param name="audioId"></param>
        /// <param name="soundVolume"></param>
        /// <returns></returns>
        public bool TryGetAudioVolume(int audioId, out float soundVolume)
        {
            if (m_SoundPlayingGroupDataDict.TryGetValue(audioId, out var playingGroupData))
            {
                soundVolume = playingGroupData.SoundVolume;
                return true;
            }

            soundVolume = 1f;
            return false;
        }

        /// <summary>
        /// 尝试获取对应的音效名称
        /// </summary>
        /// <param name="audioId"></param>
        /// <param name="audioClipName"></param>
        /// <returns></returns>
        public bool TryGetAudioClipName(int audioId, out string audioClipName)
        {
            if (m_SoundPlayingGroupDataDict.TryGetValue(audioId, out var playingGroupData))
            {
                if (playingGroupData.AudioClipNames.Count > 0)
                {
                    audioClipName = playingGroupData.AudioClipNames[0];
                    return true;
                }
            }

            audioClipName = string.Empty;
            return false;
        }

        /// <summary>
        /// 更新音源位置
        /// </summary>
        /// <param name="audioId"></param>
        /// <param name="worldPosition"></param>
        public void UpdateAudioSourcePosition(int audioId, Vector3 worldPosition)
        {
            if (m_SoundPlayingGroupDataDict.TryGetValue(audioId, out var playingGroupData))
            {
                if (playingGroupData.PlayingDatas.Count > 0)
                {
                    foreach (var playingData in playingGroupData.PlayingDatas)
                    {
                        if (playingData.mAudioEmitter != null)
                        {
                            playingData.mAudioEmitter.SetPosition(worldPosition);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新音效的距离
        /// </summary>
        /// <param name="audioId"></param>
        /// <param name="minDistance"></param>
        /// <param name="maxDistance"></param>
        public void ChangeAttenuationDistance(int audioId, float minDistance, float maxDistance)
        {
            if (m_SoundPlayingGroupDataDict.TryGetValue(audioId, out var soundPlayingGroupData))
            {
                foreach (var soundPlayingData in soundPlayingGroupData.PlayingDatas)
                {
                    soundPlayingData.mAudioEmitter?.SetAttenuationDistance(minDistance, maxDistance);
                }
            }
        }

        private bool m_IsDisposed = false;

        void InternalPlayAudio(AudioPlayingGroupData playingGroupData)
        {
            int audioId = playingGroupData.Id;
            foreach (var playingData in playingGroupData.PlayingDatas)
            {
                void CallBack(AudioClip audioClip)
                {
                    // ps:如果资源加载回来时，播放声音数据已经不存在，因为在清理的时候，音源已经回收，故此处不需要再回收
                    if (playingData.mAudioEmitter != null)
                    {
                        playingData.mAudioEmitter.SetAudioClip(audioClip);
                        if (playingData.PlayTime <= 0)
                        {
                            playingData.StatusType = EAudioSourceStatus.Playing;
                            this.OnHandlePlayAudioSource(playingData, playingGroupData);
                        }
                        else if (playingData.StatusType == EAudioSourceStatus.Completed)
                        {
                            // 已经被释放掉了，不需要再处理，静等回收即可
                        }
                        else
                        {
                            // 还属于在延迟播放期间，直接设置资源加载完成
                            playingData.StatusType = EAudioSourceStatus.LoadCompleted;
                        }
                    }
                }

                m_AudioClipController.LoadAudioClipAsync(playingData.AudioClipName, CallBack);
            }
        }

        private void OnHandlePlayAudioSource(AudioPlayingData playingData, AudioPlayingGroupData ownerGroup)
        {
            playingData.mAudioEmitter.Play();
            playingData.PlayTime = playingData.mAudioEmitter.GetClipLength(1f);
            playingData.PlayedTimes++;
            if (playingData.LoopTimes > 0)
            {
                playingData.LoopTimes--;
            }

            ownerGroup.OnHandleOneStarted(playingData);
        }

        private bool CanPlayAudio(string audioClipName)
        {
            if (m_IsDisposed)
                return false;

            if (!m_AudioClipController.CanLoadAudioClip())
                return false;

            return true;
        }

        /// <summary>
        /// 停止播放音效
        /// </summary>
        /// <param name="audioKey"></param>
        public void StopAudio(int audioKey)
        {
            if (m_IsDisposed)
                return;

            if (m_SoundPlayingGroupDataDict.TryGetValue(audioKey, out var soundPlayingGroupData))
            {
                foreach (var soundPlayingData in soundPlayingGroupData.PlayingDatas)
                {
                    StopSoundPlayingData(soundPlayingData);
                }
            }
        }

        private void StopSoundPlayingData(AudioPlayingData playingData)
        {
            if (playingData.mAudioEmitter != null)
            {
                playingData.mAudioEmitter.StopEmitter();
            }

            playingData.StatusType = EAudioSourceStatus.Completed;
        }

        readonly List<int> m_WaitingClearGroupIds = new List<int>();

        public void UpdateHandle()
        {
            if (m_IsDisposed)
                return;

            float deltaTime = Time.unscaledDeltaTime;
            foreach (var playingGroupData in m_SoundPlayingGroupDataDict.Values)
            {
                for (int i = 0; i < playingGroupData.PlayingDatas.Count; i++)
                {
                    var playingData = playingGroupData.PlayingDatas[i];
                    switch (playingData.StatusType)
                    {
                        case EAudioSourceStatus.Playing:
                            playingData.PlayTime -= deltaTime;
                            if (playingData.PlayTime <= 0)
                            {
                                try
                                {
                                    playingGroupData.OnHandleOneCompleted(playingData);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogException(ex);
                                }

                                if (playingData.LoopTimes > 0)
                                {
                                    // 如果需要重复则判断下下次的随机时间，如果大于0，则进入延迟时间状态
                                    float delayTime = playingData.GetDelayTime();
                                    if (delayTime > 0)
                                    {
                                        playingData.mAudioEmitter.Stop();
                                        playingData.PlayTime   = delayTime;
                                        playingData.StatusType = EAudioSourceStatus.Delaying;
                                    }
                                    else
                                    {
                                        OnHandlePlayAudioSource(playingData, playingGroupData);
                                    }
                                }
                                else if (playingData.LoopTimes == 0)
                                {
                                    StopSoundPlayingData(playingData);
                                }
                            }

                            break;
                        case EAudioSourceStatus.None:
                        case EAudioSourceStatus.Loading:
                            playingData.PlayTime -= deltaTime;
                            break;
                        case EAudioSourceStatus.LoadCompleted:
                        case EAudioSourceStatus.Delaying:
                            playingData.PlayTime -= deltaTime;
                            if (playingData.PlayTime <= 0)
                            {
                                OnHandlePlayAudioSource(playingData, playingGroupData);
                            }

                            break;
                        case EAudioSourceStatus.Completed:
                            playingGroupData.OnHandleOneRemove(playingData);

                            // 因为移除掉了一个，故此处索引减1
                            i--;

                            // 回收掉这个
                            m_AudioEmitterPool.Recycle(playingData.mAudioEmitter);
                            m_AudioClipController.Recycle(playingData.AudioClipName);
                            if (playingGroupData.IsCompleted)
                            {
                                m_WaitingClearGroupIds.Add(playingGroupData.Id);
                            }

                            break;
                        default:
                            break;
                    }
                }
            }

            // 清理
            if (m_WaitingClearGroupIds.Count > 0)
            {
                foreach (var clearGroupId in m_WaitingClearGroupIds)
                {
                    if (m_SoundPlayingGroupDataDict.TryGetValue(clearGroupId, out var groupData))
                    {
                        m_SoundPlayingGroupDataDict.Remove(clearGroupId);

                        // 冗余处理下，理论上这个对象的音源数据都清空了
                        if (groupData.PlayingDatas.Count > 0)
                        {
                            foreach (var playingData in groupData.PlayingDatas)
                            {
                                m_AudioEmitterPool.Recycle(playingData.mAudioEmitter);
                                m_AudioClipController.Recycle(playingData.AudioClipName);
                            }

                            groupData.PlayingDatas.Clear();
                        }
                    }
                }

                m_WaitingClearGroupIds.Clear();
            }
        }

        /// <summary>
        /// 暂停音源
        /// </summary>
        /// <param name="audioId"></param>
        /// <param name="isPause"></param>
        public void PauseAudio(int audioId, bool isPause)
        {
            if (m_SoundPlayingGroupDataDict.TryGetValue(audioId, out var soundPlayingGroupData))
            {
                foreach (var soundPlayingData in soundPlayingGroupData.PlayingDatas)
                {
                    if (isPause)
                    {
                        if (soundPlayingData.StatusType == EAudioSourceStatus.Playing)
                        {
                            soundPlayingData.StatusType = EAudioSourceStatus.Pause;
                            soundPlayingData.mAudioEmitter.Pause();
                        }
                        else
                        {
                            Debug.LogError($"暂停音效【{audioId}]:{soundPlayingData.AudioClipName}】异常，音效当前状态[{soundPlayingData.StatusType}]非播放状态!");
                        }
                    }
                    else
                    {
                        if (soundPlayingData.StatusType == EAudioSourceStatus.Pause)
                        {
                            soundPlayingData.StatusType = EAudioSourceStatus.Playing;
                            soundPlayingData.mAudioEmitter.UnPause();
                        }
                        else
                        {
                            Debug.LogError($"恢复音效【{audioId}]:{soundPlayingData.AudioClipName}】异常，音效当前状态[{soundPlayingData.StatusType}]非暂停状态!");
                        }
                    }
                }
            }
        }
    }
}