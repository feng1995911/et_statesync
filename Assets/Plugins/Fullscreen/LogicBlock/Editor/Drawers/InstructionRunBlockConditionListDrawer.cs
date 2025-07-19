using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Runtime.Common;
using System.Reflection;
using Fullscreen.LogicBlock.Runtime;
using GameCreator.Editor.Common;

namespace Fullscreen.LogicBlock.Editor
{
    [CustomPropertyDrawer(typeof(InstructionRunBlockConditionList))]
    public class InstructionRunBlockConditionListDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, int> LastBlockInstanceIDs = new();
        private static readonly Dictionary<string, int> LastReferencesCount = new();
        private SerializedObject serializedObject;
        private VisualElement referencesContainer;
        private SerializedObject blockSerializedObject;
        private Color Error = new Color32(233, 117, 76, 255);

        private static int GetArrayIndex(SerializedProperty property)
        {
            if (property == null || string.IsNullOrEmpty(property.propertyPath)) return -1;
            string path = property.propertyPath;
            int start = path.LastIndexOf('[') + 1;
            int end = path.LastIndexOf(']');
            if (start < 0 || end < start || end > path.Length) return -1;
            if (int.TryParse(path.Substring(start, end - start), out int index))
            {
                return index;
            }
            return -1;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property?.serializedObject == null || property.serializedObject.targetObject == null)
            {
                VisualElement errorContainer = new VisualElement();
                errorContainer.Add(new Label("Error: Invalid property") { style = { color = Color.red } });
                return errorContainer;
            }

            serializedObject = property.serializedObject;
            serializedObject.Update();
            VisualElement root = new VisualElement();

            SerializedProperty selectedListProp = property.FindPropertyRelative("m_SelectedList");
            SerializedProperty instanceBlockProp = property.FindPropertyRelative("m_InstanceBlock");
            SerializedProperty targetProp = property.FindPropertyRelative("m_Target");
            SerializedProperty referencesProp = property.FindPropertyRelative("m_References");

            if (selectedListProp == null || instanceBlockProp == null || targetProp == null || referencesProp == null)
            {
                root.Add(new Label("Error: Missing properties") { style = { color = Color.red } });
                return root;
            }

            PropertyField selectedListField = new PropertyField(selectedListProp, "Condition List");
            selectedListField.BindProperty(selectedListProp);
            root.Add(selectedListField);

            PropertyField targetField = new PropertyField(targetProp, "Target");
            targetField.BindProperty(targetProp);
            root.Add(targetField);

            PropertyField blockField = new PropertyField(instanceBlockProp, "Instance Block");
            blockField.BindProperty(instanceBlockProp);
            root.Add(blockField);

            referencesContainer = new VisualElement { name = "referencesContainer" };
            referencesContainer.style.marginTop = 5;
            root.Add(referencesContainer);

            string propertyPath = property.propertyPath;
            SerializedProperty blockInListProp = selectedListProp.FindPropertyRelative("m_Block");
            SerializedProperty instructionIdProp = selectedListProp.FindPropertyRelative("m_InstructionId");

            if (blockInListProp == null || instructionIdProp == null)
            {
                root.Add(new Label("Error: Missing block or instruction ID properties") { style = { color = Color.red } });
                return root;
            }

            UpdateBlockSerializedObject(blockInListProp);
            if (blockSerializedObject != null)
            {
                blockSerializedObject.Update();
            }
            UpdateReferencesUI(referencesContainer, referencesProp, property, blockInListProp);
            LastReferencesCount[propertyPath] = GetReferencesCount(blockInListProp, instructionIdProp?.stringValue);

            int currentBlockInstanceID = blockInListProp?.objectReferenceValue != null ? blockInListProp.objectReferenceValue.GetInstanceID() : 0;
            LastBlockInstanceIDs[propertyPath] = currentBlockInstanceID;

            blockField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(evt =>
            {
                if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null)
                {
                    return;
                }

                serializedObject = property.serializedObject;
                serializedObject.Update();

                SerializedProperty selectedListPropLocal = property.FindPropertyRelative("m_SelectedList");
                if (selectedListPropLocal == null)
                {
                    return;
                }

                SerializedProperty blockInListPropLocal = selectedListPropLocal.FindPropertyRelative("m_Block");
                SerializedProperty instructionIdPropLocal = selectedListPropLocal.FindPropertyRelative("m_InstructionId");
                SerializedProperty referencesPropLocal = property.FindPropertyRelative("m_References");

                if (blockInListPropLocal == null || instructionIdPropLocal == null || referencesPropLocal == null)
                {
                    return;
                }

                int newBlockInstanceID = blockInListPropLocal?.objectReferenceValue != null ? blockInListPropLocal.objectReferenceValue.GetInstanceID() : 0;
                if (newBlockInstanceID != LastBlockInstanceIDs[propertyPath])
                {
                    UpdateBlockSerializedObject(blockInListPropLocal);
                    if (blockSerializedObject != null)
                    {
                        blockSerializedObject.Update();
                    }
                    UpdateReferencesUI(referencesContainer, referencesPropLocal, property, blockInListPropLocal);
                    LastReferencesCount[propertyPath] = GetReferencesCount(blockInListPropLocal, instructionIdPropLocal?.stringValue);
                    SafeApplyModifiedProperties(property);
                    TriggerOnValidate(property);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    LastBlockInstanceIDs[propertyPath] = newBlockInstanceID;
                }
            });

            PropertyField instructionIdField = new PropertyField(instructionIdProp, "Instruction ID")
            {
                style = { display = DisplayStyle.None }
            };
            instructionIdField.BindProperty(instructionIdProp);
            root.Add(instructionIdField);

            instructionIdField.RegisterValueChangeCallback(evt =>
            {
                if (property == null || property.serializedObject == null)
                {
                    return;
                }

                serializedObject = property.serializedObject;
                if (serializedObject == null || serializedObject.targetObject == null)
                {
                    return;
                }

                SerializedProperty selectedListPropLocal = property.FindPropertyRelative("m_SelectedList");
                if (selectedListPropLocal == null)
                {
                    return;
                }

                SerializedProperty blockInListPropLocal = selectedListPropLocal.FindPropertyRelative("m_Block");
                SerializedProperty instructionIdPropLocal = selectedListPropLocal.FindPropertyRelative("m_InstructionId");
                if (blockInListPropLocal == null || instructionIdPropLocal == null)
                {
                    return;
                }

                SerializedProperty referencesPropLocal = property.FindPropertyRelative("m_References");
                if (referencesPropLocal == null)
                {
                    return;
                }

                serializedObject.Update();
                UpdateBlockSerializedObject(blockInListPropLocal);
                if (blockSerializedObject != null)
                {
                    blockSerializedObject.Update();
                }
                UpdateReferencesUI(referencesContainer, referencesPropLocal, property, blockInListPropLocal);
                LastReferencesCount[propertyPath] = GetReferencesCount(blockInListPropLocal, instructionIdPropLocal?.stringValue);
                SafeApplyModifiedProperties(property);
                TriggerOnValidate(property);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            });

            if (property.propertyPath.Contains(".Array.data["))
            {
                SerializedProperty parentArray = property.serializedObject.FindProperty(property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.')));
                if (parentArray != null && parentArray.isArray)
                {
                    for (int i = 0; i < parentArray.arraySize; i++)
                    {
                        SerializedProperty element = parentArray.GetArrayElementAtIndex(i);
                        UpdateReferencesForElement(element);
                    }
                }
            }

        bool isDelayCallActive = true;
        EditorApplication.delayCall += () =>
        {
            try
            {
                if (!isDelayCallActive)
                    return;

                if (property == null)
                    return;

                var so = property.serializedObject;
                if (so == null || so.targetObject == null)
                    return;

                if (property.propertyPath.Contains(".Array.data["))
                {
                    string parentPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.'));
                    SerializedProperty parentArray = so.FindProperty(parentPath);
                    if (parentArray == null || !parentArray.isArray || GetArrayIndex(property) >= parentArray.arraySize)
                        return;
                }

                SerializedProperty selectedListPropLocal = property.FindPropertyRelative("m_SelectedList");
                if (selectedListPropLocal == null)
                    return;

                SerializedProperty blockInListPropLocal = selectedListPropLocal.FindPropertyRelative("m_Block");
                SerializedProperty referencesPropLocal = property.FindPropertyRelative("m_References");
                if (blockInListPropLocal == null || referencesPropLocal == null)
                    return;

                so.Update();
                UpdateBlockSerializedObject(blockInListPropLocal);
                UpdateReferencesUI(referencesContainer, referencesPropLocal, property, blockInListPropLocal);
                so.ApplyModifiedProperties();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            catch (Exception)
            {
            }
        };

            root.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                LastBlockInstanceIDs.Remove(propertyPath);
                LastReferencesCount.Remove(propertyPath);
                isDelayCallActive = false;
            });

            return root;
        }

        private void UpdateReferencesForElement(SerializedProperty property)
        {
            if (property == null || property.serializedObject == null)
            {
                return;
            }

            SerializedProperty referencesProp = property.FindPropertyRelative("m_References");
            SerializedProperty blockInListProp = property.FindPropertyRelative("m_SelectedList.m_Block");
            if (referencesProp == null || blockInListProp == null)
            {
                return;
            }

            UpdateBlockSerializedObject(blockInListProp);
            if (blockSerializedObject != null)
            {
                blockSerializedObject.Update();
            }
            UpdateReferencesUI(referencesContainer, referencesProp, property, blockInListProp);
            serializedObject.ApplyModifiedProperties();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private VisualElement CreateReferenceElement(SerializedProperty refProp, SerializedProperty parentProperty, List<(string Name, string Type, SerializedProperty ReferenceProp)> referencesData)
        {
            VisualElement root = new VisualElement();

            SerializedProperty nameProp = refProp.FindPropertyRelative("m_ReferenceName");
            if (nameProp == null)
            {
                root.Add(new Label("Error: Failed to find Reference properties") { style = { color = Error } });
                return root;
            }

            string referenceType = nameProp.stringValue;
            int dataIndex = referencesData.FindIndex(data => data.Name == nameProp.stringValue);
            if (dataIndex >= 0)
            {
                referenceType = referencesData[dataIndex].Type;
            }

            var refType = Enum.TryParse<ReferenceWrapper.ReferenceType>(referenceType, out var parsedType) ? parsedType : ReferenceWrapper.ReferenceType.GameObject;

            SerializedProperty prop = ReferenceUtility.GetInstructionReferenceProperty(refProp, refType);
            if (prop?.boxedValue == null || prop.boxedValue.ToString() == "(none)")
            {
                ReferenceUtility.InitializeInstructionReference(refProp, refType);
                serializedObject.ApplyModifiedProperties();
            }

            IIcon headerIcon = ReferenceUtility.GetIconForReferenceType(refType);

            ReferenceBox contentBox = new ReferenceBox(nameProp.stringValue, true, headerIcon);
            contentBox.style.marginTop = -1;
            root.Add(contentBox);

            PropertyField field = new PropertyField(prop) { style = { flexGrow = 1, marginLeft = 10 } };
            if (prop != null)
            {
                field.BindProperty(prop);
            }
            else
            {
                field.Add(new Label($"Unable to link the reference '{nameProp.stringValue}'. Please make sure the required Game Creator 2 module is installed.") { style = { color = Error } });
            }

            if (field == null)
            {
                field = new PropertyField();
                field.Add(new Label("Property field not initialized") { style = { color = Error } });
            }

            contentBox.Content.Add(field);

            field.RegisterCallback<ChangeEvent<SerializedProperty>>(evt =>
            {
                if (serializedObject == null || serializedObject.targetObject == null)
                {
                    return;
                }
                SafeApplyModifiedProperties(parentProperty);
                TriggerOnValidate(parentProperty);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            });

            return root;
        }

        private void UpdateReferencesUI(VisualElement container, SerializedProperty referencesProp, SerializedProperty property, SerializedProperty blockProp)
        {
            if (container == null || referencesProp == null || property == null || blockProp == null)
            {
                if (container != null)
                    container.Add(new Label("Error: Invalid parameters") { style = { color = Color.red } });
                return;
            }

            if (property.type != "managedReference<InstructionRunBlockConditionList>" && 
                !property.managedReferenceFullTypename.Contains("Fullscreen.LogicBlock.Runtime.InstructionRunBlockConditionList"))
            {
                return;
            }

            container.Clear();

            if (!referencesProp.isArray || property.serializedObject == null || property.serializedObject.targetObject == null)
            {
                container.Add(new Label("Error: Invalid references or property") { style = { color = Color.red } });
                return;
            }

            string instructionId = property.FindPropertyRelative("m_SelectedList.m_InstructionId")?.stringValue;
            int expectedCount = GetReferencesCount(blockProp, instructionId);
            var referencesData = GetReferencesData(blockProp, instructionId);

            if (referencesProp.arraySize != expectedCount)
            {
                referencesProp.arraySize = expectedCount;
                for (int i = 0; i < referencesProp.arraySize; i++)
                {
                    SerializedProperty refProp = referencesProp.GetArrayElementAtIndex(i);
                    if (i < referencesData.Count)
                    {
                        SerializedProperty nameProp = refProp.FindPropertyRelative("m_ReferenceName");
                        nameProp.stringValue = referencesData[i].Name;
                        var refType = (ReferenceWrapper.ReferenceType)Enum.Parse(typeof(ReferenceWrapper.ReferenceType), referencesData[i].Type);
                        ReferenceUtility.InitializeInstructionReference(refProp, refType);
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }

            if (referencesProp.arraySize > 0)
            {
                Label header = new Label("References");
                header.style.marginBottom = 5;
                header.style.marginLeft = 4;
                container.Add(header);
            }

            for (int i = 0; i < referencesProp.arraySize; i++)
            {
                SerializedProperty refProp = referencesProp.GetArrayElementAtIndex(i);
                VisualElement refElement = CreateReferenceElement(refProp, property, referencesData);
                container.Add(refElement);
            }

            if (container.childCount <= (referencesProp.arraySize > 0 ? 1 : 0))
            {
                EditorApplication.delayCall += () => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        private void UpdateBlockSerializedObject(SerializedProperty blockProp)
        {
            if (blockProp == null || blockProp.objectReferenceValue == null)
            {
                blockSerializedObject = null;
                return;
            }

            if (blockProp.objectReferenceValue is UnityEngine.Object obj && obj != null)
            {
                blockSerializedObject = new SerializedObject(obj);
                blockSerializedObject.Update();
            }
            else
            {
                blockSerializedObject = null;
            }
        }

        private int GetReferencesCount(SerializedProperty blockProp, string instructionId)
        {
            if (blockProp?.objectReferenceValue == null || string.IsNullOrEmpty(instructionId) || blockSerializedObject == null)
            {
                return 0;
            }

            blockSerializedObject.Update();
            SerializedProperty branchListsProp = blockSerializedObject.FindProperty("m_BranchLists");
            if (branchListsProp == null || !branchListsProp.isArray)
            {
                return 0;
            }

            for (int i = 0; i < branchListsProp.arraySize; i++)
            {
                SerializedProperty listProp = branchListsProp.GetArrayElementAtIndex(i);
                SerializedProperty uniqueIdProp = listProp.FindPropertyRelative("m_UniqueId");
                if (uniqueIdProp?.stringValue == instructionId)
                {
                    SerializedProperty referencesProp = listProp.FindPropertyRelative("m_References");
                    return referencesProp?.isArray == true ? referencesProp.arraySize : 0;
                }
            }

            return 0;
        }

        private List<(string Name, string Type, SerializedProperty ReferenceProp)> GetReferencesData(SerializedProperty blockProp, string instructionId)
        {
            List<(string Name, string Type, SerializedProperty ReferenceProp)> referencesData = new();
            if (blockProp?.objectReferenceValue == null || string.IsNullOrEmpty(instructionId) || blockSerializedObject == null)
            {
                return referencesData;
            }

            blockSerializedObject.Update();
            SerializedProperty branchListsProp = blockSerializedObject.FindProperty("m_BranchLists");
            if (branchListsProp == null || !branchListsProp.isArray)
            {
                return referencesData;
            }

            for (int i = 0; i < branchListsProp.arraySize; i++)
            {
                SerializedProperty listProp = branchListsProp.GetArrayElementAtIndex(i);
                SerializedProperty uniqueIdProp = listProp.FindPropertyRelative("m_UniqueId");
                if (uniqueIdProp?.stringValue != instructionId) continue;

                SerializedProperty referencesProp = listProp.FindPropertyRelative("m_References");
                if (referencesProp == null || !referencesProp.isArray) continue;

                for (int j = 0; j < referencesProp.arraySize; j++)
                {
                    SerializedProperty refProp = referencesProp.GetArrayElementAtIndex(j);
                    SerializedProperty typeProp = refProp.FindPropertyRelative("m_Type");
                    if (typeProp == null) continue;

                    var refType = (ReferenceWrapper.ReferenceType)typeProp.enumValueIndex;
                    string name = ReferenceUtility.GetReferenceName(refProp);
                    SerializedProperty valueProp = ReferenceUtility.GetActiveReferenceProperty(refProp);
                    if (!string.IsNullOrEmpty(name))
                    {
                        referencesData.Add((name, refType.ToString(), valueProp));
                    }
                }
                break;
            }

            return referencesData;
        }

        private void SafeApplyModifiedProperties(SerializedProperty property)
        {
            if (property?.serializedObject == null || property.serializedObject.targetObject == null)
            {
                return;
            }

            try
            {
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                if (serializedObject.targetObject != null)
                    EditorUtility.SetDirty(serializedObject.targetObject);
                if (blockSerializedObject != null && blockSerializedObject.targetObject != null)
                {
                    blockSerializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(blockSerializedObject.targetObject);
                }
            }
            catch (Exception)
            {
            }
        }

        private void TriggerOnValidate(SerializedProperty property)
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null)
                    {
                        return;
                    }

                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();

                    if (blockSerializedObject != null && blockSerializedObject.targetObject != null)
                    {
                        blockSerializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(blockSerializedObject.targetObject);
                    }

                    foreach (UnityEngine.Object target in property.serializedObject.targetObjects)
                    {
                        if (target == null)
                        {
                            continue;
                        }

                        EditorUtility.SetDirty(target);
                        object parentObj = GetParent(property);
                        if (parentObj != null)
                        {
                            var onValidate = parentObj.GetType().GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            onValidate?.Invoke(parentObj, null);
                        }
                    }

                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
                catch (Exception)
                {
                }
            };
        }

        private object GetParent(SerializedProperty prop)
        {
            if (prop?.serializedObject?.targetObject == null)
            {
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
                    var field = obj?.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    var list = field?.GetValue(obj) as System.Collections.IList;
                    obj = (list != null && index < list.Count) ? list[index] : null;
                }
                else
                {
                    var field = obj?.GetType().GetField(part, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    obj = field?.GetValue(obj);
                }

                if (obj == null)
                {
                    return null;
                }
            }

            return obj;
        }
    }
}