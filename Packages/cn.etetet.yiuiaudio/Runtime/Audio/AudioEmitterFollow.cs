using UnityEngine;

namespace ET
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("")]
    public class AudioEmitterFollow : MonoBehaviour
    {
        /// <summary>
        /// 跟随对象
        /// </summary>
        [SerializeField]
        private Transform m_FollowTarget;

        /// <summary>
        /// 跟随偏移
        /// </summary>
        [SerializeField]
        private Vector3 m_OffsetPos = Vector3.zero;

        /// <summary>
        /// 是否自动回收
        /// </summary>
        private bool m_IsAutoRelease = false;

        /// <summary>
        /// 是否工作中
        /// </summary>
        private bool m_IsWorking = false;

        /// <summary>
        /// 音效Key
        /// </summary>
        private int m_AudioKey = 0;

        /// <summary>
        /// 所属管理器
        /// </summary>
        private AudioEmitterController ownController = null;

        /// <summary>
        /// 设置跟随信息
        /// </summary>
        /// <param name="audioKey"></param>
        /// <param name="targetTrans">目标</param>
        /// <param name="offsetPos">位置偏移</param>
        /// <param name="isAutoRelease">目标丢失时是否自动释放</param>
        /// <param name="isWorking">是否工作</param>
        internal void SetFollow(AudioEmitterController ownController, int audioKey, Transform targetTrans, Vector3 offsetPos, bool isAutoRelease = true, bool isWorking = false)
        {
            if (this.m_IsWorking)
            {
                Debug.LogError($"当前声音跟随脚本正在播放音乐：【{this.m_AudioKey}】中，不能执行方法：[{nameof(SetFollow)}]");
                return;
            }

            this.ownController = ownController;
            this.m_AudioKey    = audioKey;
            this.ChangeTarget(targetTrans, offsetPos);

            this.m_IsAutoRelease = isAutoRelease;
            this.m_IsWorking     = isWorking;
        }

        internal void ChangeTarget(Transform targetTrans, Vector3 offsetPos)
        {
            this.m_FollowTarget = targetTrans;
            this.m_OffsetPos    = offsetPos;

            if (targetTrans != null)
            {
                this.transform.position = m_FollowTarget.position + offsetPos;
            }
            else
            {
                this.transform.position = m_OffsetPos;
            }
        }

        /// <summary>
        /// 重置脚本
        /// </summary>
        internal void ResetComponent()
        {
            this.m_IsWorking     = false;
            this.m_FollowTarget  = null;
            this.m_AudioKey      = 0;
            this.m_OffsetPos     = Vector3.zero;
            this.m_IsAutoRelease = false;
            this.ownController   = null;

            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
        }

        private void Update()
        {
            if (!this.m_IsWorking)
                return;

            if (this.m_FollowTarget != null)
            {
                this.transform.position = m_FollowTarget.position + m_OffsetPos;
            }
            else
            {
                this.m_IsWorking = false;
                if (this.m_IsAutoRelease && this.m_AudioKey > 0)
                {
                    this.ownController?.StopAudio(this.m_AudioKey);
                }

                this.m_AudioKey = 0;
            }
        }
    }
}