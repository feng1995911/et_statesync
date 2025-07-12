using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;
using Fullscreen.LogicBlock.Runtime;
using GameCreator.Runtime.VisualScripting;

namespace Fullscreen.LogicBlock.Editor
{
    [CustomPropertyDrawer(typeof(BlockInstructionReference))]
    public class BlockInstructionReferenceDrawer : PropertyDrawer
    {
        private const string CHOOSE_LIST = "Choose List";
        private const string NO_BLOCK = "Select a Block first";
        private static readonly Dictionary<string, int> LastBlockInstanceIDs = new Dictionary<string, int>();
        private SerializedObject serializedObject;
        private SerializedObject blockSerializedObject;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property == null || property.serializedObject == null)
            {
                Debug.LogWarning($"Property or serializedObject is null at path: {property?.propertyPath}");
                return new VisualElement();
            }

            serializedObject = property.serializedObject;
            serializedObject.Update();

            VisualElement root = new VisualElement();
            SerializedProperty blockProperty = property.FindPropertyRelative("m_Block");
            SerializedProperty instructionNameProp = property.FindPropertyRelative("m_InstructionName");
            SerializedProperty instructionIdProp = property.FindPropertyRelative("m_InstructionId");

            if (blockProperty == null || instructionNameProp == null || instructionIdProp == null)
            {
                Debug.LogWarning($"Could not find required properties at path: {property.propertyPath}");
                return root;
            }

            UpdateBlockSerializedObject(blockProperty);

            PropertyField blockField = new PropertyField(blockProperty, "Block")
            {
                style = { flexGrow = 1 }
            };
            blockField.BindProperty(blockProperty);
            root.Add(blockField);

            VisualElement content = new VisualElement();
            root.Add(content);

            string propertyPath = property.propertyPath;
            int currentBlockInstanceID = blockProperty.objectReferenceValue != null ? blockProperty.objectReferenceValue.GetInstanceID() : 0;
            LastBlockInstanceIDs[propertyPath] = currentBlockInstanceID;

            UpdateUI(content, property, blockProperty, instructionNameProp, instructionIdProp);
            serializedObject.ApplyModifiedProperties();

            blockField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(evt =>
            {
                int newBlockInstanceID = blockProperty.objectReferenceValue != null ? blockProperty.objectReferenceValue.GetInstanceID() : 0;
                bool isDifferentBlock = newBlockInstanceID != LastBlockInstanceIDs[propertyPath] && evt.previousValue != evt.newValue;

                if (isDifferentBlock)
                {
                    instructionNameProp.stringValue = string.Empty;
                    instructionIdProp.stringValue = string.Empty;
                    UpdateBlockSerializedObject(blockProperty);
                    UpdateUI(content, property, blockProperty, instructionNameProp, instructionIdProp);
                    TriggerOnValidate(property);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(serializedObject.targetObject);
                    LastBlockInstanceIDs[propertyPath] = newBlockInstanceID;
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            });

            return root;
        }

        private void UpdateBlockSerializedObject(SerializedProperty blockProperty)
        {
            if (blockProperty?.objectReferenceValue != null)
            {
                blockSerializedObject = new SerializedObject(blockProperty.objectReferenceValue);
                blockSerializedObject.Update();
            }
            else
            {
                blockSerializedObject = null;
            }
        }

        private void UpdateUI(VisualElement content, SerializedProperty property, SerializedProperty blockProperty,
            SerializedProperty instructionNameProp, SerializedProperty instructionIdProp)
        {
            content.Clear();

            Block block = blockProperty.objectReferenceValue as Block;
            string currentInstructionId = instructionIdProp?.stringValue;

            string[] options;
            string[] listIds;
            int selectedIndex = 0;
            string popupLabel = "List";

            bool isConditionList = IsConditionListInstruction(property);

            if (block == null)
            {
                options = new string[] { NO_BLOCK };
                listIds = new string[] { string.Empty };
                popupLabel = isConditionList ? "Condition List" : "Action List";
            }
            else
            {
                if (isConditionList)
                {
                    var lists = block.BranchLists;
                    string[] listNames = lists?.Select(list => list.ListName).ToArray() ?? Array.Empty<string>();
                    listIds = lists?.Select(list => list.UniqueId).ToArray() ?? Array.Empty<string>();
                    popupLabel = "Condition List";

                    options = listNames.Length > 0
                        ? new string[] { CHOOSE_LIST }.Concat(listNames).ToArray()
                        : new string[] { CHOOSE_LIST };
                    listIds = listNames.Length > 0
                        ? new string[] { string.Empty }.Concat(listIds).ToArray()
                        : new string[] { string.Empty };
                }
                else
                {
                    var lists = block.ActionLists;
                    string[] listNames = lists?.Select(list => list.ListName).ToArray() ?? Array.Empty<string>();
                    listIds = lists?.Select(list => list.UniqueId).ToArray() ?? Array.Empty<string>();
                    popupLabel = "Action List";

                    options = listNames.Length > 0
                        ? new string[] { CHOOSE_LIST }.Concat(listNames).ToArray()
                        : new string[] { CHOOSE_LIST };
                    listIds = listNames.Length > 0
                        ? new string[] { string.Empty }.Concat(listIds).ToArray()
                        : new string[] { string.Empty };
                }

                selectedIndex = string.IsNullOrEmpty(currentInstructionId)
                    ? 0
                    : Array.IndexOf(listIds, currentInstructionId);

                if (selectedIndex < 0 && !string.IsNullOrEmpty(currentInstructionId))
                {
                    selectedIndex = 0;
                    instructionIdProp.stringValue = string.Empty;
                    instructionNameProp.stringValue = string.Empty;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            PopupField<string> popupField = new PopupField<string>(popupLabel, options.ToList(), selectedIndex)
            {
                style =
                {
                    height = 21
                }
            };

            popupField.SetEnabled(block != null && options.Length > 1 && options[0] != NO_BLOCK);

            popupField.RegisterValueChangedCallback(evt =>
            {
                string newValue = evt.newValue;
                int newIndex = options.ToList().IndexOf(newValue);

                if (newValue == CHOOSE_LIST || newValue == NO_BLOCK)
                {
                    instructionNameProp.stringValue = string.Empty;
                    instructionIdProp.stringValue = string.Empty;
                }
                else
                {
                    instructionNameProp.stringValue = newValue;
                    instructionIdProp.stringValue = listIds[newIndex];
                }

                instructionNameProp.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(instructionNameProp.serializedObject.targetObject);
                content.Clear();
                UpdateUI(content, property, blockProperty, instructionNameProp, instructionIdProp);
                TriggerOnValidate(property);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            });

            content.Add(popupField);
        }

        private bool IsConditionListInstruction(SerializedProperty property)
        {
            object parent = GetParent(property);
            if (parent == null) return false;

            Type parentType = parent.GetType();
            return parentType.Name == "InstructionRunBlockConditionList";
        }

        private void TriggerOnValidate(SerializedProperty property)
        {
            EditorApplication.delayCall += () =>
            {
                if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null)
                {
                    return;
                }

                try
                {
                    property.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(property.serializedObject.targetObject);

                    if (blockSerializedObject != null)
                    {
                        blockSerializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(blockSerializedObject.targetObject);
                    }

                    foreach (UnityEngine.Object target in property.serializedObject.targetObjects)
                    {
                        if (target == null) continue;

                        object parentObj = GetParent(property);
                        if (parentObj != null)
                        {
                            System.Reflection.MethodInfo onValidate = parentObj.GetType().GetMethod("OnValidate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                            onValidate?.Invoke(parentObj, null);
                        }
                    }

                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Exception during TriggerOnValidate at path: {property?.propertyPath}: {ex.Message}");
                }
            };
        }

        private object GetParent(SerializedProperty prop)
        {
            if (prop == null || prop.serializedObject?.targetObject == null)
            {
                Debug.LogWarning($"Property or targetObject is null at path: {prop?.propertyPath}");
                return null;
            }

            string[] parts = prop.propertyPath.Replace(".Array.data[", "[").Split('.');
            object obj = prop.serializedObject.targetObject;

            foreach (string part in parts.Take(parts.Length - 1))
            {
                if (part.Contains("["))
                {
                    string fieldName = part[..part.IndexOf('[')];
                    int index = int.Parse(part[(part.IndexOf('[') + 1)..^1]);

                    var field = obj?.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    var list = field?.GetValue(obj) as System.Collections.IList;
                    obj = (list != null && index < list.Count) ? list[index] : null;
                }
                else
                {
                    var field = obj?.GetType().GetField(part, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    obj = field?.GetValue(obj);
                }

                if (obj == null)
                {
                    Debug.LogWarning($"Failed to resolve part '{part}' at path: {prop?.propertyPath}");
                    return null;
                }
            }
            return obj;
        }
    }
}