using UnityEngine;
using System;

namespace Fullscreen.LogicBlock.Runtime
{
    [Serializable]
    public class ReferenceWrapper
    {
        public enum ReferenceType
        {
            GameObject, String, Number, Boolean, Color, Vector3,
            AnimationClip, AudioClip, Material, Sprite, Texture,
            Handle, Scene, Scale, Rotation, Direction,
            Item, RuntimeItem, LootTable, Quest,
            ShooterWeapon, MeleeWeapon, Shield, Skill,
            Attribute, Formula, Stat, StatusEffect
        }

        [SerializeField] private ReferenceType m_Type;

        [SerializeField] private GameObjectReference m_GameObjectRef;
        [SerializeField] private StringReference m_StringRef;
        [SerializeField] private NumberReference m_NumberRef;
        [SerializeField] private BooleanReference m_BooleanRef;
        [SerializeField] private ColorReference m_ColorRef;
        [SerializeField] private Vector3Reference m_Vector3Ref;
        [SerializeField] private AnimationClipReference m_AnimClipRef;
        [SerializeField] private AudioClipReference m_AudioClipRef;
        [SerializeField] private MaterialReference m_MaterialRef;
        [SerializeField] private SpriteReference m_SpriteRef;
        [SerializeField] private TextureReference m_TextureRef;
        [SerializeField] private HandleReference m_HandleRef;
        [SerializeField] private SceneReference m_SceneRef;
        [SerializeField] private ScaleReference m_ScaleRef;
        [SerializeField] private RotationReference m_RotationRef;
        [SerializeField] private DirectionReference m_DirectionRef;

        [SerializeField] private ItemReference m_ItemRef;
        [SerializeField] private RuntimeItemReference m_RuntimeItemRef;
        [SerializeField] private LootTableReference m_LootTableRef;

        [SerializeField] private QuestReference m_QuestRef;
    
        [SerializeField] private ShooterWeaponReference m_ShooterWeaponRef;

        [SerializeField] private MeleeWeaponReference m_MeleeWeaponRef;
        [SerializeField] private ShieldReference m_ShieldRef;
        [SerializeField] private SkillReference m_SkillRef;

        [SerializeField] private AttributeReference m_AttributeRef;
        [SerializeField] private FormulaReference m_FormulaRef;
        [SerializeField] private StatReference m_StatRef;
        [SerializeField] private StatusEffectReference m_StatusEffectRef;

        public ReferenceType Type => m_Type;

        public string Name => GetReference()?.Name ?? string.Empty;

        public IReference GetReference()
        {
            return m_Type switch
            {
                ReferenceType.GameObject => m_GameObjectRef,
                ReferenceType.String => m_StringRef,
                ReferenceType.Number => m_NumberRef,
                ReferenceType.Boolean => m_BooleanRef,
                ReferenceType.Color => m_ColorRef,
                ReferenceType.Vector3 => m_Vector3Ref,
                ReferenceType.AnimationClip => m_AnimClipRef,
                ReferenceType.AudioClip => m_AudioClipRef,
                ReferenceType.Material => m_MaterialRef,
                ReferenceType.Sprite => m_SpriteRef,
                ReferenceType.Texture => m_TextureRef,

                ReferenceType.Handle => m_HandleRef,
                ReferenceType.Scene => m_SceneRef,
                ReferenceType.Scale => m_ScaleRef,
                ReferenceType.Rotation => m_RotationRef,
                ReferenceType.Direction => m_DirectionRef,

                ReferenceType.Item => m_ItemRef,
                ReferenceType.RuntimeItem => m_RuntimeItemRef,
                ReferenceType.LootTable => m_LootTableRef,

                ReferenceType.Quest => m_QuestRef,

                ReferenceType.ShooterWeapon => m_ShooterWeaponRef,
                ReferenceType.MeleeWeapon => m_MeleeWeaponRef,
                ReferenceType.Shield => m_ShieldRef,
                ReferenceType.Skill => m_SkillRef,

                ReferenceType.Attribute => m_AttributeRef,
                ReferenceType.Formula => m_FormulaRef,
                ReferenceType.Stat => m_StatRef,
                ReferenceType.StatusEffect => m_StatusEffectRef,

                _ => null
            };
        }
    }
}
