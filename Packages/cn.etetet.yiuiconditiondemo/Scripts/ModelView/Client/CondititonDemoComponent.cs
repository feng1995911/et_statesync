using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Desc    条件测试
    /// </summary>
    [ComponentOf]
    public class ConditionDemoComponent : Entity, IAwake, IDestroy
    {
        public int  DemoValue { get; set; }
        public long m_TimerId;
        public long m_ListenerId;
    }
}