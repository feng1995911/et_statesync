using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using GameCreator.Runtime.Common;
using GameCreator.Editor.Common;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using Fullscreen.LogicBlock.Runtime;

namespace Fullscreen.LogicBlock.Editor
{
    [CustomEditor(typeof(Fullscreen.LogicBlock.Runtime.Block))]
    public class BlockEditor : UnityEditor.Editor
    {
        private VisualElement m_Root;
        private VisualElement m_Container;
        private const string USS_PATH = "Assets/Plugins/Fullscreen/LogicBlock/Editor/StyleSheets/BlockBox";
        private const string ERR_DUPLICATE_ID = "Another Block has the same Unique ID as this one";
        private static readonly Length DEFAULT_MARGIN_TOP = new Length(5, LengthUnit.Pixel);
        private IIcon ICON_INSTRUCTIONS;
        private IIcon ICON_CONDITIONS;
        private IIcon ICON_REFERENCE;
        private ErrorMessage m_ErrorUniqueId;
        private PropertyField m_FieldUniqueId;

        public override VisualElement CreateInspectorGUI()
        {
            m_Root = new VisualElement();
            InitializeRootElement();
            m_Container = new VisualElement();

            StyleSheet[] styleSheets = StyleSheetUtils.Load(USS_PATH);
            foreach (StyleSheet sheet in styleSheets)
            {
                m_Root.styleSheets.Add(sheet);
            }

            ICON_INSTRUCTIONS = new IconInstructions(ColorTheme.Type.Gray);
            ICON_CONDITIONS = new IconConditions(ColorTheme.Type.Gray);
            ICON_REFERENCE = new IconReference(ColorTheme.Type.Gray);
            
            SetupUniqueIdField();

            m_Root.Add(m_Container);
            RefreshList(-1, -1, -1, null);

            return m_Root;
        }

        private void InitializeRootElement()
        {
            m_Root = new VisualElement
            {
                style = { marginTop = DEFAULT_MARGIN_TOP }
            };
        }

        private void SetupUniqueIdField()
        {
            SerializedProperty uniqueId = serializedObject.FindProperty("m_UniqueId");
            m_ErrorUniqueId = new ErrorMessage(string.Empty);
            m_FieldUniqueId = new PropertyField(uniqueId, "Unique ID");
            
            m_FieldUniqueId.RegisterValueChangeCallback(_ => RefreshErrorUniqueId());
            RefreshErrorUniqueId();
            
            m_Root.Add(m_FieldUniqueId);
            m_Root.Add(m_ErrorUniqueId);
        }

        private void RefreshErrorUniqueId()
        {
            serializedObject.Update();
            m_ErrorUniqueId.style.display = DisplayStyle.None;

            SerializedProperty uniqueId = serializedObject.FindProperty("m_UniqueId");
            string itemId = uniqueId
                .FindPropertyRelative(UniqueIDDrawer.SERIALIZED_ID)
                .FindPropertyRelative(IdStringDrawer.NAME_STRING)
                .stringValue;

            CheckForDuplicateIds(itemId);
        }

        private void CheckForDuplicateIds(string itemId)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(Block)}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Block block = AssetDatabase.LoadAssetAtPath<Block>(path);

                if (block.ID.String == itemId && block != target)
                {
                    m_ErrorUniqueId.Text = ERR_DUPLICATE_ID;
                    m_ErrorUniqueId.style.display = DisplayStyle.Flex;
                    return;
                }
            }
        }

        private Button CreateAddButton(string text, IIcon icon, System.Action onClick)
        {
            Button button = new Button(onClick);
            button.AddToClassList("gc-add-button");

            button.Add(new Image
            {
                image = icon.Texture
            });
            button.Add(new Label { text = text });

            return button;
        }

        public void RefreshList(int newActionListIndex = -1, int newBranchListIndex = -1, int refListIndex = -1, string refListType = null)
        {
            if (m_Container == null || serializedObject == null || serializedObject.targetObject == null)
            {
                return;
            }

            m_Container.Clear();
            serializedObject.Update();
            SerializedProperty actionListsProp = serializedObject.FindProperty("m_ActionLists");
            SerializedProperty branchListsProp = serializedObject.FindProperty("m_BranchLists");

            if (actionListsProp == null || branchListsProp == null)
            {
                return;
            }

            int actionListsCount = actionListsProp.arraySize;
            int branchListsCount = branchListsProp.arraySize;

            m_Container.Add(new SpaceSmall());

            for (int i = 0; i < actionListsCount; ++i)
            {
                SerializedProperty elementProp = actionListsProp.GetArrayElementAtIndex(i);
                if (elementProp == null) continue;
                bool isLast = i == actionListsCount - 1;
                bool isNew = i == newActionListIndex;
                bool isRefList = refListType == "Instruction" && i == refListIndex;
                m_Container.Add(CreateActionListBox(elementProp, i, isLast, isNew || isRefList));
            }

            m_Container.Add(new SpaceSmall());

            Button buttonAddInstruction = CreateAddButton("Add Instruction List...", ICON_INSTRUCTIONS, () =>
            {
                serializedObject.Update();
                actionListsProp.InsertArrayElementAtIndex(actionListsProp.arraySize);
                SerializedProperty newElement = actionListsProp.GetArrayElementAtIndex(actionListsProp.arraySize - 1);

                SerializedProperty nameProp = newElement.FindPropertyRelative("m_ListName");
                SerializedProperty instructionsProp = newElement.FindPropertyRelative("m_Instructions");
                SerializedProperty referencesProp = newElement.FindPropertyRelative("m_References");
                SerializedProperty uniqueIdProp = newElement.FindPropertyRelative("m_UniqueId");
                SerializedProperty iconTypeProp = newElement.FindPropertyRelative("m_IconTypeName");
                SerializedProperty iconColorProp = newElement.FindPropertyRelative("m_IconColor");

                if (nameProp == null || instructionsProp == null || referencesProp == null || uniqueIdProp == null || iconTypeProp == null || iconColorProp == null)
                {
                    return;
                }

                nameProp.stringValue = $"Instruction List {actionListsProp.arraySize}";
                uniqueIdProp.stringValue = Guid.NewGuid().ToString();
                iconTypeProp.stringValue = "IconInstructions";
                iconColorProp.enumValueIndex = (int)ColorTheme.Type.Blue;
                referencesProp.ClearArray();

                if (instructionsProp != null)
                {
                    SerializedProperty instructionsArray = instructionsProp.FindPropertyRelative("m_Instructions");
                    if (instructionsArray?.isArray ?? false)
                    {
                        instructionsArray.ClearArray();
                    }
                }

                serializedObject.ApplyModifiedProperties();
                RefreshList(actionListsProp.arraySize - 1, -1, -1, null);
            });

            m_Container.Add(buttonAddInstruction);
            m_Container.Add(new SpaceSmall());

            for (int i = 0; i < branchListsCount; ++i)
            {
                SerializedProperty elementProp = branchListsProp.GetArrayElementAtIndex(i);
                if (elementProp == null) continue;
                bool isLast = i == branchListsCount - 1;
                bool isNew = i == newBranchListIndex;
                bool isRefList = refListType == "Branch" && i == refListIndex;
                m_Container.Add(CreateBranchListBox(elementProp, i, isLast, isNew || isRefList));
            }

            m_Container.Add(new SpaceSmall());

            Button buttonAddBranch = CreateAddButton("Add Conditions List...", ICON_CONDITIONS, () =>
            {
                serializedObject.Update();
                branchListsProp.InsertArrayElementAtIndex(branchListsProp.arraySize);
                SerializedProperty newElement = branchListsProp.GetArrayElementAtIndex(branchListsProp.arraySize - 1);

                SerializedProperty nameProp = newElement.FindPropertyRelative("m_ListName");
                SerializedProperty branchesProp = newElement.FindPropertyRelative("m_Branches");
                SerializedProperty referencesProp = newElement.FindPropertyRelative("m_References");
                SerializedProperty uniqueIdProp = newElement.FindPropertyRelative("m_UniqueId");
                SerializedProperty iconTypeProp = newElement.FindPropertyRelative("m_IconTypeName");
                SerializedProperty iconColorProp = newElement.FindPropertyRelative("m_IconColor");

                if (nameProp == null || branchesProp == null || referencesProp == null || uniqueIdProp == null || iconTypeProp == null || iconColorProp == null)
                {
                    return;
                }

                nameProp.stringValue = $"Condition List {branchListsProp.arraySize}";
                uniqueIdProp.stringValue = Guid.NewGuid().ToString();
                iconTypeProp.stringValue = "IconConditions";
                iconColorProp.enumValueIndex = (int)ColorTheme.Type.Green;
                referencesProp.ClearArray();

                if (branchesProp != null)
                {
                    SerializedProperty branchesArray = branchesProp.FindPropertyRelative("m_Branches");
                    if (branchesArray?.isArray ?? false)
                    {
                        branchesArray.ClearArray();
                    }
                }

                serializedObject.ApplyModifiedProperties();
                RefreshList(-1, branchListsProp.arraySize - 1, -1, null);
            });

            m_Container.Add(buttonAddBranch);

            m_Root.Bind(serializedObject);
            m_Container.MarkDirtyRepaint();
        }

        private string GenerateUniqueName(string baseName, SerializedProperty referencesProp, ReferenceWrapper.ReferenceType refType)
        {
            string cleanBaseName = baseName;
            int existingNumber = 0;

            if (baseName.EndsWith(")"))
            {
                int openParenIndex = baseName.LastIndexOf("(");
                if (openParenIndex > 0 && int.TryParse(baseName.Substring(openParenIndex + 1, baseName.Length - openParenIndex - 2), out int num))
                {
                    cleanBaseName = baseName.Substring(0, openParenIndex).Trim();
                    existingNumber = num;
                }
            }

            List<string> existingNames = new List<string>();
            int typeCount = 0;
            int highestNumber = 0;

            for (int i = 0; i < referencesProp.arraySize; i++)
            {
                SerializedProperty referenceProp = referencesProp.GetArrayElementAtIndex(i);
                SerializedProperty activeRefProp = ReferenceUtility.GetActiveReferenceProperty(referenceProp);
                if (activeRefProp?.boxedValue == null)
                {
                    continue;
                }

                SerializedProperty typeProp = referenceProp.FindPropertyRelative("m_Type");
                if (typeProp == null || typeProp.enumValueIndex != (int)refType)
                {
                    continue;
                }

                typeCount++;
                string name = ReferenceUtility.GetReferenceName(referenceProp);
                if (!string.IsNullOrEmpty(name))
                {
                    existingNames.Add(name);

                    if (name.StartsWith(cleanBaseName) && name.EndsWith(")"))
                    {
                        int openParenIndex = name.LastIndexOf("(");
                        if (openParenIndex > 0 && int.TryParse(name.Substring(openParenIndex + 1, name.Length - openParenIndex - 2), out int num))
                        {
                            highestNumber = Mathf.Max(highestNumber, num);
                        }
                    }
                }
            }

            if (!existingNames.Contains(baseName))
            {
                return baseName;
            }

            int number = Mathf.Max(existingNumber, highestNumber);
            string candidateName;
            do
            {
                number++;
                candidateName = $"{cleanBaseName}({number})";
            } while (existingNames.Contains(candidateName));

            return candidateName;
        }

        private VisualElement CreateReferenceInputUI(SerializedProperty referencesProp, VisualElement addReferenceContainer, string listType, int listIndex)
        {
            VisualElement inputContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 5
                }
            };

            ReferenceWrapper.ReferenceType defaultType = ReferenceWrapper.ReferenceType.GameObject;
            string defaultName = GenerateUniqueName($"{defaultType}", referencesProp, defaultType);
            bool isDefaultName = true;

            TextField nameField = new TextField
            {
                value = defaultName
            };
            nameField.style.flexGrow = 2;
            nameField.style.marginRight = 5;

            EnumField typeField = new EnumField("", defaultType);
            typeField.style.flexGrow = 3;
            typeField.style.marginRight = 5;

            IIcon buttonIcon = ReferenceUtility.GetIconForReferenceType(defaultType);

            Button createButton = new Button();
            createButton.style.flexGrow = 1;
            createButton.style.flexDirection = FlexDirection.Row;
            createButton.style.justifyContent = Justify.Center;
            createButton.style.alignItems = Align.Center;
            createButton.style.marginRight = 5;
            createButton.AddToClassList("gc-add-button");
            createButton.Add(new Image
            {
                image = buttonIcon.Texture,
                style = { width = 16, height = 16, marginRight = 5 }
            });
            createButton.Add(new Label { text = "Create" });

            nameField.RegisterValueChangedCallback(evt =>
            {
                isDefaultName = false;
            });

            typeField.RegisterValueChangedCallback(evt =>
            {
                ReferenceWrapper.ReferenceType newType = (ReferenceWrapper.ReferenceType)evt.newValue;
                buttonIcon = ReferenceUtility.GetIconForReferenceType(newType);
                createButton.Clear();
                createButton.Add(new Image
                {
                    image = buttonIcon.Texture,
                    style = { width = 16, height = 16, marginRight = 5 }
                });
                createButton.Add(new Label { text = "Create" });

                if (isDefaultName)
                {
                    string newDefaultName = GenerateUniqueName($"{newType}", referencesProp, newType);
                    nameField.value = newDefaultName;
                }
            });

            createButton.clicked += () =>
            {
                serializedObject.Update();

                int initialSize = referencesProp.arraySize;
                string refName = string.IsNullOrWhiteSpace(nameField.value)
                    ? GenerateUniqueName($"{(ReferenceWrapper.ReferenceType)typeField.value}", referencesProp, (ReferenceWrapper.ReferenceType)typeField.value)
                    : GenerateUniqueName(nameField.value, referencesProp, (ReferenceWrapper.ReferenceType)typeField.value);

                referencesProp.InsertArrayElementAtIndex(referencesProp.arraySize);
                SerializedProperty newElement = referencesProp.GetArrayElementAtIndex(referencesProp.arraySize - 1);

                SerializedProperty typeProp = newElement.FindPropertyRelative("m_Type");
                if (typeProp == null)
                {
                    Debug.LogError("Type property not found for new reference.");
                    return;
                }

                typeProp.enumValueIndex = (int)(ReferenceWrapper.ReferenceType)typeField.value;
                ReferenceUtility.InitializeReference(newElement, (ReferenceWrapper.ReferenceType)typeField.value, refName);

                serializedObject.ApplyModifiedProperties();

                addReferenceContainer.Clear();
                Button buttonAddReference = CreateAddButton("Add Reference...", ICON_REFERENCE, () =>
                {
                    addReferenceContainer.Clear();
                    addReferenceContainer.Add(CreateReferenceInputUI(referencesProp, addReferenceContainer, listType, listIndex));
                });
                buttonAddReference.AddToClassList("gc-add-button");
                addReferenceContainer.Add(buttonAddReference);

                RefreshList(-1, -1, listIndex, listType);
            };

            Button cancelButton = new Button(() =>
            {
                addReferenceContainer.Clear();
                Button buttonAddReference = CreateAddButton("Add Reference...", ICON_REFERENCE, () =>
                {
                    addReferenceContainer.Clear();
                    addReferenceContainer.Add(CreateReferenceInputUI(referencesProp, addReferenceContainer, listType, listIndex));
                });
                buttonAddReference.AddToClassList("gc-add-button");
                addReferenceContainer.Add(buttonAddReference);
            })
            {
                text = "X"
            };
            cancelButton.style.width = 25;
            cancelButton.style.alignSelf = Align.FlexStart;
            cancelButton.AddToClassList("gc-add-button");

            inputContainer.Add(nameField);
            inputContainer.Add(typeField);
            inputContainer.Add(createButton);
            inputContainer.Add(cancelButton);

            return inputContainer;
        }

        private VisualElement CreateActionListBox(SerializedProperty listProp, int index, bool isLast, bool isExpanded)
        {
            SerializedProperty nameProp = listProp.FindPropertyRelative("m_ListName");
            SerializedProperty instrProp = listProp.FindPropertyRelative("m_Instructions");
            SerializedProperty referencesProp = listProp.FindPropertyRelative("m_References");

            if (nameProp == null || instrProp == null || referencesProp == null)
            {
                return new VisualElement();
            }

            string listName = string.IsNullOrEmpty(nameProp.stringValue)
                ? $"Instruction List {index + 1}"
                : nameProp.stringValue;

            Block block = (Block)target;
            BlockBox contentBox = new BlockBox(listProp, block)
            {
                Title = listName,
                IsExpanded = isExpanded
            };

            if (isLast)
            {
                contentBox.AddToClassList("gc-content-box-last");
            }

            contentBox.SetupContextMenu(() =>
            {
                serializedObject.Update();
                SerializedProperty listArrayProp = serializedObject.FindProperty("m_ActionLists");
                listArrayProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                RefreshList(-1, -1, -1, null);
            });

            contentBox.SetupDragHandling((fromIndex, toIndex, sourceListType) =>
            {
                ReorderLists(fromIndex, toIndex, sourceListType);
            });

            PropertyField instructionsField = new PropertyField(instrProp, "Instructions");

            List<VisualElement> referenceElements = new List<VisualElement>();
            VisualElement spacer = new VisualElement { style = { height = 2 } };
            VisualElement addReferenceContainer = new VisualElement();
            addReferenceContainer.AddToClassList("gc-add-reference-container");

            void AddReferences()
            {
                foreach (var element in referenceElements)
                {
                    if (contentBox.Content.Contains(element))
                    {
                        contentBox.Content.Remove(element);
                    }
                }
                referenceElements.Clear();

                serializedObject.Update();
                for (int i = 0; i < referencesProp.arraySize; i++)
                {
                    int currentIndex = i;
                    SerializedProperty referenceProp = referencesProp.GetArrayElementAtIndex(i);
                    SerializedProperty typeProp = referenceProp.FindPropertyRelative("m_Type");

                    if (typeProp == null)
                    {
                        Debug.LogWarning($"Reference at index {i} has null type property");
                        continue;
                    }

                    var refType = (ReferenceWrapper.ReferenceType)typeProp.enumValueIndex;
                    string refName = $"Reference {i + 1}";
                    IIcon headerIcon = ReferenceUtility.GetIconForReferenceType(refType);
                    SerializedProperty activeRefProp = ReferenceUtility.GetActiveReferenceProperty(referenceProp);

                    if (activeRefProp?.boxedValue == null)
                    {
                        ReferenceUtility.InitializeReference(referenceProp, refType, refName);
                        serializedObject.ApplyModifiedProperties();
                        activeRefProp = ReferenceUtility.GetActiveReferenceProperty(referenceProp);
                    }

                    if (activeRefProp?.boxedValue != null)
                    {
                        SerializedProperty refNameProp = activeRefProp.FindPropertyRelative("m_Name");
                        if (refNameProp != null)
                        {
                            refName = string.IsNullOrEmpty(refNameProp.stringValue) ? $"Reference {i + 1}" : refNameProp.stringValue;
                        }
                    }

                    ReferenceBox refBox = new ReferenceBox(refName, false, headerIcon);
                    refBox.style.marginTop = -1;
                    refBox.AddToClassList("gc-content-box");

                    VisualElement refContainer = new VisualElement
                    {
                        style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
                    };

                    TextField nameField = new TextField("Name")
                    {
                        value = refName,
                        style = { flexGrow = 1, marginRight = 5 }
                    };
                    if (activeRefProp?.boxedValue != null)
                    {
                        nameField.BindProperty(activeRefProp.FindPropertyRelative("m_Name"));
                    }

                    EnumField typeField = new EnumField("Type", refType)
                    {
                        style = { flexGrow = 1, marginRight = 5 }
                    };
                    typeField.BindProperty(typeProp);

                    PropertyField valueField = new PropertyField
                    {
                        style = { flexGrow = 1 }
                    };
                    ReferenceUtility.BindValueField(valueField, activeRefProp, refType);

                    Button deleteButton = new Button(() =>
                    {
                        serializedObject.Update();
                        if (currentIndex >= 0 && currentIndex < referencesProp.arraySize)
                        {
                            referencesProp.DeleteArrayElementAtIndex(currentIndex);
                            serializedObject.ApplyModifiedProperties();
                            RefreshList(-1, -1, index, "Instruction");
                        }
                    })
                    {
                        text = "Delete"
                    };
                    deleteButton.AddToClassList("gc-add-button");

                    refContainer.Add(deleteButton);
                    refBox.Content.Add(refContainer);
                    contentBox.Content.Add(refBox);
                    referenceElements.Add(refBox);
                }

                contentBox.Content.Add(spacer);
                addReferenceContainer.Clear();
                Button buttonAddReference = CreateAddButton("Add Reference...", ICON_REFERENCE, () =>
                {
                    addReferenceContainer.Clear();
                    addReferenceContainer.Add(CreateReferenceInputUI(referencesProp, addReferenceContainer, "Instruction", index));
                });
                buttonAddReference.AddToClassList("gc-add-button");
                addReferenceContainer.Add(buttonAddReference);
                contentBox.Content.Add(addReferenceContainer);
                referenceElements.Add(spacer);
                referenceElements.Add(addReferenceContainer);
            }

            void BindAndAddInstructionsField()
            {
                if (!contentBox.IsExpanded) return;
                if (!contentBox.Content.Contains(instructionsField))
                {
                    instructionsField.Bind(serializedObject);
                    contentBox.Content.Insert(1, new SpaceSmall());
                    contentBox.Content.Insert(1, instructionsField);
                }
                AddReferences();
                contentBox.Content.MarkDirtyRepaint();
            }

            if (isExpanded)
            {
                BindAndAddInstructionsField();
            }

            contentBox.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue)
                {
                    BindAndAddInstructionsField();
                }
            });

            return contentBox;
        }

        private VisualElement CreateBranchListBox(SerializedProperty listProp, int index, bool isLast, bool isExpanded)
        {
            SerializedProperty nameProp = listProp.FindPropertyRelative("m_ListName");
            SerializedProperty branchesProp = listProp.FindPropertyRelative("m_Branches");
            SerializedProperty referencesProp = listProp.FindPropertyRelative("m_References");

            if (nameProp == null || branchesProp == null || referencesProp == null)
            {
                return new VisualElement();
            }

            string listName = string.IsNullOrEmpty(nameProp.stringValue)
                ? $"Condition List {index + 1}"
                : nameProp.stringValue;

            Block block = (Block)target;
            BlockBox contentBox = new BlockBox(listProp, block)
            {
                Title = listName,
                IsExpanded = isExpanded
            };

            if (isLast)
            {
                contentBox.AddToClassList("gc-content-box-last");
            }

            contentBox.SetupContextMenu(() =>
            {
                serializedObject.Update();
                SerializedProperty listArrayProp = serializedObject.FindProperty("m_BranchLists");
                listArrayProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                RefreshList(-1, -1, -1, null);
            });

            contentBox.SetupDragHandling((fromIndex, toIndex, sourceListType) =>
            {
                ReorderLists(fromIndex, toIndex, sourceListType);
            });

            PropertyField branchesField = new PropertyField(branchesProp, "Branches");

            List<VisualElement> referenceElements = new List<VisualElement>();
            VisualElement spacer = new VisualElement { style = { height = 2 } };
            VisualElement addReferenceContainer = new VisualElement();
            addReferenceContainer.AddToClassList("gc-add-reference-container");

            void AddReferences()
            {
                foreach (var element in referenceElements)
                {
                    if (contentBox.Content.Contains(element))
                    {
                        contentBox.Content.Remove(element);
                    }
                }
                referenceElements.Clear();

                serializedObject.Update();
                for (int i = 0; i < referencesProp.arraySize; i++)
                {
                    int currentIndex = i;
                    SerializedProperty referenceProp = referencesProp.GetArrayElementAtIndex(i);
                    SerializedProperty typeProp = referenceProp.FindPropertyRelative("m_Type");

                    if (typeProp == null)
                    {
                        Debug.LogWarning($"Reference at index {i} has null type property");
                        continue;
                    }

                    var refType = (ReferenceWrapper.ReferenceType)typeProp.enumValueIndex;
                    string refName = $"Reference {i + 1}";
                    IIcon headerIcon = ReferenceUtility.GetIconForReferenceType(refType);
                    SerializedProperty activeRefProp = ReferenceUtility.GetActiveReferenceProperty(referenceProp);

                    if (activeRefProp?.boxedValue == null)
                    {
                        ReferenceUtility.InitializeReference(referenceProp, refType, refName);
                        serializedObject.ApplyModifiedProperties();
                        activeRefProp = ReferenceUtility.GetActiveReferenceProperty(referenceProp);
                    }

                    if (activeRefProp?.boxedValue != null)
                    {
                        SerializedProperty refNameProp = activeRefProp.FindPropertyRelative("m_Name");
                        if (refNameProp != null)
                        {
                            refName = string.IsNullOrEmpty(refNameProp.stringValue) ? $"Reference {i + 1}" : refNameProp.stringValue;
                        }
                    }

                    ReferenceBox refBox = new ReferenceBox(refName, false, headerIcon);
                    refBox.style.marginTop = -1;
                    refBox.AddToClassList("gc-content-box");

                    VisualElement refContainer = new VisualElement
                    {
                        style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
                    };

                    TextField nameField = new TextField("Name")
                    {
                        value = refName,
                        style = { flexGrow = 1, marginRight = 5 }
                    };
                    if (activeRefProp?.boxedValue != null)
                    {
                        nameField.BindProperty(activeRefProp.FindPropertyRelative("m_Name"));
                    }

                    EnumField typeField = new EnumField("Type", refType)
                    {
                        style = { flexGrow = 1, marginRight = 5 }
                    };
                    typeField.BindProperty(typeProp);

                    PropertyField valueField = new PropertyField
                    {
                        style = { flexGrow = 1 }
                    };
                    ReferenceUtility.BindValueField(valueField, activeRefProp, refType);

                    Button deleteButton = new Button(() =>
                    {
                        serializedObject.Update();
                        if (currentIndex >= 0 && currentIndex < referencesProp.arraySize)
                        {
                            referencesProp.DeleteArrayElementAtIndex(currentIndex);
                            serializedObject.ApplyModifiedProperties();
                            RefreshList(-1, -1, index, "Branch");
                        }
                    })
                    {
                        text = "Delete"
                    };
                    deleteButton.AddToClassList("gc-add-button");

                    refContainer.Add(deleteButton);
                    refBox.Content.Add(refContainer);
                    contentBox.Content.Add(refBox);
                    referenceElements.Add(refBox);
                }

                contentBox.Content.Add(spacer);
                addReferenceContainer.Clear();
                Button buttonAddReference = CreateAddButton("Add Reference...", ICON_REFERENCE, () =>
                {
                    addReferenceContainer.Clear();
                    addReferenceContainer.Add(CreateReferenceInputUI(referencesProp, addReferenceContainer, "Branch", index));
                });
                buttonAddReference.AddToClassList("gc-add-button");
                addReferenceContainer.Add(buttonAddReference);
                contentBox.Content.Add(addReferenceContainer);
                referenceElements.Add(spacer);
                referenceElements.Add(addReferenceContainer);
            }

            void BindAndAddBranchesField()
            {
                if (!contentBox.IsExpanded) return;
                if (!contentBox.Content.Contains(branchesField))
                {
                    branchesField.Bind(serializedObject);
                    contentBox.Content.Insert(1, new SpaceSmall());
                    contentBox.Content.Insert(1, branchesField);
                }
                AddReferences();
            }

            if (isExpanded)
            {
                BindAndAddBranchesField();
            }

            contentBox.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue)
                {
                    BindAndAddBranchesField();
                }
            });

            return contentBox;
        }

        private void ReorderLists(int fromIndex, int toIndex, string sourceListType)
        {
            serializedObject.Update();
            SerializedProperty sourceListProp = sourceListType == "Instruction"
                ? serializedObject.FindProperty("m_ActionLists")
                : serializedObject.FindProperty("m_BranchLists");

            if (fromIndex >= 0 && fromIndex < sourceListProp.arraySize &&
                toIndex >= 0 && toIndex < sourceListProp.arraySize)
            {
                sourceListProp.MoveArrayElement(fromIndex, toIndex);
                serializedObject.ApplyModifiedProperties();
                RefreshList(-1, -1, -1, null);
            }
        }
    }
}