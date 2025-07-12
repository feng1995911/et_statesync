using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;

namespace Fullscreen.LogicBlock.Runtime
{
    [Serializable]
    public class PropertyGetHandle : TPropertyGet<PropertyTypeGetHandle, Handle>
    {
        public PropertyGetHandle() : base(new GetHandleInstance())
        { }

        public PropertyGetHandle(PropertyTypeGetHandle defaultType) : base(defaultType)
        { }

        public HandleResult GetResult(Args args)
        {
            Handle handle = this.Get(args);
            return handle != null ? handle.Get(args) : default;
        }
    }
}
