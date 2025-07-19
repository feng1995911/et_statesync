using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Fullscreen.LogicBlock.Runtime;
using GameCreator.Runtime.Common;

namespace Fullscreen.LogicBlock.Editor
{
    [CustomPropertyDrawer(typeof(PropertyGetBlockListReference))]
    public class PropertyGetBlockListReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            SerializedProperty blockProp = property.FindPropertyRelative("m_Block");
            SerializedProperty listIdProp = property.FindPropertyRelative("m_InstructionListId");
            SerializedProperty referenceNameProp = property.FindPropertyRelative("m_ReferenceName");

            var blockField = new PropertyField(blockProp, "Block");
            root.Add(blockField);

            var listDropdown = new DropdownField("List") { style = { marginTop = 4 } };
            root.Add(listDropdown);

            var refDropdown = new DropdownField("Reference") { style = { marginTop = 4 } };
            root.Add(refDropdown);

            ReferenceWrapper.ReferenceType expectedType = GetExpectedReferenceType(property);

            blockField.RegisterValueChangeCallback(_ =>
            {
                UpdateListDropdown(listDropdown, blockProp, listIdProp, expectedType);
                UpdateReferenceDropdown(refDropdown, blockProp, listIdProp, referenceNameProp, expectedType);
                property.serializedObject.ApplyModifiedProperties();
            });

            listDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != evt.previousValue)
                {
                    var block = blockProp.objectReferenceValue as Block;
                    if (block != null)
                    {
                        var allLists = block.ActionLists.Cast<INamedList>()
                            .Concat(block.BranchLists.Cast<INamedList>())
                            .Where(l => IsListValidForType(l, expectedType))
                            .ToList();

                        var selected = allLists.FirstOrDefault(l => l.ListName == evt.newValue);
                        if (selected != null)
                        {
                            listIdProp.stringValue = selected.UniqueId;
                            UpdateReferenceDropdown(refDropdown, blockProp, listIdProp, referenceNameProp, expectedType);
                            property.serializedObject.ApplyModifiedProperties();
                        }
                        else
                        {
                            listIdProp.stringValue = string.Empty;
                            UpdateReferenceDropdown(refDropdown, blockProp, listIdProp, referenceNameProp, expectedType);
                        }
                    }
                }
            });

            refDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != evt.previousValue && refDropdown.index >= 0)
                {
                    referenceNameProp.stringValue = evt.newValue;
                    property.serializedObject.ApplyModifiedProperties();
                }
            });

            UpdateListDropdown(listDropdown, blockProp, listIdProp, expectedType);
            UpdateReferenceDropdown(refDropdown, blockProp, listIdProp, referenceNameProp, expectedType);

            return root;
        }

        private ReferenceWrapper.ReferenceType GetExpectedReferenceType(SerializedProperty property)
        {
            string propertyPath = property.propertyPath;
            SerializedProperty parentProperty = property;

            while (parentProperty != null && parentProperty.propertyPath.Contains("."))
            {
                parentProperty = GetParentProperty(parentProperty);
                if (parentProperty == null) break;

                string parentTypeName = parentProperty.type;

                switch (parentTypeName)
                {
                    case string s when s.Contains("PropertyGetGameObject"):
                        return ReferenceWrapper.ReferenceType.GameObject;
                    case string s when s.Contains("PropertyGetString"):
                        return ReferenceWrapper.ReferenceType.String;
                   case string s when s.Contains("PropertyGetDecimal") || s.Contains("PropertyGetNumber") || s.Contains("PropertyGetInteger"):
                        return ReferenceWrapper.ReferenceType.Number;
                    case string s when s.Contains("PropertyGetBool"):
                        return ReferenceWrapper.ReferenceType.Boolean;
                    case string s when s.Contains("PropertyGetColor"):
                        return ReferenceWrapper.ReferenceType.Color;
                    case string s when s.Contains("PropertyGetPosition"):
                        return ReferenceWrapper.ReferenceType.Vector3;
                    case string s when s.Contains("PropertyGetAnimation"):
                        return ReferenceWrapper.ReferenceType.AnimationClip;
                    case string s when s.Contains("PropertyGetAudio"):
                        return ReferenceWrapper.ReferenceType.AudioClip;
                    case string s when s.Contains("PropertyGetMaterial"):
                        return ReferenceWrapper.ReferenceType.Material;
                    case string s when s.Contains("PropertyGetSprite"):
                        return ReferenceWrapper.ReferenceType.Sprite;
                    case string s when s.Contains("PropertyGetTexture"):
                        return ReferenceWrapper.ReferenceType.Texture;
                    case string s when s.Contains("PropertyGetHandle"):
                        return ReferenceWrapper.ReferenceType.Handle;
                    case string s when s.Contains("PropertyGetScene"):
                        return ReferenceWrapper.ReferenceType.Scene;
                    case string s when s.Contains("PropertyGetScale"):
                        return ReferenceWrapper.ReferenceType.Scale;
                    case string s when s.Contains("PropertyGetRotation"):
                        return ReferenceWrapper.ReferenceType.Rotation;
                    case string s when s.Contains("PropertyGetDirection"):
                        return ReferenceWrapper.ReferenceType.Direction;
                    case string s when s.Contains("PropertyGetItem"):
                        return ReferenceWrapper.ReferenceType.Item;
                    case string s when s.Contains("PropertyGetRuntimeItem"):
                        return ReferenceWrapper.ReferenceType.RuntimeItem;
                    case string s when s.Contains("PropertyGetLootTable"):
                        return ReferenceWrapper.ReferenceType.LootTable;
                    case string s when s.Contains("PropertyGetQuest"):
                        return ReferenceWrapper.ReferenceType.Quest;
                    case string s when s.Contains("PropertyGetWeapon"):
                    {
                        SerializedProperty getBlock = parentProperty.FindPropertyRelative("m_Property");
                        if (getBlock != null && !string.IsNullOrEmpty(getBlock.managedReferenceFullTypename))
                        {
                            string blockTypeName = getBlock.managedReferenceFullTypename;
                            if (blockTypeName.EndsWith("BlockGetShooterWeaponReference") || blockTypeName.Contains("BlockGetShooterWeaponReference"))
                                return ReferenceWrapper.ReferenceType.ShooterWeapon;
                            if (blockTypeName.EndsWith("BlockGetMeleeWeaponReference") || blockTypeName.Contains("BlockGetMeleeWeaponReference"))
                                return ReferenceWrapper.ReferenceType.MeleeWeapon;
                        }
                        return ReferenceWrapper.ReferenceType.ShooterWeapon;
                    }
                    case string s when s.Contains("PropertyGetShield"):
                        return ReferenceWrapper.ReferenceType.Shield;
                    case string s when s.Contains("PropertyGetSkill"):
                        return ReferenceWrapper.ReferenceType.Skill;
                    case string s when s.Contains("PropertyGetAttribute"):
                        return ReferenceWrapper.ReferenceType.Attribute;
                    case string s when s.Contains("PropertyGetFormula"):
                        return ReferenceWrapper.ReferenceType.Formula;
                    case string s when s.Contains("PropertyGetStatusEffect"):
                        return ReferenceWrapper.ReferenceType.StatusEffect;
                    case string s when s.Contains("PropertyGetStat"):
                        return ReferenceWrapper.ReferenceType.Stat;
                }
            }

            return ReferenceWrapper.ReferenceType.GameObject;
        }

        private SerializedProperty GetParentProperty(SerializedProperty property)
        {
            string parentPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.'));
            return string.IsNullOrEmpty(parentPath) ? null : property.serializedObject.FindProperty(parentPath);
        }

        private bool IsListValidForType(INamedList list, ReferenceWrapper.ReferenceType expectedType)
        {
            if (list is NamedInstructionList namedList)
            {
                return namedList.References != null &&
                       namedList.References.Any(r => r.Type == expectedType && r.GetReference() != null);
            }
            if (list is NamedBranchList branchList)
            {
                return branchList.References != null &&
                       branchList.References.Any(r => r.Type == expectedType && r.GetReference() != null);
            }
            return false;
        }

        private void UpdateListDropdown(DropdownField dropdown, SerializedProperty blockProp, SerializedProperty listIdProp, ReferenceWrapper.ReferenceType expectedType)
        {
            dropdown.choices.Clear();
            var block = blockProp.objectReferenceValue as Block;

            if (block == null)
            {
                dropdown.SetEnabled(false);
                dropdown.choices.Add("Block not assigned");
                dropdown.value = "Block not assigned";
                listIdProp.stringValue = string.Empty;
                return;
            }

            var allLists = block.ActionLists.Cast<INamedList>()
                .Concat(block.BranchLists.Cast<INamedList>())
                .Where(l => IsListValidForType(l, expectedType))
                .ToList();

            if (allLists.Count == 0)
            {
                dropdown.choices.Add($"No lists with {expectedType} references");
                dropdown.SetEnabled(false);
                dropdown.value = $"No lists with {expectedType} references";
                listIdProp.stringValue = string.Empty;
                return;
            }

            var listOptions = allLists.Select(l => l.ListName).ToList();
            dropdown.choices = listOptions;

            var currentList = allLists.FirstOrDefault(l => l.UniqueId == listIdProp.stringValue);
            int index = currentList != null ? listOptions.IndexOf(currentList.ListName) : 0;

            dropdown.SetEnabled(true);
            dropdown.index = Mathf.Clamp(index, 0, listOptions.Count - 1);
            dropdown.value = listOptions[dropdown.index];
            listIdProp.stringValue = allLists[dropdown.index].UniqueId;
        }

        private void UpdateReferenceDropdown(DropdownField dropdown, SerializedProperty blockProp, SerializedProperty listIdProp, SerializedProperty refNameProp, ReferenceWrapper.ReferenceType expectedType)
        {
            dropdown.choices.Clear();
            var block = blockProp.objectReferenceValue as Block;

            if (block == null)
            {
                dropdown.choices.Add("No block assigned");
                dropdown.SetEnabled(false);
                dropdown.value = "No block assigned";
                refNameProp.stringValue = string.Empty;
                return;
            }

            var allLists = block.ActionLists.Cast<INamedList>()
                .Concat(block.BranchLists.Cast<INamedList>())
                .ToList();

            var selectedList = allLists.FirstOrDefault(l => l.UniqueId == listIdProp.stringValue);

            if (selectedList == null)
            {
                dropdown.choices.Add("No list selected");
                dropdown.SetEnabled(false);
                dropdown.value = "No list selected";
                refNameProp.stringValue = string.Empty;
                return;
            }

            var references = selectedList.References?
                .Where(r => r.Type == expectedType && r.GetReference() != null)
                .Select(r => r.Name)
                .ToList() ?? new List<string>();

            if (references.Count == 0)
            {
                dropdown.choices.Add($"No {expectedType} references in list");
                dropdown.SetEnabled(false);
                dropdown.value = $"No {expectedType} references in list";
                refNameProp.stringValue = string.Empty;
                return;
            }

            dropdown.choices = references;
            dropdown.SetEnabled(true);

            string storedReferenceName = refNameProp.stringValue;
            if (!string.IsNullOrEmpty(storedReferenceName) && references.Contains(storedReferenceName))
            {
                dropdown.index = references.IndexOf(storedReferenceName);
                dropdown.value = storedReferenceName;
            }
            else
            {
                dropdown.index = 0;
                dropdown.value = references[0];
                refNameProp.stringValue = references[0];
            }
        }
    }
}