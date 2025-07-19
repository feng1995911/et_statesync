using UnityEngine;
using System;


namespace Fullscreen.LogicBlock.Runtime
{
    public interface IReference
    {
        string Name { get; }
    }

    [Serializable] public class GameObjectReference : IReference { [SerializeField] public string m_Name = "Reference"; [SerializeField] public GameObject m_GameObject; public string Name => m_Name; }
    [Serializable] public class StringReference : IReference { [SerializeField] public string m_Name = "Reference"; [SerializeField] public string m_Value; public string Name => m_Name; }
    [Serializable] public class NumberReference : IReference { [SerializeField] public string m_Name = "Reference"; [SerializeField] public float m_Value; public string Name => m_Name; }
    [Serializable] public class BooleanReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class ColorReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class Vector3Reference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class AnimationClipReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class AudioClipReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class MaterialReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class SpriteReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class TextureReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class HandleReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class SceneReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class ScaleReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class RotationReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class DirectionReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }

    [Serializable] public class ItemReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class RuntimeItemReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class LootTableReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }

    [Serializable] public class QuestReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }

    [Serializable] public class ShooterWeaponReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }

    [Serializable] public class MeleeWeaponReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class ShieldReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class SkillReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }

    [Serializable] public class AttributeReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class FormulaReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class StatReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
    [Serializable] public class StatusEffectReference : IReference { [SerializeField] public string m_Name = "Reference"; public string Name => m_Name; }
}
