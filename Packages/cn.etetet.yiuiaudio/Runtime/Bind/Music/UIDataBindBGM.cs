using System;
using ET;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 播放背景音乐
    /// 传入想要播放的音乐名称 就会播放
    /// 如果之前播放过其他音乐 就会先暂停上一个 播放现在
    /// 如果传入null 就会暂停上一个
    /// 如果被摧毁 也会停止播放 上一个音乐
    /// </summary>
    [LabelText("BGMMusic 音乐背景")]
    [AddComponentMenu("YIUIBind/Data/音乐背景Mono 【BGMMusic】 AudioUIDataBindBGM")]
    public sealed class UIDataBindBGM : MonoBehaviour
    {
        [SerializeField]
        [LabelText("音效名称")]
        private string m_MusicNmae;

        [SerializeField]
        [LabelText("优先级")]
        private int m_Priority = 10;

        [SerializeField]
        [LabelText("激活时播放")]
        private bool m_ActivatePlay = true;

        [SerializeField]
        [LabelText("使用配置")]
        private bool m_IsConfig;

        private string m_LastMusic;

        private void OnDisable()
        {
            if (!m_ActivatePlay) return;
            StopLastMusic();
        }

        private void OnEnable()
        {
            if (!m_ActivatePlay) return;
            PlayMusic();
        }

        private void Start()
        {
            PlayMusic();
        }

        private void PlayMusic()
        {
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            ChangeMusic(m_MusicNmae);
        }

        private void ChangeMusic(string musicName)
        {
            StopLastMusic();
            if (string.IsNullOrEmpty(musicName)) return;
            AudioMgr.Inst.PlayMusic(musicName, m_Priority, m_IsConfig);
            m_LastMusic = musicName;
        }

        private void OnDestroy()
        {
            StopLastMusic();
        }

        private void StopLastMusic()
        {
            if (string.IsNullOrEmpty(m_LastMusic)) return;
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            AudioMgr.Inst.StopMusic(m_LastMusic, m_Priority, m_IsConfig);
            m_LastMusic = "";
        }
    }
}