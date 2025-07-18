namespace ET
{
    public static class YIUIConditionHelper
    {
        public static ConditionMgr YIUICondition(this Entity entity)
        {
            return entity.Root().GetComponent<ConditionMgr>();
        }

        public static ConditionMgr YIUICondition(this Scene scene)
        {
            return scene.Root().GetComponent<ConditionMgr>();
        }
    }
}