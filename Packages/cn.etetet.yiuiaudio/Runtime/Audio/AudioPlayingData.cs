using UnityEngine;

namespace ET
{
    /// <summary>
    /// 声音播放数据
    /// </summary>
    class AudioPlayingData
    {
        /// <summary>
        /// 音效片段名称
        /// </summary>
        public string AudioClipName;

        /// <summary>
        /// 音效发射器
        /// </summary>
        public AudioEmitter mAudioEmitter;

        /// <summary>
        /// 播放时长
        /// </summary>
        public float PlayTime = 0;

        /// <summary>
        /// 已播放次数
        /// </summary>
        public int PlayedTimes { get; set; }

        /// <summary>
        /// 重复次数
        /// </summary>
        public int LoopTimes = 1;

        /// <summary>
        /// 延迟时间范围
        /// </summary>
        public Vector2 DelayTimeRange;

        /// <summary>
        /// 获取延迟时间
        /// </summary>
        /// <returns></returns>
        public float GetDelayTime()
        {
            return UnityEngine.Random.Range(DelayTimeRange.x, DelayTimeRange.y);
        }

        /// <summary>
        /// 状态
        /// </summary>
        public EAudioSourceStatus StatusType = EAudioSourceStatus.None;
    }
}