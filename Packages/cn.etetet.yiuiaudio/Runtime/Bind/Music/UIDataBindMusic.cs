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
    [LabelText("BindMusic 音乐背景")]
    [AddComponentMenu("YIUIBind/Data/音乐背景Bind 【BindMusic】 AudioUIDataBindMusic")]
    public sealed class UIDataBindMusic : UIDataBindSelectBase
    {
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

        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.String;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        private void OnDisable()
        {
            if (!m_ActivatePlay) return;
            StopLastMusic();
        }

        private void OnEnable()
        {
            if (!m_ActivatePlay) return;
            OnValueChanged();
        }

        protected override void OnValueChanged()
        {
            var dataValue = GetFirstValue<string>();

            if (string.IsNullOrEmpty(dataValue) && string.IsNullOrEmpty(m_LastMusic))
            {
                StopLastMusic();
                return;
            }

            if (m_LastMusic == dataValue)
            {
                return;
            }

            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            ChangeMusic(dataValue);
        }

        private void ChangeMusic(string musicName)
        {
            StopLastMusic();
            if (string.IsNullOrEmpty(musicName)) return;
            AudioMgr.Inst.PlayMusic(musicName, m_Priority, m_IsConfig);
            m_LastMusic = musicName;
        }

        protected override void UnBindData()
        {
            base.UnBindData();
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