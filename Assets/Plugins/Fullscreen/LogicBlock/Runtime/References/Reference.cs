using UnityEngine;
using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
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

namespace Fullscreen.LogicBlock.Runtime
{
    [Serializable]
    public class Reference
    {
        [SerializeField] private string m_ReferenceName;

        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] private PropertyGetString m_String = new PropertyGetString();
        [SerializeField] private PropertyGetDecimal m_Decimal = new PropertyGetDecimal();
        [SerializeField] private PropertyGetBool m_Bool = new PropertyGetBool();
        [SerializeField] private PropertyGetColor m_Color = new PropertyGetColor();
        [SerializeField] private PropertyGetPosition m_Position = new PropertyGetPosition();
        [SerializeField] private PropertyGetAnimation m_Animation = new PropertyGetAnimation();
        [SerializeField] private PropertyGetAudio m_Audio = new PropertyGetAudio();
        [SerializeField] private PropertyGetMaterial m_Material = new PropertyGetMaterial();
        [SerializeField] private PropertyGetSprite m_Sprite = new PropertyGetSprite();
        [SerializeField] private PropertyGetTexture m_Texture = new PropertyGetTexture();

        [SerializeField] private PropertyGetHandle m_Handle = new PropertyGetHandle();
        [SerializeField] private PropertyGetScene m_Scene = new PropertyGetScene();
        [SerializeField] private PropertyGetScale m_Scale = new PropertyGetScale();
        [SerializeField] private PropertyGetRotation m_Rotation = new PropertyGetRotation();
        [SerializeField] private PropertyGetDirection m_Direction = new PropertyGetDirection();

        //<Inventory>
[SerializeField] private PropertyGetItem m_Item = new PropertyGetItem();
[SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = new PropertyGetRuntimeItem();
[SerializeField] private PropertyGetLootTable m_LootTable = new PropertyGetLootTable();
     //</Inventory>

        //<Quests>
[SerializeField] private PropertyGetQuest m_Quest = new PropertyGetQuest();
     //</Quests>

        //<Shooter>
[SerializeField] private PropertyGetWeapon m_ShooterWeapon = GetWeaponShooterInstance.Create();
   //</Shooter>

        //<Melee>
[SerializeField] private PropertyGetWeapon m_MeleeWeapon = GetWeaponMeleeInstance.Create();
[SerializeField] private PropertyGetShield m_Shield = new PropertyGetShield();
[SerializeField] private PropertyGetSkill m_Skill = new PropertyGetSkill();
     //</Melee>

        //<Stats>
[SerializeField] private PropertyGetAttribute m_Attribute = new PropertyGetAttribute();
[SerializeField] private PropertyGetFormula m_Formula = new PropertyGetFormula();
[SerializeField] private PropertyGetStat m_Stat = new PropertyGetStat();
[SerializeField] private PropertyGetStatusEffect m_StatusEffect = new PropertyGetStatusEffect();
     //</Stats>

        public string ReferenceName => m_ReferenceName;

        public PropertyGetGameObject GameObject => m_GameObject;
        public PropertyGetString String => m_String;
        public PropertyGetDecimal Decimal => m_Decimal;
        public PropertyGetBool Bool => m_Bool;
        public PropertyGetColor Color => m_Color;
        public PropertyGetPosition Position => m_Position;
        public PropertyGetAnimation Animation => m_Animation;
        public PropertyGetAudio Audio => m_Audio;
        public PropertyGetMaterial Material => m_Material;
        public PropertyGetSprite Sprite => m_Sprite;
        public PropertyGetTexture Texture => m_Texture;

        public PropertyGetHandle Handle => m_Handle;
        public PropertyGetScene Scene => m_Scene;
        public PropertyGetScale Scale => m_Scale;
        public PropertyGetRotation Rotation => m_Rotation;
        public PropertyGetDirection Direction => m_Direction;

        //<Inventory>
public PropertyGetItem Item => m_Item;
public PropertyGetRuntimeItem RuntimeItem => m_RuntimeItem;
public PropertyGetLootTable LootTable => m_LootTable;
     //</Inventory>

        //<Quests>
public PropertyGetQuest Quest => m_Quest;
     //</Quests>

        //<Shooter>
public PropertyGetWeapon ShooterWeapon => m_ShooterWeapon;
   //</Shooter>

        //<Melee>
public PropertyGetWeapon MeleeWeapon => m_MeleeWeapon;
public PropertyGetShield Shield => m_Shield;
public PropertyGetSkill Skill => m_Skill;
     //</Melee>

        //<Stats>
public PropertyGetAttribute Attribute => m_Attribute;
public PropertyGetFormula Formula => m_Formula;
public PropertyGetStat Stat => m_Stat;
public PropertyGetStatusEffect StatusEffect => m_StatusEffect;
     //</Stats>

        public Reference(string name, ReferenceWrapper wrapper)
        {
            m_ReferenceName = name;

            switch (wrapper.Type)
            {
                case ReferenceWrapper.ReferenceType.GameObject:
                    m_GameObject = new PropertyGetGameObject();
                    break;
                case ReferenceWrapper.ReferenceType.String:
                    m_String = new PropertyGetString();
                    break;
                case ReferenceWrapper.ReferenceType.Number:
                    m_Decimal = new PropertyGetDecimal();
                    break;
                case ReferenceWrapper.ReferenceType.Boolean:
                    m_Bool = new PropertyGetBool();
                    break;
                case ReferenceWrapper.ReferenceType.Color:
                    m_Color = new PropertyGetColor();
                    break;
                case ReferenceWrapper.ReferenceType.Vector3:
                    m_Position = new PropertyGetPosition();
                    break;
                case ReferenceWrapper.ReferenceType.AnimationClip:
                    m_Animation = new PropertyGetAnimation();
                    break;
                case ReferenceWrapper.ReferenceType.AudioClip:
                    m_Audio = new PropertyGetAudio();
                    break;
                case ReferenceWrapper.ReferenceType.Material:
                    m_Material = new PropertyGetMaterial();
                    break;
                case ReferenceWrapper.ReferenceType.Sprite:
                    m_Sprite = new PropertyGetSprite();
                    break;
                case ReferenceWrapper.ReferenceType.Texture:
                    m_Texture = new PropertyGetTexture();
                    break;

                case ReferenceWrapper.ReferenceType.Handle:
                    m_Handle = new PropertyGetHandle();
                    break;
                case ReferenceWrapper.ReferenceType.Scene:
                    m_Scene = new PropertyGetScene();
                    break;
                case ReferenceWrapper.ReferenceType.Scale:
                    m_Scale = new PropertyGetScale();
                    break;
                case ReferenceWrapper.ReferenceType.Rotation:
                    m_Rotation = new PropertyGetRotation();
                    break;
                case ReferenceWrapper.ReferenceType.Direction:
                    m_Direction = new PropertyGetDirection();
                    break;

                //<Inventory>
case ReferenceWrapper.ReferenceType.Item:
  m_Item = new PropertyGetItem();
  break;
case ReferenceWrapper.ReferenceType.RuntimeItem:
  m_RuntimeItem = new PropertyGetRuntimeItem();
  break;
case ReferenceWrapper.ReferenceType.LootTable:
  m_LootTable = new PropertyGetLootTable();
  break;
             //</Inventory>

                //<Quests>
case ReferenceWrapper.ReferenceType.Quest:
 m_Quest = new PropertyGetQuest();
 break;
             //</Quests>

                //<Shooter>
case ReferenceWrapper.ReferenceType.ShooterWeapon:
m_ShooterWeapon = GetWeaponShooterInstance.Create();
break;
           //</Shooter>

                //<Melee>
case ReferenceWrapper.ReferenceType.MeleeWeapon:
 m_MeleeWeapon = GetWeaponMeleeInstance.Create();
 break;
case ReferenceWrapper.ReferenceType.Shield:
 m_Shield = GetShieldMeleeInstance.Create();
 break;
case ReferenceWrapper.ReferenceType.Skill:
 m_Skill = new PropertyGetSkill();
 break;
             //</Melee>

                //<Stats>
case ReferenceWrapper.ReferenceType.Attribute:
 m_Attribute = new PropertyGetAttribute();
 break;
case ReferenceWrapper.ReferenceType.Formula:
 m_Formula = new PropertyGetFormula();
 break;
case ReferenceWrapper.ReferenceType.Stat:
 m_Stat = new PropertyGetStat();
 break;
case ReferenceWrapper.ReferenceType.StatusEffect:
 m_StatusEffect = new PropertyGetStatusEffect();
 break;
             //</Stats>
            }
        }
    }
}