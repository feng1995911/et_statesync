using UnityEngine;

namespace ET
{
    /// <summary>
    /// 全局的音源
    /// </summary>
    [RequireComponent(typeof(AudioListener))]
    [AddComponentMenu("")]
    public class AudioGlobalListener : MonoBehaviour
    {
        [StaticField]
        private static AudioGlobalListener s_Instance;

        /// <summary>
        /// 实例
        /// </summary>
        public static AudioGlobalListener Instance { get => s_Instance; }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <returns></returns>
        public static AudioGlobalListener CreateInstance()
        {
            if (s_Instance == null)
            {
                GameObject go = new GameObject();
                go.name    = $"[::{nameof(AudioGlobalListener)}]";
                s_Instance = go.AddComponent<AudioGlobalListener>();
            }

            return s_Instance;
        }

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                GameObject.Destroy(gameObject);
                return;
            }

            GameObject.DontDestroyOnLoad(gameObject);
            s_Instance = this;
        }

        private void OnDestroy()
        {
            GameObject.Destroy(gameObject);

            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        private Transform m_FollowTarget;

        /// <summary>
        /// 设置音源跟随对象
        /// </summary>
        /// <param name="target"></param>
        public void SetFollow(Transform target)
        {
            m_FollowTarget = target;
        }

        /// <summary>
        /// 设置坐标
        /// </summary>
        /// <param name="worldPosition"></param>
        public void SetPostion(Vector3 worldPosition)
        {
            if (m_FollowTarget != null)
                return;

            transform.position = worldPosition;
        }

        void LateUpdate()
        {
            if (m_FollowTarget != null)
            {
                transform.position = m_FollowTarget.position;
            }
        }
    }
}