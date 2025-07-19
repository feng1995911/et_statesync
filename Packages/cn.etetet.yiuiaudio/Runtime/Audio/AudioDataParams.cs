using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class AudioDataParams : IPool
    {
        /// <summary>
        /// 请求参数
        /// </summary>
        /// <returns></returns>
        public static AudioDataParams Fetch()
        {
            return ObjectPool.Fetch<AudioDataParams>();
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Recycle()
        {
            this.Reset();
            ObjectPool.Recycle(this);
        }

        /// <summary>
        /// 声音类型
        /// </summary>
        public int SoundType = 0;

        /// <summary>
        /// 音效组
        /// </summary>
        public string SoundKey = string.Empty;

        /// <summary>
        /// 声音片段名
        /// </summary>
        public List<string> AudioClipNames = new List<string>();

        /// <summary>
        /// 开始回调
        /// </summary>
        public Action<int> OnStarted = null;

        /// <summary>
        /// 结束回调
        /// </summary>
        public Action<int> OnStopped = null;

        /// <summary>
        /// 某一个音效片段开始播放回调
        /// </summary>
        public Action<int, string> OnOneStarted = null;

        /// <summary>
        /// 某一个音效片段完成播放回调
        /// </summary>
        public Action<int, string> OnOneCompleted = null;

        /// <summary>
        /// 循环次数
        /// </summary>
        public int LoopTimes = 1;

        /// <summary>
        /// 音量大小
        /// </summary>
        public float SoundVolume = 1f;

        /// <summary>
        /// 首次最低延迟时间
        /// </summary>
        public float DelayTimeMin = 0f;

        /// <summary>
        /// 首次最高延迟时间
        /// </summary>
        public float DelayTimeMax = 0f;

        /// <summary>
        /// 后续最低延迟时间
        /// </summary>
        public float NextTimeMin = 0f;

        /// <summary>
        /// 后续最高延迟时间
        /// </summary>
        public float NextTimeMax = 0f;

        /// <summary>
        /// 音频可听最小范围
        /// </summary>
        public float DistanceMin = 500;

        /// <summary>
        /// 音频可听最大范围
        /// </summary>
        public float DistanceMax = 800;

        /// <summary>
        /// 音频混音器
        /// </summary>
        public string AudioMixerGroupName = null;

        /// <summary>
        /// 音频空间混合(0:2D->1:3D)
        /// </summary>
        public float SpatialBlend = 0f;

        /// <summary>
        /// 3D音跟随
        /// </summary>
        public Transform AudioSyncRootTrans = null;

        /// <summary>
        /// 声音偏移
        /// </summary>
        public Vector3 LocalPosition = default(Vector3);

        /// <summary>
        /// 如果3D跟随单位丢失，是否自动释放音效
        /// </summary>
        public bool IsAutoRelease = true;

        private void Reset()
        {
            this.SoundType = 0;
            this.SoundKey  = string.Empty;
            this.AudioClipNames.Clear();

            this.OnStarted      = null;
            this.OnOneStarted   = null;
            this.OnOneCompleted = null;
            this.OnStopped      = null;

            this.LoopTimes    = 1;
            this.SoundVolume  = 1f;
            this.DelayTimeMin = 0f;
            this.DelayTimeMax = 0f;
            this.NextTimeMin  = 0f;
            this.NextTimeMax  = 0f;
            this.DistanceMax  = 500;
            this.DistanceMax  = 800;

            this.AudioMixerGroupName = string.Empty;

            this.SpatialBlend       = 0f;
            this.AudioSyncRootTrans = null;
            this.LocalPosition      = default(Vector3);
            this.IsAutoRelease      = true;
        }

        public void Dispose()
        {
            this.Recycle();
        }

        public bool IsFromPool { get; set; }
    }
}