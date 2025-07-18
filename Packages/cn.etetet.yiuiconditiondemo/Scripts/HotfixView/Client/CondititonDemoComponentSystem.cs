using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Desc    条件测试
    /// </summary>
    [FriendOf(typeof(ConditionDemoComponent))]
    [EntitySystemOf(typeof(ConditionDemoComponent))]
    public static partial class ConditionDemoComponentSystem
    {
        #region ObjectSystem

        [EntitySystem]
        private static void Awake(this ConditionDemoComponent self)
        {
            self.YIUICondition().AddCheckConditionGroupListener(ref self.m_ListenerId, self, $"{nameof(ConditionDemoComponent)}.{nameof(ConditionResult)}", EConditionGroupId.DemoGroup1);
            self.m_TimerId = self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(2000, ConditionDemoTimerInvokeType.ConditionDemoTimerInvoke, self);
        }

        [YIUIInvoke]
        private static void ConditionResult(this ConditionDemoComponent self, long instanceId, bool arg1, string arg2)
        {
            Log.Error($"条件判断: 结果:{arg1}  失败原因:{arg2}");
        }

        [EntitySystem]
        private static void Destroy(this ConditionDemoComponent self)
        {
            self.YIUICondition().RemoveCheckConditionListener(ref self.m_ListenerId);
            self?.Root()?.GetComponent<TimerComponent>()?.Remove(ref self.m_TimerId);
        }

        [Invoke(ConditionDemoTimerInvokeType.ConditionDemoTimerInvoke)]
        public class TimerInvoke_ConditionDemo : ATimer<ConditionDemoComponent>
        {
            protected override void Run(ConditionDemoComponent self)
            {
                self.DemoValue = (self.DemoValue + 1) % 3;
                Log.Info($"条件demo改变测试:  {self.DemoValue}");
                self.YIUICondition().TriggerListener(EConditionType.Demo);
            }
        }

        #endregion
    }
}