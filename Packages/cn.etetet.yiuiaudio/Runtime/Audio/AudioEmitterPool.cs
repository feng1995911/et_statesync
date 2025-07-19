using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    internal class AudioEmitterPool : IDisposable
    {
        /// <summary>
        /// 空闲的声音发射器
        /// </summary>
        private Queue<AudioEmitter> m_Idles = new Queue<AudioEmitter>();

        /// <summary>
        /// 池子创建的对象
        /// </summary>
        private HashSet<AudioEmitter> m_PoolGens = new HashSet<AudioEmitter>();

        /// <summary>
        /// 空闲发射器根节点
        /// </summary>
        Transform m_IdleRootTransform = null;

        /// <summary>
        /// 保留最大个数
        /// </summary>
        public int KeepMaxCount = 10;

        /// <summary>
        /// 声音发射器池子
        /// </summary>
        /// <param name="idleRoot"></param>
        /// <param name="keepMaxCount"></param>
        public AudioEmitterPool(Transform idleRoot, int keepMaxCount)
        {
            m_IdleRootTransform = idleRoot;
            KeepMaxCount        = keepMaxCount;
        }

        /// <summary>
        /// 获取一个音源
        /// </summary>
        /// <param name="emitterName"></param>
        /// <returns></returns>
        internal AudioEmitter Fetch(string emitterName)
        {
            AudioEmitter result;
            while (m_Idles.Count > 0)
            {
                result = m_Idles.Dequeue();
                if (result != null)
                {
                    result.SetEmitterName(emitterName);
                    return result;
                }
            }

            result = new AudioEmitter();

            result.Init();
            result.SetEmitterName(emitterName);

            m_PoolGens.Add(result);

            return result;
        }

        /// <summary>
        /// 回收一个音源
        /// </summary>
        /// <param name="emitter"></param>
        internal void Recycle(AudioEmitter emitter)
        {
            if (emitter == null)
                return;

            if (!m_Disposed)
            {
                if (m_PoolGens.Contains(emitter))
                {
                    if (m_Idles.Count < KeepMaxCount)
                    {
                        emitter.SetEmitterName("idle_audio_source");
                        emitter.Reset(m_IdleRootTransform);

                        m_Idles.Enqueue(emitter);
                        return;
                    }

                    m_PoolGens.Remove(emitter);
                }
            }

            emitter.Dispose();
        }

        /// <summary>
        /// 是否已释放
        /// </summary>
        private bool m_Disposed = false;

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Disposed = true;

            m_IdleRootTransform = null;
            KeepMaxCount        = 0;

            m_PoolGens.Clear();

            foreach (var item in m_Idles)
            {
                item.Dispose();
            }

            m_Idles.Clear();
        }
    }
}