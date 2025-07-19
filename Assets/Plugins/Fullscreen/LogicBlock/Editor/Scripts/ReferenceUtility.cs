using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Fullscreen.LogicBlock.Runtime;
using GameCreator.Runtime.Common;
//<Inventory>
using GameCreator.Runtime.Inventory;
//</Inventory>
//<Melee>
using GameCreator.Runtime.Melee;
//</Melee>
//<Shooter>
using GameCreator.Runtime.Shooter;
//</Shooter>
//<Quests>
using GameCreator.Runtime.Quests;
//</Quests>
//<Stats>
using GameCreator.Runtime.Stats;
//</Stats>

namespace Fullscreen.LogicBlock.Editor
{
    public static class ReferenceUtility
    {
        private static readonly Dictionary<ReferenceWrapper.ReferenceType, string> ReferencePropertyNames = new()
        {
            { ReferenceWrapper.ReferenceType.GameObject, "m_GameObjectRef" },
            { ReferenceWrapper.ReferenceType.String, "m_StringRef" },
            { ReferenceWrapper.ReferenceType.Number, "m_NumberRef" },
            { ReferenceWrapper.ReferenceType.Boolean, "m_BooleanRef" },
            { ReferenceWrapper.ReferenceType.Color, "m_ColorRef" },
            { ReferenceWrapper.ReferenceType.Vector3, "m_Vector3Ref" },
            { ReferenceWrapper.ReferenceType.AnimationClip, "m_AnimClipRef" },
            { ReferenceWrapper.ReferenceType.AudioClip, "m_AudioClipRef" },
            { ReferenceWrapper.ReferenceType.Material, "m_MaterialRef" },
            { ReferenceWrapper.ReferenceType.Sprite, "m_SpriteRef" },
            { ReferenceWrapper.ReferenceType.Texture, "m_TextureRef" },
            { ReferenceWrapper.ReferenceType.Handle, "m_HandleRef" },
            { ReferenceWrapper.ReferenceType.Scene, "m_SceneRef" },
            { ReferenceWrapper.ReferenceType.Scale, "m_ScaleRef" },
            { ReferenceWrapper.ReferenceType.Rotation, "m_RotationRef" },
            { ReferenceWrapper.ReferenceType.Direction, "m_DirectionRef" },
            { ReferenceWrapper.ReferenceType.Item, "m_ItemRef" },
            { ReferenceWrapper.ReferenceType.RuntimeItem, "m_RuntimeItemRef" },
            { ReferenceWrapper.ReferenceType.LootTable, "m_LootTableRef" },
            { ReferenceWrapper.ReferenceType.Quest, "m_QuestRef" },
            { ReferenceWrapper.ReferenceType.ShooterWeapon, "m_ShooterWeaponRef" },
            { ReferenceWrapper.ReferenceType.MeleeWeapon, "m_MeleeWeaponRef" },
            { ReferenceWrapper.ReferenceType.Shield, "m_ShieldRef" },
            { ReferenceWrapper.ReferenceType.Skill, "m_SkillRef" },
            { ReferenceWrapper.ReferenceType.Attribute, "m_AttributeRef" },
            { ReferenceWrapper.ReferenceType.Formula, "m_FormulaRef" },
            { ReferenceWrapper.ReferenceType.Stat, "m_StatRef" },
            { ReferenceWrapper.ReferenceType.StatusEffect, "m_StatusEffectRef" }
        };

        private static readonly Dictionary<ReferenceWrapper.ReferenceType, Type> ReferenceTypes = new()
        {
            { ReferenceWrapper.ReferenceType.GameObject, typeof(GameObjectReference) },
            { ReferenceWrapper.ReferenceType.String, typeof(StringReference) },
            { ReferenceWrapper.ReferenceType.Number, typeof(NumberReference) },
            { ReferenceWrapper.ReferenceType.Boolean, typeof(BooleanReference) },
            { ReferenceWrapper.ReferenceType.Color, typeof(ColorReference) },
            { ReferenceWrapper.ReferenceType.Vector3, typeof(Vector3Reference) },
            { ReferenceWrapper.ReferenceType.AnimationClip, typeof(AnimationClipReference) },
            { ReferenceWrapper.ReferenceType.AudioClip, typeof(AudioClipReference) },
            { ReferenceWrapper.ReferenceType.Material, typeof(MaterialReference) },
            { ReferenceWrapper.ReferenceType.Sprite, typeof(SpriteReference) },
            { ReferenceWrapper.ReferenceType.Texture, typeof(TextureReference) },
            { ReferenceWrapper.ReferenceType.Handle, typeof(HandleReference) },
            { ReferenceWrapper.ReferenceType.Scene, typeof(Fullscreen.LogicBlock.Runtime.SceneReference) },
            { ReferenceWrapper.ReferenceType.Scale, typeof(ScaleReference) },
            { ReferenceWrapper.ReferenceType.Rotation, typeof(RotationReference) },
            { ReferenceWrapper.ReferenceType.Direction, typeof(DirectionReference) },
            { ReferenceWrapper.ReferenceType.Item, typeof(ItemReference) },
            { ReferenceWrapper.ReferenceType.RuntimeItem, typeof(RuntimeItemReference) },
            { ReferenceWrapper.ReferenceType.LootTable, typeof(LootTableReference) },
            { ReferenceWrapper.ReferenceType.Quest, typeof(QuestReference) },
            { ReferenceWrapper.ReferenceType.ShooterWeapon, typeof(ShooterWeaponReference) },
            { ReferenceWrapper.ReferenceType.MeleeWeapon, typeof(MeleeWeaponReference) },
            { ReferenceWrapper.ReferenceType.Shield, typeof(ShieldReference) },
            { ReferenceWrapper.ReferenceType.Skill, typeof(SkillReference) },
            { ReferenceWrapper.ReferenceType.Attribute, typeof(AttributeReference) },
            { ReferenceWrapper.ReferenceType.Formula, typeof(FormulaReference) },
            { ReferenceWrapper.ReferenceType.Stat, typeof(StatReference) },
            { ReferenceWrapper.ReferenceType.StatusEffect, typeof(StatusEffectReference) }
        };

        private static readonly Dictionary<ReferenceWrapper.ReferenceType, string> InstructionReferencePropertyNames = new()
        {
            { ReferenceWrapper.ReferenceType.GameObject, "m_GameObject" },
            { ReferenceWrapper.ReferenceType.String, "m_String" },
            { ReferenceWrapper.ReferenceType.Number, "m_Decimal" },
            { ReferenceWrapper.ReferenceType.Boolean, "m_Bool" },
            { ReferenceWrapper.ReferenceType.Color, "m_Color" },
            { ReferenceWrapper.ReferenceType.Vector3, "m_Position" },
            { ReferenceWrapper.ReferenceType.AnimationClip, "m_Animation" },
            { ReferenceWrapper.ReferenceType.AudioClip, "m_Audio" },
            { ReferenceWrapper.ReferenceType.Material, "m_Material" },
            { ReferenceWrapper.ReferenceType.Sprite, "m_Sprite" },
            { ReferenceWrapper.ReferenceType.Texture, "m_Texture" },
            { ReferenceWrapper.ReferenceType.Handle, "m_Handle" },
            { ReferenceWrapper.ReferenceType.Scene, "m_Scene" },
            { ReferenceWrapper.ReferenceType.Scale, "m_Scale" },
            { ReferenceWrapper.ReferenceType.Rotation, "m_Rotation" },
            { ReferenceWrapper.ReferenceType.Direction, "m_Direction" },
            { ReferenceWrapper.ReferenceType.Item, "m_Item" },
            { ReferenceWrapper.ReferenceType.RuntimeItem, "m_RuntimeItem" },
            { ReferenceWrapper.ReferenceType.LootTable, "m_LootTable" },
            { ReferenceWrapper.ReferenceType.Quest, "m_Quest" },
            { ReferenceWrapper.ReferenceType.ShooterWeapon, "m_ShooterWeapon" },
            { ReferenceWrapper.ReferenceType.MeleeWeapon, "m_MeleeWeapon" },
            { ReferenceWrapper.ReferenceType.Shield, "m_Shield" },
            { ReferenceWrapper.ReferenceType.Skill, "m_Skill" },
            { ReferenceWrapper.ReferenceType.Attribute, "m_Attribute" },
            { ReferenceWrapper.ReferenceType.Formula, "m_Formula" },
            { ReferenceWrapper.ReferenceType.Stat, "m_Stat" },
            { ReferenceWrapper.ReferenceType.StatusEffect, "m_StatusEffect" }
        };

        private static readonly Dictionary<ReferenceWrapper.ReferenceType, Type> InstructionReferenceTypes = new()
        {
            { ReferenceWrapper.ReferenceType.GameObject, typeof(PropertyGetGameObject) },
            { ReferenceWrapper.ReferenceType.String, typeof(PropertyGetString) },
            { ReferenceWrapper.ReferenceType.Number, typeof(PropertyGetDecimal) },
            { ReferenceWrapper.ReferenceType.Boolean, typeof(PropertyGetBool) },
            { ReferenceWrapper.ReferenceType.Color, typeof(PropertyGetColor) },
            { ReferenceWrapper.ReferenceType.Vector3, typeof(PropertyGetPosition) },
            { ReferenceWrapper.ReferenceType.AnimationClip, typeof(PropertyGetAnimation) },
            { ReferenceWrapper.ReferenceType.AudioClip, typeof(PropertyGetAudio) },
            { ReferenceWrapper.ReferenceType.Material, typeof(PropertyGetMaterial) },
            { ReferenceWrapper.ReferenceType.Sprite, typeof(PropertyGetSprite) },
            { ReferenceWrapper.ReferenceType.Texture, typeof(PropertyGetTexture) },
            { ReferenceWrapper.ReferenceType.Handle, typeof(PropertyGetHandle) },
            { ReferenceWrapper.ReferenceType.Scene, typeof(PropertyGetScene) },
            { ReferenceWrapper.ReferenceType.Scale, typeof(PropertyGetScale) },
            { ReferenceWrapper.ReferenceType.Rotation, typeof(PropertyGetRotation) },
            //<Inventory>
{ ReferenceWrapper.ReferenceType.Item, typeof(PropertyGetItem) },
{ ReferenceWrapper.ReferenceType.RuntimeItem, typeof(PropertyGetRuntimeItem) },
{ ReferenceWrapper.ReferenceType.LootTable, typeof(PropertyGetLootTable) },
         //</Inventory>
            //<Quests>
         { ReferenceWrapper.ReferenceType.Quest, typeof(PropertyGetQuest) },
         //</Quests>
            //<Shooter>
{ ReferenceWrapper.ReferenceType.ShooterWeapon, typeof(PropertyGetWeapon) },
      //</Shooter>
            //<Melee>
{ ReferenceWrapper.ReferenceType.MeleeWeapon, typeof(PropertyGetWeapon) },
{ ReferenceWrapper.ReferenceType.Shield, typeof(PropertyGetShield) },
{ ReferenceWrapper.ReferenceType.Skill, typeof(PropertyGetSkill) },
        //</Melee>
            //<Stats>
{ ReferenceWrapper.ReferenceType.Attribute, typeof(PropertyGetAttribute) },
{ ReferenceWrapper.ReferenceType.Formula, typeof(PropertyGetFormula) },
{ ReferenceWrapper.ReferenceType.Stat, typeof(PropertyGetStat) },
{ ReferenceWrapper.ReferenceType.StatusEffect, typeof(PropertyGetStatusEffect) },
         //</Stats>
            { ReferenceWrapper.ReferenceType.Direction, typeof(PropertyGetDirection) }
        };

        private static readonly Dictionary<ReferenceWrapper.ReferenceType, Type> ReferenceToIconMap = new()
        {
            { ReferenceWrapper.ReferenceType.GameObject, typeof(IconCubeSolid) },
            { ReferenceWrapper.ReferenceType.String, typeof(IconString) },
            { ReferenceWrapper.ReferenceType.Number, typeof(IconNumber) },
            { ReferenceWrapper.ReferenceType.Boolean, typeof(IconToggleOn) },
            { ReferenceWrapper.ReferenceType.Color, typeof(IconColor) },
            { ReferenceWrapper.ReferenceType.Vector3, typeof(IconVector3) },
            { ReferenceWrapper.ReferenceType.AnimationClip, typeof(IconAnimationClip) },
            { ReferenceWrapper.ReferenceType.AudioClip, typeof(IconAudioClip) },
            { ReferenceWrapper.ReferenceType.Material, typeof(IconMaterial) },
            { ReferenceWrapper.ReferenceType.Sprite, typeof(IconSprite) },
            { ReferenceWrapper.ReferenceType.Texture, typeof(IconTexture) },
            { ReferenceWrapper.ReferenceType.Handle, typeof(IconHandle) },
            { ReferenceWrapper.ReferenceType.Scene, typeof(IconUnity) },
            { ReferenceWrapper.ReferenceType.Scale, typeof(IconScale) },
            { ReferenceWrapper.ReferenceType.Rotation, typeof(IconRotation) },
            { ReferenceWrapper.ReferenceType.Direction, typeof(IconVector3) },
            //<Inventory>
{ ReferenceWrapper.ReferenceType.Item, typeof(IconItem) },
{ ReferenceWrapper.ReferenceType.RuntimeItem, typeof(IconItem) },
{ ReferenceWrapper.ReferenceType.LootTable, typeof(IconLoot) },
         //</Inventory>
            //<Quests>
         { ReferenceWrapper.ReferenceType.Quest, typeof(IconQuestSolid) },
         //</Quests>
            //<Shooter>
{ ReferenceWrapper.ReferenceType.ShooterWeapon, typeof(IconPistol) },
      //</Shooter>
            //<Melee>
{ ReferenceWrapper.ReferenceType.MeleeWeapon, typeof(IconMeleeSword) },
{ ReferenceWrapper.ReferenceType.Shield, typeof(IconShieldSolid) },
{ ReferenceWrapper.ReferenceType.Skill, typeof(IconMeleeSkill) },
        //</Melee>
            //<Stats>
{ ReferenceWrapper.ReferenceType.Attribute, typeof(IconAttr) },
{ ReferenceWrapper.ReferenceType.Formula, typeof(IconFormula) },
{ ReferenceWrapper.ReferenceType.Stat, typeof(IconStat) },
{ ReferenceWrapper.ReferenceType.StatusEffect, typeof(IconStatusEffect) },
         //</Stats>
        };

        public static string GetReferenceName(SerializedProperty referenceProp)
        {
            var typeProp = referenceProp?.FindPropertyRelative("m_Type");
            if (typeProp == null) return null;

            var refType = (ReferenceWrapper.ReferenceType)typeProp.enumValueIndex;
            if (!ReferencePropertyNames.TryGetValue(refType, out var propName)) return null;

            var refProp = referenceProp.FindPropertyRelative(propName);
            if (refProp?.boxedValue == null) return null;

            return refProp.FindPropertyRelative("m_Name")?.stringValue;
        }

        public static IIcon GetIconForReferenceType(ReferenceWrapper.ReferenceType type)
        {
            if (ReferenceToIconMap.TryGetValue(type, out Type iconType))
            {
                try
                {
                    var constructors = iconType.GetConstructors();
                    foreach (var ctor in constructors)
                    {
                        var parameters = ctor.GetParameters();

                        if (parameters.Length >= 1 && parameters[0].ParameterType == typeof(Color))
                        {
                            object[] args = parameters.Length == 2
                                ? new object[] { GetColorForReferenceType(type), null }
                                : new object[] { GetColorForReferenceType(type) };

                            return (IIcon)ctor.Invoke(args);
                        }

                        if (parameters.Length >= 1 && parameters[0].ParameterType == typeof(ColorTheme.Type))
                        {
                            ColorTheme.Type colorType = type switch
                            {
                                ReferenceWrapper.ReferenceType.GameObject => ColorTheme.Type.Blue,
                                ReferenceWrapper.ReferenceType.String => ColorTheme.Type.Yellow,
                                ReferenceWrapper.ReferenceType.Number => ColorTheme.Type.Blue,
                                ReferenceWrapper.ReferenceType.Boolean => ColorTheme.Type.Red,
                                ReferenceWrapper.ReferenceType.Color => ColorTheme.Type.Purple,
                                ReferenceWrapper.ReferenceType.Vector3 => ColorTheme.Type.Green,
                                ReferenceWrapper.ReferenceType.AnimationClip => ColorTheme.Type.Teal,
                                ReferenceWrapper.ReferenceType.AudioClip => ColorTheme.Type.Yellow,
                                ReferenceWrapper.ReferenceType.Material => ColorTheme.Type.Blue,
                                ReferenceWrapper.ReferenceType.Sprite => ColorTheme.Type.Green,
                                ReferenceWrapper.ReferenceType.Texture => ColorTheme.Type.Purple,
                                ReferenceWrapper.ReferenceType.Handle => ColorTheme.Type.Yellow,
                                ReferenceWrapper.ReferenceType.Scene => ColorTheme.Type.TextNormal,
                                ReferenceWrapper.ReferenceType.Scale => ColorTheme.Type.Yellow,
                                ReferenceWrapper.ReferenceType.Rotation => ColorTheme.Type.Yellow,
                                ReferenceWrapper.ReferenceType.Direction => ColorTheme.Type.Green,
                                ReferenceWrapper.ReferenceType.Item => ColorTheme.Type.Green,
                                ReferenceWrapper.ReferenceType.RuntimeItem => ColorTheme.Type.Blue,
                                ReferenceWrapper.ReferenceType.LootTable => ColorTheme.Type.Red,
                                ReferenceWrapper.ReferenceType.Quest => ColorTheme.Type.Yellow,
                                ReferenceWrapper.ReferenceType.ShooterWeapon => ColorTheme.Type.Blue,
                                ReferenceWrapper.ReferenceType.MeleeWeapon => ColorTheme.Type.Blue,
                                ReferenceWrapper.ReferenceType.Shield => ColorTheme.Type.Red,
                                ReferenceWrapper.ReferenceType.Skill => ColorTheme.Type.Green,
                                ReferenceWrapper.ReferenceType.Attribute => ColorTheme.Type.Blue,
                                ReferenceWrapper.ReferenceType.Formula => ColorTheme.Type.Purple,
                                ReferenceWrapper.ReferenceType.Stat => ColorTheme.Type.Red,
                                ReferenceWrapper.ReferenceType.StatusEffect => ColorTheme.Type.Green,
                                _ => ColorTheme.Type.Red
                            };

                            object[] args = parameters.Length == 2
                                ? new object[] { colorType, null }
                                : new object[] { colorType };

                            return (IIcon)ctor.Invoke(args);
                        }
                    }

                    return (IIcon)Activator.CreateInstance(iconType);
                }
                catch (Exception)
                {
                }
            }

            return new IconReference(ColorTheme.Get(ColorTheme.Type.Red));
        }
        
        private static Color GetColorForReferenceType(ReferenceWrapper.ReferenceType type)
        {
            return ColorTheme.Get(type switch
            {
                ReferenceWrapper.ReferenceType.GameObject => ColorTheme.Type.Blue,
                ReferenceWrapper.ReferenceType.String => ColorTheme.Type.Yellow,
                ReferenceWrapper.ReferenceType.Number => ColorTheme.Type.Blue,
                ReferenceWrapper.ReferenceType.Boolean => ColorTheme.Type.Red,
                ReferenceWrapper.ReferenceType.Color => ColorTheme.Type.Purple,
                ReferenceWrapper.ReferenceType.Vector3 => ColorTheme.Type.Green,
                ReferenceWrapper.ReferenceType.AnimationClip => ColorTheme.Type.Teal,
                ReferenceWrapper.ReferenceType.AudioClip => ColorTheme.Type.Yellow,
                ReferenceWrapper.ReferenceType.Material => ColorTheme.Type.Blue,
                ReferenceWrapper.ReferenceType.Sprite => ColorTheme.Type.Green,
                ReferenceWrapper.ReferenceType.Texture => ColorTheme.Type.Purple,
                ReferenceWrapper.ReferenceType.Handle => ColorTheme.Type.Yellow,
                ReferenceWrapper.ReferenceType.Scene => ColorTheme.Type.TextNormal,
                ReferenceWrapper.ReferenceType.Scale => ColorTheme.Type.Yellow,
                ReferenceWrapper.ReferenceType.Rotation => ColorTheme.Type.Yellow,
                ReferenceWrapper.ReferenceType.Direction => ColorTheme.Type.Green,
                ReferenceWrapper.ReferenceType.Item => ColorTheme.Type.Green,
                ReferenceWrapper.ReferenceType.RuntimeItem => ColorTheme.Type.Blue,
                ReferenceWrapper.ReferenceType.LootTable => ColorTheme.Type.Red,
                ReferenceWrapper.ReferenceType.Quest => ColorTheme.Type.Yellow,
                ReferenceWrapper.ReferenceType.ShooterWeapon => ColorTheme.Type.Blue,
                ReferenceWrapper.ReferenceType.MeleeWeapon => ColorTheme.Type.Blue,
                ReferenceWrapper.ReferenceType.Shield => ColorTheme.Type.Red,
                ReferenceWrapper.ReferenceType.Skill => ColorTheme.Type.Green,
                ReferenceWrapper.ReferenceType.Attribute => ColorTheme.Type.Blue,
                ReferenceWrapper.ReferenceType.Formula => ColorTheme.Type.Purple,
                ReferenceWrapper.ReferenceType.Stat => ColorTheme.Type.Red,
                ReferenceWrapper.ReferenceType.StatusEffect => ColorTheme.Type.Green,
                _ => ColorTheme.Type.Red
            });
        }

        public static void InitializeReference(SerializedProperty referenceProp, ReferenceWrapper.ReferenceType refType, string refName)
        {
            if (!ReferencePropertyNames.TryGetValue(refType, out var propName)) return;

            var refProp = referenceProp.FindPropertyRelative(propName);
            if (refProp == null) return;

            refProp.boxedValue = CreateReference(refType, refName);
        }

        public static object CreateReference(ReferenceWrapper.ReferenceType refType, string refName)
        {
            switch (refType)
            {
                case ReferenceWrapper.ReferenceType.GameObject: return new GameObjectReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.String: return new StringReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Number: return new NumberReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Boolean: return new BooleanReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Color: return new ColorReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Vector3: return new Vector3Reference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.AnimationClip: return new AnimationClipReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.AudioClip: return new AudioClipReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Material: return new MaterialReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Sprite: return new SpriteReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Texture: return new TextureReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Handle: return new HandleReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Scene: return new Fullscreen.LogicBlock.Runtime.SceneReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Scale: return new ScaleReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Rotation: return new RotationReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Direction: return new DirectionReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Item: return new ItemReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.RuntimeItem: return new RuntimeItemReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.LootTable: return new LootTableReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Quest: return new QuestReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.ShooterWeapon: return new ShooterWeaponReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.MeleeWeapon: return new MeleeWeaponReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Shield: return new ShieldReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Skill: return new SkillReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Attribute: return new AttributeReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Formula: return new FormulaReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.Stat: return new StatReference { m_Name = refName };
                case ReferenceWrapper.ReferenceType.StatusEffect: return new StatusEffectReference { m_Name = refName };
                default: throw new ArgumentException($"Unknown reference type: {refType}");
            }
        }

        public static SerializedProperty GetActiveReferenceProperty(SerializedProperty referenceProp)
        {
            var typeProp = referenceProp.FindPropertyRelative("m_Type");
            if (typeProp == null) return null;

            var refType = (ReferenceWrapper.ReferenceType)typeProp.enumValueIndex;
            return ReferencePropertyNames.TryGetValue(refType, out var propName)
                ? referenceProp.FindPropertyRelative(propName)
                : null;
        }

        public static void BindValueField(PropertyField valueField, SerializedProperty activeRefProp, ReferenceWrapper.ReferenceType refType)
        {
            if (activeRefProp?.boxedValue == null) return;

            var valueProp = activeRefProp.FindPropertyRelative("m_Value");
            if (valueProp != null)
            {
                valueField.BindProperty(valueProp);
            }
        }

        public static void InitializeInstructionReference(SerializedProperty refProp, ReferenceWrapper.ReferenceType refType)
        {
            if (!InstructionReferencePropertyNames.TryGetValue(refType, out var propName))
                return;

            var prop = refProp.FindPropertyRelative(propName);
            if (prop == null)
                return;

            if (InstructionReferenceTypes.TryGetValue(refType, out var type))
            {
                if (prop.boxedValue == null || prop.boxedValue.ToString() == "(none)")
                {
                    switch (refType)
                    {
                        //<Shooter>
case ReferenceWrapper.ReferenceType.ShooterWeapon:
prop.boxedValue = GetWeaponShooterInstance.Create();
break;
                  //</Shooter>
                        //<Melee>
case ReferenceWrapper.ReferenceType.MeleeWeapon:
prop.boxedValue = GetWeaponMeleeInstance.Create();
break;
case ReferenceWrapper.ReferenceType.Shield:
prop.boxedValue = GetShieldMeleeInstance.Create();
break;
                    //</Melee>
                        default:
                            prop.boxedValue = Activator.CreateInstance(type);
                            break;
                    }
                }
            }
        }

        public static SerializedProperty GetInstructionReferenceProperty(SerializedProperty refProp, ReferenceWrapper.ReferenceType refType)
        {
            return InstructionReferencePropertyNames.TryGetValue(refType, out var propName)
                ? refProp.FindPropertyRelative(propName)
                : null;
        }

        public static string GetInstructionReferenceName(SerializedProperty refProp, ReferenceWrapper.ReferenceType refType)
        {
            var prop = GetInstructionReferenceProperty(refProp, refType);
            return prop?.boxedValue != null ? refProp.FindPropertyRelative("m_ReferenceName")?.stringValue : null;
        }
    }
}