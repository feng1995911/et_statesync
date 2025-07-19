using UnityEngine.Audio;

namespace ET
{
    public interface IAudioMixerGroupManager
    {
        /// <summary>
        /// 获取混音器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        AudioMixerGroup GetAudioMixerGroup(string name);
    }
}