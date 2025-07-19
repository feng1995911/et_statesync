using System;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 音效加载器接口
    /// </summary>
    public interface IAudioClipLoader
    {
        /// <summary>
        /// 释放音效资源
        /// </summary>
        /// <param name="clip"></param>
        void UnloadAudioClip(AudioClip clip);

        /// <summary>
        /// 加载音效资源
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="loadCompleted"></param>
        void LoadAudioClip(string clipName, Action<AudioClip> loadCompleted);
    }
}