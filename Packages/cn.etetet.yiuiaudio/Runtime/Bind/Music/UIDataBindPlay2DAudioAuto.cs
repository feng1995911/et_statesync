using ET;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 自动播放音效2D
    /// OnEnable直接触发
    /// 没有控制开关
    /// </summary>
    [LabelText("Play 音效2D播放自动")]
    [AddComponentMenu("YIUIBind/Data/音效2D播放自动 【Play2DAudioAuto】 AudioUIDataBindPlayAuto2DAudio")]
    public sealed class UIDataBindPlay2DAudioAuto : MonoBehaviour
    {
        [SerializeField]
        [LabelText("2D音效名称")]
        private string m_MusicNmae;

        [SerializeField]
        [Range(0f, 1f)]
        [LabelText("音量")]
        private float m_Volume = 1;

        [SerializeField]
        [LabelText("使用配置")]
        private bool m_IsConfig;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(m_MusicNmae))
            {
                Debug.LogError($"未配置点击音效请检查 {this.gameObject.name}");
                return;
            }

            AudioMgr.Inst.Play2DAudio(m_MusicNmae, m_Volume, m_IsConfig);
        }
    }
}