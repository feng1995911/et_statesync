using ET;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YIUIFramework
{
    /// <summary>
    /// 点击就 播放2D音乐
    /// 由UI点击 直接触发
    /// 没有控制开关
    /// </summary>
    [LabelText("Play 音效2D")]
    [AddComponentMenu("YIUIBind/Data/音效2D播放 【Play2DAudio】 AudioUIDataBindPlay2DAudio")]
    public sealed class UIDataBindPlay2DAudio : MonoBehaviour, IPointerClickHandler
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

        public void OnPointerClick(PointerEventData eventData)
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