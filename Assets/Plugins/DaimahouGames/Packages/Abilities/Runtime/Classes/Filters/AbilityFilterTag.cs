using System;
using DaimahouGames.Runtime.Core.Common;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace DaimahouGames.Runtime.Abilities
{
    [Category("Tag")]
    [Image(typeof(IconTag), ColorTheme.Type.Red)]
    
    [Description("Filter based on tags")]
    
    [Serializable]
    public class AbilityFilterTag : AbilityFilter
    {    
        [SerializeField] private string tag;
        [FormerlySerializedAs("include")] [SerializeField] private bool exclude = true;
        
        protected override string Summary => $"{(exclude ? "exclude" : "include")} [{tag}] tag";
        protected override bool Filter_Internal(ExtendedArgs args)
        {
            if (args.Target == null) return true;
            
            return exclude ? args.Target.CompareTag(tag) : !args.Target.CompareTag(tag);
        }
    }
}