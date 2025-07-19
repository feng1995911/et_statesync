using System;
using DaimahouGames.Runtime.Core.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace DaimahouGames.Runtime.Abilities
{
    [Category("Custom")]
    [Image(typeof(IconCondition), ColorTheme.Type.Red)]
    
    [Description("Remove the caster from valid targets.")]
    
    [Serializable]
    public class AbilityFilterCustom : AbilityFilter
    {
        //============================================================================================================||
        // ※  Variables: --------------------------------------------------------------------------------------------|
        // ---| Exposed State -------------------------------------------------------------------------------------->|
        
        [SerializeField] private string m_Description;
        [SerializeField] private ConditionList m_Conditions;
        
        // ---| Internal State ------------------------------------------------------------------------------------->|
        // ---| Dependencies --------------------------------------------------------------------------------------->|
        // ---| Properties ----------------------------------------------------------------------------------------->|

        protected override string Summary => string.Format("{0}", 
            string.IsNullOrEmpty(m_Description) ? "Generic conditions" : m_Description
        );

        protected override bool Filter_Internal(ExtendedArgs args)
        {
            return m_Conditions.Check(args, CheckMode.And);
        }
        
        // ※  Virtual Methods: --------------------------------------------------------------------------------------|
        // ※  Private Methods: --------------------------------------------------------------------------------------|
        //============================================================================================================||
    }
}