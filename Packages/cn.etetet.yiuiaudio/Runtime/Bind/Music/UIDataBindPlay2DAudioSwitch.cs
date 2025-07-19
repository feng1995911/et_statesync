using ET;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YIUIFramework
{
    /// <summary>
    /// 播放2D音乐
    /// 由一个bool 控制开关
    /// 其他时候由UI点击 直接触发
    /// 音效名称直接配置 方便随时改变 不由代码切换
    /// </summary>
    [LabelText("Play 音效2D(开关)")]
    [AddComponentMenu("YIUIBind/Data/音效2DBind 【Play2DAudioSwitch】 AudioUIDataBindPlay2DAudioSwitch")]
    public sealed class UIDataBindPlay2DAudioSwitch : UIDataBindSelectBase, IPointerClickHandler
    {
        [SerializeField]
        [LabelText("2D音效名称")]
        private string m_MusicNmae;

        [SerializeField]
        [Range(0f, 1f)]
        [LabelText("音量")]
        private float m_Volume = 1;

        [SerializeField]
        [ReadOnly]
        [LabelText("开关")]
        private bool m_CanPlay;

        [SerializeField]
        [LabelText("使用配置")]
        private bool m_IsConfig;

        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Bool;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        protected override void OnValueChanged()
        {
            m_CanPlay = GetFirstValue<bool>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!m_CanPlay) return;
            if (string.IsNullOrEmpty(m_MusicNmae))
            {
                Debug.LogError($"未配置点击音效请检查 {this.gameObject.name}");
                return;
            }

            AudioMgr.Inst.Play2DAudio(m_MusicNmae, m_Volume, m_IsConfig);
        }
    }
}