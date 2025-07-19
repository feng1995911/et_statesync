using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class AudioSystem
    {
        /// <summary>
        /// 音量改变事件(音量类型，音量大小)
        /// </summary>
        public static event Action<int, float> OnVolumeChangedEvent;

        /// <summary>
        /// 音量字典
        /// </summary>
        [StaticField]
        private static readonly Dictionary<int, float> s_SoundVolumes = new();

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="volume"></param>
        public static void SetVolume(int soundType, float volume)
        {
            volume = Mathf.Clamp01(volume);
            var oldVolume = GetVolume(soundType, volume);
            s_SoundVolumes[soundType] = volume;

            if (Math.Abs(oldVolume - volume) > 0.001f)
            {
                OnVolumeChangedEvent?.Invoke(soundType, volume);
            }
        }

        /// <summary>
        /// 获取音量
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float GetVolume(int soundType, float defaultValue = 1.0f)
        {
            return s_SoundVolumes.GetValueOrDefault(soundType, defaultValue);
        }
    }
}