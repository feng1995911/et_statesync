using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;

namespace Fullscreen.LogicBlock.Runtime
{
    [Serializable]
    public abstract class PropertyTypeGetHandle : TPropertyTypeGet<Handle>
    {
        public override Handle Get(Args args) => default;
        public override Handle Get(GameObject gameObject) => default;
    }
}
