using System;
using DaimahouGames.Runtime.Core.Common;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace DaimahouGames.Runtime.Abilities
{
    [Category("Layer")]
    [Image(typeof(IconLayers), ColorTheme.Type.Red)]
    
    [Description("Filter based on layers")]
    
    [Serializable]
    public class AbilityFilterLayer : AbilityFilter
    {    
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private bool invert = true;
        
        protected override string Summary => $"{(invert ? "exclude" : "include")} layers [{LayerMaskToString(layerMask)}]";

        protected override bool Filter_Internal(ExtendedArgs args)
        {
            if (args.Target == null) return true;

            int targetLayer = args.Target.gameObject.layer;
            bool isInLayerMask = (layerMask.value & (1 << targetLayer)) != 0;
            
            return invert ? isInLayerMask : !isInLayerMask;
        }

        // Helper method to convert LayerMask to readable string
        static string LayerMaskToString(LayerMask mask)
        {
            int maskValue = mask.value;
            string[] layers = new string[32];
            int count = 0;
            
            for (int i = 0; i < 32; i++)
            {
                if ((maskValue & (1 << i)) != 0)
                {
                    layers[count++] = LayerMask.LayerToName(i);
                }
            }
            
            return string.Join(", ", layers, 0, count);
        }
    }
}