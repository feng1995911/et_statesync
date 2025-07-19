using System;
using UnityEngine;
using YIUIFramework;

namespace ET
{
    [EnableClass]
    public class AudioClipLoader : IAudioClipLoader
    {
        private readonly EntityRef<Entity> m_EntityRef;

        public AudioClipLoader(Entity entity)
        {
            m_EntityRef = entity;
        }

        public void UnloadAudioClip(AudioClip clip)
        {
            EventSystem.Instance?.YIUIInvokeEntitySync(m_EntityRef, new YIUIInvokeEntity_Release { obj = clip });
        }

        public void LoadAudioClip(string clipName, Action<AudioClip> loadCompleted)
        {
            var audioClip = EventSystem.Instance?.YIUIInvokeEntitySync<YIUIInvokeLoadAudioClip, AudioClip>(m_EntityRef, new YIUIInvokeLoadAudioClip
            {
                ResName = clipName,
            });        

            if (audioClip == null)
            {
                Debug.LogError($"音频: {clipName} 没有找到!");
                return;
            }

            loadCompleted?.Invoke(audioClip);
        }
    }
}