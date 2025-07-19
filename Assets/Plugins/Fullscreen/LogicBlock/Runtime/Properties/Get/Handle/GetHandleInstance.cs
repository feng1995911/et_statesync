using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Handle Asset")]
    [Category("Handles/Handle Asset")]
    
    [Image(typeof(IconHandle), ColorTheme.Type.Yellow)]
    [Description("A direct reference to a Handle asset")]

    [Serializable] [HideLabelsInEditor]
    public class GetHandleInstance : PropertyTypeGetHandle
    {
        [SerializeField] private Handle m_Handle;

        public override Handle Get(Args args) => this.m_Handle;
        public override Handle Get(GameObject gameObject) => this.m_Handle;

        public GetHandleInstance() : base() { }

        public GetHandleInstance(Handle handle) : this()
        {
            this.m_Handle = handle;
        }

        public static PropertyGetHandle Create()
        {
            return new PropertyGetHandle(new GetHandleInstance());
        }

        public static PropertyGetHandle Create(Handle handle)
        {
            return new PropertyGetHandle(new GetHandleInstance(handle));
        }

        public override string String => this.m_Handle != null 
            ? this.m_Handle.name 
            : "(none)";

        public override Handle EditorValue => this.m_Handle;
    }
}
