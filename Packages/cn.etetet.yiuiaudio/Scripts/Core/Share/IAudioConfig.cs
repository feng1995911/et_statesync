using System.Collections.Generic;

namespace ET
{
    public interface IAudioConfig
    {
        /// <summary>
        /// 获取GroupKey
        /// </summary>
        /// <returns></returns>
        string GetSoundKey();

        /// <summary>
        /// 获取音频名称
        /// </summary>
        /// <returns></returns>
        List<string> GetAudioClipNames();

        /// <summary>
        /// 获取声音的音量
        /// </summary>
        /// <returns></returns>
        float GetVolume();

        /// <summary>
        /// 获取当次播放的音频发射器数量
        /// </summary>
        /// <returns></returns>
        int GetEmitterCount();

        /// <summary>
        /// 声音空间混合比例，0 纯2D音，1 纯3D音
        /// </summary>
        /// <returns></returns>
        float GetSpatialBlend();

        /// <summary>
        /// 声音开始衰减距离
        /// </summary>
        /// <returns></returns>
        float GetDistanceMin();

        /// <summary>
        /// 声音完全衰减距离
        /// </summary>
        /// <returns></returns>
        float GetDistanceMax();

        /// <summary>
        /// 获取音效启动间隔
        /// </summary>
        /// <returns></returns>
        float GetIntervalTimeMin();

        /// <summary>
        /// 获取音效启动间隔
        /// </summary>
        /// <returns></returns>
        float GetIntervalTimeMax();

        /// <summary>
        /// 获取下一次间隔最小时间
        /// </summary>
        /// <returns></returns>
        float GetNextTimeMin();

        /// <summary>
        /// 获取下一次间隔最大时间
        /// </summary>
        /// <returns></returns>
        float GetNextTimeMax();
    }
}
