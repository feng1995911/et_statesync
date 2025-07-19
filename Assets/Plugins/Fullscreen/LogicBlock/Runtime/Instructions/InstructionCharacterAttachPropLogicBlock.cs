using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace Fullscreen.LogicBlock.Runtime
{
    [Version(0, 0, 1)]

    [Title("Attach Prop")]
    [Description("Attaches a prefab or instance Prop onto a Character's bone")]

    [Category("Characters/Visuals/Attach Prop (LogicBlock)")]

    [Parameter("Character", "The character target")]
    [Parameter("Type", "Whether to attach the prop as a prefab or instance")]
    [Parameter("Prop", "The prefab or instance object that is attached to the character")]
    [Parameter("Handle", "The Handle reference that contains bone and offset info")]

    [Keywords("Characters", "Add", "Grab", "Draw", "Pull", "Take", "Object")]
    [Image(typeof(IconTennis), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionCharacterAttachPropLogicBlock : Instruction
    {
        private enum Type
        {
            Prefab,
            Instance
        }

        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private Type m_Type = Type.Prefab;
        [SerializeField] private PropertyGetGameObject m_Prop = new PropertyGetGameObject();
        [SerializeField] private PropertyGetHandle m_Handle = new PropertyGetHandle();

        public override string Title =>
            $"Attach {this.m_Type} {this.m_Prop} on {this.m_Character} {this.m_Handle}";

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            GameObject prop = this.m_Prop.Get(args);
            if (prop == null) return DefaultResult;

            Handle handleAsset = this.m_Handle.Get(args);
            if (handleAsset == null) return DefaultResult;

            HandleResult handle = handleAsset.Get(new Args(character.gameObject));

            switch (this.m_Type)
            {
                case Type.Prefab:
                    character.Props.AttachPrefab(
                        handle.Bone, prop,
                        handle.LocalPosition, handle.LocalRotation
                    );
                    break;

                case Type.Instance:
                    character.Props.AttachInstance(
                        handle.Bone, prop,
                        handle.LocalPosition, handle.LocalRotation
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return DefaultResult;
        }
    }
}
