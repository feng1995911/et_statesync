using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Fullscreen.LogicBlock.Runtime;
using GameCreator.Runtime.Common;
using GameCreator.Editor.Common;

namespace Fullscreen.LogicBlock.Editor
{
    public sealed class BlockBox : VisualElement
    {
        private const string USS_PATH = "Assets/Plugins/Fullscreen/LogicBlock/Editor/StyleSheets/BlockBox";
        private const string TIP_EDIT = "Edit";
        private const string TIP_DUPLICATE = "Duplicate";
        private const string TIP_DELETE = "Delete";
        private const string CLASS_BODY_ACTIVE = "gc-content-box-active";
        private const string NAME_DROP_ABOVE = "gc-content-box-drop-above";
        private const string NAME_DROP_BELOW = "gc-content-box-drop-below";

        private readonly Block m_Block;
        private readonly SerializedProperty m_Property;
        private readonly SerializedObject m_SerializedObject;
        private readonly string m_BoxType;

        private static readonly IIcon ICON_ARR_D = new IconArrowDropDown(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_ARR_R = new IconArrowDropRight(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_INSTRUCTIONS = new IconInstructions(ColorTheme.Type.Blue);
        private static readonly IIcon ICON_CONDITIONS = new IconConditions(ColorTheme.Type.Green);
        private static readonly IIcon ICON_GAMEOBJECT = new IconCubeSolid(ColorTheme.Type.Blue);
        private static IIcon ICON_PLAY;
        private static readonly IIcon ICON_DELETE = new IconMinus(ColorTheme.Type.TextNormal);
        private static readonly IIcon ICON_RENAME = new IconEdit(ColorTheme.Type.TextNormal);
        private static readonly IIcon ICON_DUPLICATE = new IconDuplicate(ColorTheme.Type.TextNormal);
        private static readonly IIcon ICON_SORT = new IconDrag(ColorTheme.Type.TextLight);

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private VisualElement m_Head;
        [NonSerialized] private VisualElement m_Body;
        [NonSerialized] private VisualElement m_ButtonContainer;
        [NonSerialized] private Button m_ButtonPlay;
        [NonSerialized] private Button m_ButtonRename;
        [NonSerialized] private Button m_ButtonDuplicate;
        [NonSerialized] private Button m_ButtonDelete;
        [NonSerialized] private Button m_ButtonSort;
        [NonSerialized] private Image m_HeadIcon;
        [NonSerialized] private Label m_HeadLabel;
        [NonSerialized] private TextField m_NameField;
        [NonSerialized] private Button m_SaveButton;
        [NonSerialized] private bool m_IsRenaming;
        [NonSerialized] private VisualElement m_IconDropdownContainer;
        [NonSerialized] private VisualElement m_IconDropdownButton;
        [NonSerialized] private ListView m_IconDropdownList;
        [NonSerialized] private VisualElement m_RenameContainer;

        [NonSerialized] private bool m_IsDragging;
        [NonSerialized] private Vector2 m_DragStartPosition;
        [NonSerialized] private int m_OriginalIndex;
        [NonSerialized] private Action<int, int, string> m_OnReorder;
        [NonSerialized] private VisualElement m_DropAbove;
        [NonSerialized] private VisualElement m_DropBelow;
        [NonSerialized] private string m_ListType;

        // PROPERTIES: ----------------------------------------------------------------------------

        public string Title
        {
            get => this.m_HeadLabel.text;
            set => this.m_HeadLabel.text = value;
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => this._isExpanded;
            set
            {
                if (this._isExpanded == value) return;

                bool oldValue = this._isExpanded;
                this._isExpanded = value;

                this.m_Body.style.display = value
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                this.Refresh();

                using (ChangeEvent<bool> changeEvent = ChangeEvent<bool>.GetPooled(oldValue, value))
                {
                    changeEvent.target = this;
                    this.SendEvent(changeEvent);
                }
            }
        }

        public VisualElement Content => this.m_Body;

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public BlockBox(SerializedProperty property, Block block, string boxType = "List", string title = null)
        {
            m_Property = property;
            m_Block = block;
            m_SerializedObject = property?.serializedObject;
            m_BoxType = boxType;
            m_ListType = boxType == "List" ? (property.propertyPath.Contains("m_ActionLists") ? "Instruction" : "Branch") : "GameObject";
            InitializeUIElements(title ?? (boxType == "List" ? m_ListType : "Reference"), false);
        }

        private void InitializeUIElements(string title, bool isExpanded)
        {
            ICON_PLAY = new IconPlay(EditorApplication.isPlaying ? ColorTheme.Type.TextNormal : ColorTheme.Type.TextLight);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            this.m_Head = new VisualElement { name = "GC-ContentBox-Head" };
            this.m_Body = new VisualElement { name = "GC-ContentBox-Body" };
            this.m_DropAbove = new VisualElement { name = "gc-content-box-drop-above" };
            this.m_DropBelow = new VisualElement { name = "gc-content-box-drop-below" };

            this.m_Body.AddToClassList(AlignLabel.CLASS_UNITY_MAIN_CONTAINER);
            this.m_Body.AddToClassList(AlignLabel.CLASS_UNITY_INSPECTOR_ELEMENT);

            StyleSheet[] sheets = StyleSheetUtils.Load(USS_PATH);
            foreach (StyleSheet sheet in sheets) this.styleSheets.Add(sheet);

            this.Add(this.m_DropAbove);
            this.Add(this.m_Head);
            this.Add(this.m_Body);
            this.Add(this.m_DropBelow);

            this.m_Head.RegisterCallback<MouseEnterEvent>(evt => m_Head.AddToClassList("gc-hovered"));
            this.m_Head.RegisterCallback<MouseLeaveEvent>(evt => m_Head.RemoveFromClassList("gc-hovered"));

            this.m_ButtonSort = CreateButton(ICON_SORT.Texture, null, "Sort");
            this.m_Head.Add(this.m_ButtonSort);

            string iconTypeName = "DefaultIcon";
            ColorTheme.Type iconColor = ColorTheme.Type.TextNormal;

            if (m_BoxType == "List" && m_Property != null)
            {
                SerializedProperty iconTypeProp = m_Property.FindPropertyRelative("m_IconTypeName");
                SerializedProperty iconColorProp = m_Property.FindPropertyRelative("m_IconColor");
                if (iconTypeProp != null && iconColorProp != null)
                {
                    iconTypeName = iconTypeProp.stringValue;
                    iconColor = (ColorTheme.Type)iconColorProp.enumValueIndex;
                }
            }

            IIcon headerIcon = GetHeaderIcon(iconTypeName, iconColor);

            this.m_HeadIcon = new Image
            {
                image = headerIcon.Texture,
                style = { width = 16, height = 16, marginTop = 2, marginLeft = 8 }
            };
            this.m_Head.Add(this.m_HeadIcon);

            this.m_HeadLabel = new Label
            {
                style = { marginLeft = 4 }
            };
            this.m_Head.Add(this.m_HeadLabel);

            this.m_ButtonContainer = new VisualElement
            {
                name = "ButtonContainer",
                style = { flexDirection = FlexDirection.Row, marginLeft = StyleKeyword.Auto }
            };
            this.m_ButtonContainer.AddToClassList("gc-button-container");

            m_RenameContainer = new VisualElement
            {
                style = { 
                    flexDirection = FlexDirection.Row, 
                    display = DisplayStyle.None,
                    marginLeft = 8,
                    marginTop = 4,
                    marginBottom = 3,
                    flexWrap = Wrap.Wrap
                }
            };

            var chooseIconButton = new Button
            {
                text = "Choose Icon",
                style = { 
                    width = 100, 
                    marginLeft = 8 
                }
            };
            chooseIconButton.AddToClassList("unity-button");
            chooseIconButton.clicked += () =>
            {
                IconSelectorWindow.ShowWindow(GetListId(), selectedInfo =>
                {
                    if (selectedInfo != null)
                    {
                        UpdateHeaderIcon(selectedInfo);
                        if (m_BoxType == "List" && m_Property != null)
                        {
                            SerializedProperty iconTypeProp = m_Property.FindPropertyRelative("m_IconTypeName");
                            SerializedProperty iconColorProp = m_Property.FindPropertyRelative("m_IconColor");
                            if (iconTypeProp != null && iconColorProp != null)
                            {
                                iconTypeProp.stringValue = selectedInfo.ImageInfo.IconType.Name;
                                iconColorProp.enumValueIndex = (int)selectedInfo.Color;
                                m_SerializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                });
            };

            if (m_BoxType == "List")
            {
                this.m_ButtonPlay = CreateButton(ICON_PLAY.Texture, RunInstructions, "Play");
            }
            this.m_ButtonRename = CreateButton(ICON_RENAME.Texture, RenameAction, "Rename");
            this.m_ButtonDuplicate = CreateButton(ICON_DUPLICATE.Texture, DuplicateAction, "Duplicate");
            this.m_ButtonDelete = CreateButton(ICON_DELETE.Texture, DeleteAction, "Delete");
            this.m_ButtonRename.tooltip = TIP_EDIT;
            this.m_ButtonDuplicate.tooltip = TIP_DUPLICATE;
            this.m_ButtonDelete.tooltip = TIP_DELETE;

            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            this.m_ButtonContainer.Add(spacer);

            if (m_BoxType == "List")
            {
                AddButtonToContainer(this.m_ButtonPlay, isFirst: true);
            }
            AddButtonToContainer(this.m_ButtonRename, isFirst: m_BoxType != "List");
            AddButtonToContainer(this.m_ButtonDuplicate);
            AddButtonToContainer(this.m_ButtonDelete);

            this.m_Head.Add(this.m_ButtonContainer);

            m_NameField = new TextField
            {
                name = "RenameField",
                style = { width = 150 }
            };
            m_NameField.AddToClassList("unity-text-field");

            m_SaveButton = new Button(OnSaveRename)
            {
                name = "SaveButton",
                text = "Save",
                style = { width = 60, marginLeft = 8 }
            };
            m_SaveButton.AddToClassList("unity-button");

            m_RenameContainer.Add(m_NameField);
            m_RenameContainer.Add(m_SaveButton);
            m_RenameContainer.Add(chooseIconButton);
            this.m_Body.Insert(0, m_RenameContainer);

            this.m_Head.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0 &&
                    (evt.target == this.m_Head ||
                    evt.target == this.m_HeadLabel))
                {
                    this.IsExpanded = !this.IsExpanded;
                    evt.StopPropagation();
                }
            }, TrickleDown.TrickleDown);

            this.m_ButtonSort.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                m_IsDragging = true;
                m_DragStartPosition = evt.mousePosition;
                m_OriginalIndex = GetCurrentListIndex();
                this.m_ButtonSort.CaptureMouse();
                UpdateDragVisuals(m_OriginalIndex, -1);
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);

            this.m_ButtonSort.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            this.m_ButtonSort.RegisterCallback<MouseUpEvent>(OnMouseUp);

            this.m_DropAbove.style.display = DisplayStyle.None;
            this.m_DropBelow.style.display = DisplayStyle.None;

            this.Title = title;
            this._isExpanded = isExpanded;
            this.Refresh();
        }

        private IIcon GetHeaderIcon(string iconTypeName, ColorTheme.Type color)
        {
            var iconInfo = IconRegistry.GetAvailableIcons().Find(info => info.IconType.Name == iconTypeName);
            if (iconInfo != null)
            {
                try
                {
                    var constructor = iconInfo.IconType.GetConstructor(new[] { typeof(ColorTheme.Type), typeof(IIcon) });
                    if (constructor != null)
                    {
                        return (IIcon)constructor.Invoke(new object[] { color, null });
                    }

                    constructor = iconInfo.IconType.GetConstructor(new[] { typeof(Color), typeof(IIcon) });
                    if (constructor != null)
                    {
                        return (IIcon)constructor.Invoke(new object[] { ColorTheme.Get(color), null });
                    }

                    constructor = iconInfo.IconType.GetConstructor(new[] { typeof(Color) });
                    if (constructor != null)
                    {
                        return (IIcon)constructor.Invoke(new object[] { ColorTheme.Get(color) });
                    }

                    constructor = iconInfo.IconType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        return (IIcon)constructor.Invoke(null);
                    }
                }
                catch (Exception)
                {
                }
            }
            return GetDefaultIcon();
        }

        private void UpdateHeaderIcon(IconSelectorWindow.IconInfo iconInfo)
        {
            IIcon icon = GetHeaderIcon(iconInfo.ImageInfo.IconType.Name, iconInfo.Color);
            if (m_HeadIcon != null)
            {
                Texture2D texture = icon?.Texture ?? iconInfo.ImageInfo.Preview;
                if (texture != null)
                {
                    m_HeadIcon.image = texture;
                    m_HeadIcon.MarkDirtyRepaint();
                }
            }
        }

        private IIcon GetDefaultIcon()
        {
            return m_BoxType == "List"
                ? (m_ListType == "Instruction" ? ICON_INSTRUCTIONS : ICON_CONDITIONS)
                : ICON_GAMEOBJECT;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            ICON_PLAY = new IconPlay(EditorApplication.isPlaying ? ColorTheme.Type.TextNormal : ColorTheme.Type.TextLight);
            if (m_ButtonPlay != null)
            {
                foreach (var image in m_ButtonPlay.Query<Image>().ToList())
                {
                    image.image = ICON_PLAY.Texture;
                }
            }
        }

        ~BlockBox()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private Action deleteAction;

        public void SetupContextMenu(Action deleteAction)
        {
            this.deleteAction = deleteAction;
        }

        public void SetupDragHandling(Action<int, int, string> onReorder)
        {
            this.m_OnReorder = onReorder;
        }

        public void DisplayAsNormal()
        {
            this.m_Head.style.opacity = 1f;
            this.m_Body.style.opacity = 1f;
            this.m_DropAbove.style.display = DisplayStyle.None;
            this.m_DropBelow.style.display = DisplayStyle.None;
        }

        public void DisplayAsDrag()
        {
            this.m_Head.style.opacity = 0.25f;
            this.m_Body.style.opacity = 0.25f;
            this.m_DropAbove.style.display = DisplayStyle.None;
            this.m_DropBelow.style.display = DisplayStyle.None;
        }

        public void DisplayAsTargetAbove()
        {
            this.m_DropAbove.style.display = DisplayStyle.Flex;
            this.m_DropBelow.style.display = DisplayStyle.None;
        }

        public void DisplayAsTargetBelow()
        {
            this.m_DropAbove.style.display = DisplayStyle.None;
            this.m_DropBelow.style.display = DisplayStyle.Flex;
        }

        public string GetListType()
        {
            return m_ListType;
        }

        private void AddButtonToContainer(Button button, bool isFirst = false)
        {
            if (button == null) return;
            if (isFirst)
            {
                button.AddToClassList("gc-content-box-button-first");
            }
            this.m_ButtonContainer.Add(button);
        }

        private Button CreateButton(Texture2D icon, Action clickAction, string buttonName)
        {
            Button button = new Button(clickAction);
            button.name = $"Button{buttonName}";
            button.AddToClassList("gc-content-box-button");

            button.Add(new Image
            {
                image = icon,
                style = { width = 16, height = 16, marginLeft = 3, marginTop = 2 }
            });

            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                button.AddToClassList("gc-button-hovered");
                m_Head.AddToClassList("gc-button-hovered");
                m_Head.RemoveFromClassList("gc-hovered");
            });

            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                button.RemoveFromClassList("gc-button-hovered");
                m_Head.RemoveFromClassList("gc-button-hovered");
                m_Head.AddToClassList("gc-hovered");
            });

            return button;
        }

        private void RunInstructions()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode || m_BoxType != "List" || m_Block == null)
            {
                return;
            }

            int index = GetCurrentListIndex();
            string listName = null;

            if (m_ListType == "Instruction")
            {
                if (index >= 0 && index < m_Block.ActionLists.Count)
                {
                    listName = m_Block.ActionLists[index].ListName;
                }
            }
            else if (m_ListType == "Branch")
            {
                if (index >= 0 && index < m_Block.BranchLists.Count)
                {
                    listName = m_Block.BranchLists[index].ListName;
                }
            }

            if (!string.IsNullOrEmpty(listName))
            {
                Args args = new Args((GameObject)null);
                m_Block.Execute(listName, args);
            }
        }

        private void DeleteAction()
        {
            if (EditorUtility.DisplayDialog(
                "Confirm Delete",
                $"Are you sure you want to delete '{this.Title}'?",
                "Yes",
                "No"))
            {
                deleteAction?.Invoke();
            }
        }

        private void RenameAction()
        {
            if (m_IsRenaming) return;

            m_IsRenaming = true;
            this.IsExpanded = true;

            m_NameField.value = this.Title;
            m_RenameContainer.style.display = DisplayStyle.Flex;
            m_NameField.Focus();
        }

        private void OnSaveRename()
        {
            if (!m_IsRenaming) return;

            string newInputName = m_NameField.value.Trim();
            string baseName = string.IsNullOrEmpty(newInputName)
                ? $"{(m_BoxType == "List" ? m_ListType : "Reference")} {GetCurrentListIndex() + 1}"
                : newInputName;

            if (baseName == this.Title)
            {
                m_RenameContainer.style.display = DisplayStyle.None;
                m_IsRenaming = false;
                return;
            }

            string newName = baseName;
            if (m_Property != null)
            {
                SerializedProperty listProp = m_BoxType == "List"
                    ? (m_ListType == "Instruction"
                        ? m_SerializedObject.FindProperty("m_ActionLists")
                        : m_SerializedObject.FindProperty("m_BranchLists"))
                    : GetParentProperty(m_Property);
                newName = GenerateUniqueName(baseName, listProp);

                SerializedProperty nameProp = m_Property.FindPropertyRelative(m_BoxType == "List" ? "m_ListName" : "m_Name");
                if (nameProp != null)
                {
                    nameProp.stringValue = newName;
                    nameProp.serializedObject.ApplyModifiedProperties();
                }
            }

            this.Title = newName;

            m_RenameContainer.style.display = DisplayStyle.None;
            m_IsRenaming = false;
        }

        private void DuplicateAction()
        {
            if (m_Property == null || m_SerializedObject == null)
            {
                return;
            }

            int currentIndex = GetCurrentListIndex();
            if (currentIndex < 0)
            {
                return;
            }

            SerializedProperty parentListProp = m_BoxType == "List"
                ? (m_ListType == "Instruction"
                    ? m_SerializedObject.FindProperty("m_ActionLists")
                    : m_SerializedObject.FindProperty("m_BranchLists"))
                : GetParentProperty(m_Property);

            if (parentListProp == null)
            {
                return;
            }

            m_SerializedObject.Update();
            parentListProp.InsertArrayElementAtIndex(currentIndex);
            SerializedProperty newElement = parentListProp.GetArrayElementAtIndex(currentIndex + 1);
            SerializedProperty currentElement = parentListProp.GetArrayElementAtIndex(currentIndex);

            if (newElement == null || currentElement == null)
            {
                return;
            }

            string newListId = null;
            string newListName = null;

            SerializedProperty newNameProp = null;
            SerializedProperty currentNameProp = null;

            if (m_BoxType == "List")
            {
                newNameProp = newElement.FindPropertyRelative("m_ListName");
                currentNameProp = currentElement.FindPropertyRelative("m_ListName");
                if (newNameProp == null || currentNameProp == null)
                {
                    return;
                }
                newNameProp.stringValue = GenerateUniqueName(currentNameProp.stringValue, parentListProp);
                newListName = newNameProp.stringValue;

                SerializedProperty newIconTypeProp = newElement.FindPropertyRelative("m_IconTypeName");
                SerializedProperty newIconColorProp = newElement.FindPropertyRelative("m_IconColor");
                SerializedProperty currentIconTypeProp = currentElement.FindPropertyRelative("m_IconTypeName");
                SerializedProperty currentIconColorProp = currentElement.FindPropertyRelative("m_IconColor");
                if (newIconTypeProp != null && newIconColorProp != null && currentIconTypeProp != null && currentIconColorProp != null)
                {
                    newIconTypeProp.stringValue = currentIconTypeProp.stringValue;
                    newIconColorProp.enumValueIndex = currentIconColorProp.enumValueIndex;
                }

                string[] possibleIdFields = { "m_UniqueId", "UniqueId", "m_Id", "uniqueId" };
                SerializedProperty uniqueIdProp = null;
                foreach (var idField in possibleIdFields)
                {
                    uniqueIdProp = newElement.FindPropertyRelative(idField);
                    if (uniqueIdProp != null && uniqueIdProp.propertyType == SerializedPropertyType.String)
                    {
                        break;
                    }
                }
                if (uniqueIdProp != null && uniqueIdProp.propertyType == SerializedPropertyType.String)
                {
                    newListId = System.Guid.NewGuid().ToString();
                    uniqueIdProp.stringValue = newListId;
                }

                string fieldName = m_ListType == "Instruction" ? "m_Instructions" : "m_Branches";
                SerializedProperty newItemsProp = newElement.FindPropertyRelative(fieldName);
                SerializedProperty currentItemsProp = currentElement.FindPropertyRelative(fieldName);

                if (newItemsProp != null && currentItemsProp != null)
                {
                    string itemsArrayName = m_ListType == "Instruction" ? "m_Instructions" : "m_Branches";
                    SerializedProperty newItemsArray = newItemsProp.FindPropertyRelative(itemsArrayName);
                    SerializedProperty currentItemsArray = currentItemsProp.FindPropertyRelative(itemsArrayName);

                    if (newItemsArray != null && currentItemsArray != null && newItemsArray.isArray && currentItemsArray.isArray)
                    {
                        newItemsArray.arraySize = currentItemsArray.arraySize;

                        for (int i = 0; i < currentItemsArray.arraySize; i++)
                        {
                            SerializedProperty currentItem = currentItemsArray.GetArrayElementAtIndex(i);
                            SerializedProperty newItem = newItemsArray.GetArrayElementAtIndex(i);

                            if (currentItem.propertyType == SerializedPropertyType.ManagedReference && currentItem.managedReferenceValue != null)
                            {
                                string serializedData = JsonUtility.ToJson(currentItem.managedReferenceValue);
                                object newInstance = JsonUtility.FromJson(serializedData, currentItem.managedReferenceValue.GetType());
                                newItem.managedReferenceValue = newInstance;
                            }
                            else
                            {
                                newItem.managedReferenceValue = null;
                            }
                        }

                        if (m_ListType == "Instruction" && newItemsArray.arraySize > 0 && !string.IsNullOrEmpty(newListId))
                        {
                            UpdateBlockInstructionReferences(newItemsArray, newListId, newListName);
                        }
                    }
                }

                SerializedProperty newRefsProp = newElement.FindPropertyRelative("m_References");
                SerializedProperty currentRefsProp = currentElement.FindPropertyRelative("m_References");
                if (newRefsProp != null && currentRefsProp != null)
                {
                    newRefsProp.arraySize = currentRefsProp.arraySize;
                    for (int i = 0; i < currentRefsProp.arraySize; i++)
                    {
                        SerializedProperty currentRef = currentRefsProp.GetArrayElementAtIndex(i);
                        SerializedProperty newRef = newRefsProp.GetArrayElementAtIndex(i);
                        if (currentRef != null && newRef != null)
                        {
                            SerializedProperty currentTypeProp = currentRef.FindPropertyRelative("m_Type");
                            SerializedProperty newTypeProp = newRef.FindPropertyRelative("m_Type");
                            if (currentTypeProp != null && newTypeProp != null)
                            {
                                newTypeProp.enumValueIndex = currentTypeProp.enumValueIndex;
                            }

                            SerializedProperty currentActiveRefProp = ReferenceUtility.GetActiveReferenceProperty(currentRef);
                            SerializedProperty newActiveRefProp = ReferenceUtility.GetActiveReferenceProperty(newRef);
                            if (currentActiveRefProp != null && newActiveRefProp != null)
                            {
                                SerializedProperty currentRefNameProp = currentActiveRefProp.FindPropertyRelative("m_Name");
                                SerializedProperty newRefNameProp = newActiveRefProp.FindPropertyRelative("m_Name");
                                if (currentRefNameProp != null && newRefNameProp != null)
                                {
                                    newRefNameProp.stringValue = GenerateUniqueName(currentRefNameProp.stringValue, newRefsProp);
                                }
                            }

                            SerializedProperty typeProp = newRef.FindPropertyRelative("m_Type");
                            SerializedProperty nameProp = newActiveRefProp?.FindPropertyRelative("m_Name");
                            if (typeProp != null && nameProp != null)
                            {
                                ReferenceUtility.InitializeReference(newRef, (ReferenceWrapper.ReferenceType)typeProp.enumValueIndex, nameProp.stringValue);
                            }
                        }
                    }
                }
            }
            else
            {
                SerializedProperty newRefNameProp = newElement.FindPropertyRelative("m_Name");
                SerializedProperty currentRefNameProp = currentElement.FindPropertyRelative("m_Name");
                if (newRefNameProp != null && currentRefNameProp != null)
                {
                    newRefNameProp.stringValue = GenerateUniqueName(currentRefNameProp.stringValue, parentListProp);
                }
                else
                {
                    return;
                }

                SerializedProperty newGameObjectProp = newElement.FindPropertyRelative("m_GameObject");
                SerializedProperty currentGameObjectProp = currentElement.FindPropertyRelative("m_GameObject");
                if (newGameObjectProp != null && currentGameObjectProp != null)
                {
                    newGameObjectProp.objectReferenceValue = currentGameObjectProp.objectReferenceValue;
                }
                else
                {
                    return;
                }
            }

            m_SerializedObject.ApplyModifiedProperties();

            BlockEditor blockEditor = null;
            UnityEditor.Editor[] editors = Resources.FindObjectsOfTypeAll<UnityEditor.Editor>();
            foreach (var editor in editors)
            {
                if (editor is BlockEditor abe && abe.target == m_Block)
                {
                    blockEditor = abe;
                    break;
                }
            }

            if (blockEditor != null)
            {
                blockEditor.serializedObject.Update();
                blockEditor.RefreshList(
                    m_ListType == "Instruction" ? currentIndex + 1 : -1,
                    m_ListType == "Branch" ? currentIndex + 1 : -1,
                    currentIndex + 1,
                    m_ListType
                );
            }
            else
            {
                BlockEditor newEditor = UnityEditor.Editor.CreateEditor(m_Block, typeof(BlockEditor)) as BlockEditor;
                if (newEditor != null)
                {
                    newEditor.serializedObject.Update();
                    newEditor.RefreshList(
                        m_ListType == "Instruction" ? currentIndex + 1 : -1,
                        m_ListType == "Branch" ? currentIndex + 1 : -1,
                        currentIndex + 1,
                        m_ListType
                    );
                    UnityEngine.Object.DestroyImmediate(newEditor);
                }
            }
        }

        private void UpdateBlockInstructionReferences(SerializedProperty instructionsArray, string newListId, string newListName)
        {
            for (int i = 0; i < instructionsArray.arraySize; i++)
            {
                SerializedProperty instructionProp = instructionsArray.GetArrayElementAtIndex(i);
                if (instructionProp.propertyType != SerializedPropertyType.ManagedReference || instructionProp.managedReferenceValue == null)
                {
                    continue;
                }

                object instruction = instructionProp.managedReferenceValue;
                Type instructionType = instruction.GetType();

                foreach (var field in instructionType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    object fieldValue = field.GetValue(instruction);

                    if (fieldValue == null)
                    {
                        continue;
                    }

                    if (fieldValue.GetType().Name == "PropertyGetGameObject")
                    {
                        SerializedProperty fieldProp = instructionProp.FindPropertyRelative(field.Name);
                        if (fieldProp != null)
                        {
                            var iterator = fieldProp.Copy();
                            bool enterChildren = true;
                            while (iterator.NextVisible(enterChildren))
                            {
                                enterChildren = false;
                            }

                            string[] possibleValueFields = { "m_Value", "m_Property", "Value", "property" };
                            SerializedProperty valueProp = null;
                            foreach (var valueField in possibleValueFields)
                            {
                                valueProp = fieldProp.FindPropertyRelative(valueField);
                                if (valueProp != null)
                                {
                                    break;
                                }
                            }

                            if (valueProp != null && valueProp.propertyType == SerializedPropertyType.ManagedReference)
                            {
                                if (valueProp.managedReferenceValue?.GetType().Name == "BlockGetGameObjectReference")
                                {
                                    SerializedProperty listReferenceProp = valueProp.FindPropertyRelative("m_ListReference");
                                    if (listReferenceProp != null)
                                    {
                                        SerializedProperty instructionListIdProp = listReferenceProp.FindPropertyRelative("m_InstructionListId");
                                        if (instructionListIdProp != null && instructionListIdProp.propertyType == SerializedPropertyType.String)
                                        {
                                            instructionListIdProp.stringValue = newListId;
                                        }

                                        SerializedProperty referenceNameProp = listReferenceProp.FindPropertyRelative("m_ReferenceName");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            instructionsArray.serializedObject.ApplyModifiedProperties();
        }

        private string GenerateUniqueName(string baseName, SerializedProperty listsProp)
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
            for (int i = 0; i < listsProp.arraySize; i++)
            {
                SerializedProperty nameProp = listsProp.GetArrayElementAtIndex(i).FindPropertyRelative(m_BoxType == "List" ? "m_ListName" : "m_Name");
                if (nameProp != null)
                    existingNames.Add(nameProp.stringValue);
            }

            if (!existingNames.Contains(baseName))
                return baseName;

            int number = existingNumber;
            string candidateName;
            do
            {
                number++;
                candidateName = $"{cleanBaseName}({number})";
            } while (existingNames.Contains(candidateName));

            return candidateName;
        }

        private int GetCurrentListIndex()
        {
            if (m_Property == null)
            {
                return -1;
            }

            string path = m_Property.propertyPath;
            int startIndex = path.LastIndexOf("[") + 1;
            int endIndex = path.IndexOf("]", startIndex);
            if (startIndex > 0 && endIndex > startIndex)
            {
                string indexStr = path.Substring(startIndex, endIndex - startIndex);
                if (int.TryParse(indexStr, out int index))
                {
                    return index;
                }
            }
            return -1;
        }

        private int GetParentListSize()
        {
            if (m_SerializedObject == null) return 0;
            SerializedProperty listProp = m_BoxType == "List"
                ? (m_ListType == "Instruction"
                    ? m_SerializedObject.FindProperty("m_ActionLists")
                    : m_SerializedObject.FindProperty("m_BranchLists"))
                : GetParentProperty(m_Property);
            return listProp?.arraySize ?? 0;
        }

        private string GetListId()
        {
            if (m_Property == null) return "default_" + Guid.NewGuid().ToString();

            string[] possibleIdFields = { "m_UniqueId", "UniqueId", "m_Id", "uniqueId" };
            foreach (var idField in possibleIdFields)
            {
                SerializedProperty idProp = m_Property.FindPropertyRelative(idField);
                if (idProp != null && idProp.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(idProp.stringValue))
                {
                    return idProp.stringValue;
                }
            }
            return m_BoxType + "_" + GetCurrentListIndex();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!m_IsDragging) return;

            int targetIndex = FindTargetIndex(evt.mousePosition);
            UpdateDragVisuals(m_OriginalIndex, targetIndex);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!m_IsDragging) return;
            m_IsDragging = false;
            this.m_ButtonSort.ReleaseMouse();

            int targetIndex = FindTargetIndex(evt.mousePosition);
            if (targetIndex != -1 && targetIndex != m_OriginalIndex)
            {
                m_OnReorder?.Invoke(m_OriginalIndex, targetIndex, m_ListType);
            }

            ResetDragVisuals();
            evt.StopPropagation();
        }

        private int FindTargetIndex(Vector2 pointerPosition)
        {
            VisualElement container = this.parent;
            if (container == null) return -1;

            int currentIndex = GetCurrentListIndex();
            int targetIndex = currentIndex;

            for (int i = 0; i < container.childCount; i++)
            {
                VisualElement child = container.ElementAt(i);
                if (!(child is BlockBox item) || item.GetListType() != m_ListType) continue;

                Rect childRect = child.worldBound;
                if (pointerPosition.y < childRect.yMin + childRect.height / 2)
                {
                    targetIndex = item.GetCurrentListIndex();
                    if (targetIndex > currentIndex) targetIndex--;
                    break;
                }
                else if (pointerPosition.y < childRect.yMax)
                {
                    targetIndex = item.GetCurrentListIndex();
                    if (targetIndex < currentIndex) targetIndex++;
                }
            }

            return Mathf.Clamp(targetIndex, 0, GetParentListSize() - 1);
        }

        private void UpdateDragVisuals(int draggedIndex, int targetIndex)
        {
            VisualElement container = this.parent;
            if (container == null) return;

            for (int i = 0; i < container.childCount; i++)
            {
                VisualElement child = container.ElementAt(i);
                if (!(child is BlockBox item) || item.GetListType() != m_ListType) continue;

                int itemIndex = item.GetCurrentListIndex();
                if (itemIndex == draggedIndex)
                {
                    item.DisplayAsDrag();
                }
                else if (itemIndex == targetIndex)
                {
                    if (targetIndex < draggedIndex || (targetIndex == draggedIndex - 1 && draggedIndex == GetParentListSize() - 1))
                    {
                        item.DisplayAsTargetAbove();
                    }
                    else
                    {
                        item.DisplayAsTargetBelow();
                    }
                }
                else
                {
                    item.DisplayAsNormal();
                }
            }
        }

        private void ResetDragVisuals()
        {
            VisualElement container = this.parent;
            if (container == null) return;

            for (int i = 0; i < container.childCount; i++)
            {
                VisualElement child = container.ElementAt(i);
                if (child is BlockBox item && item.GetListType() == m_ListType)
                {
                    item.DisplayAsNormal();
                }
            }
        }

        private void Refresh()
        {
            if (this.IsExpanded)
            {
                this.m_Body.style.display = DisplayStyle.Flex;
                this.m_Body.AddToClassList(CLASS_BODY_ACTIVE);
            }
            else
            {
                this.m_Body.style.display = DisplayStyle.None;
                this.m_Body.RemoveFromClassList(CLASS_BODY_ACTIVE);
                if (m_IsRenaming)
                {
                    m_RenameContainer.style.display = DisplayStyle.None;
                    m_IsRenaming = false;
                }
            }
        }

        private SerializedProperty GetParentProperty(SerializedProperty property)
        {
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot == -1) return null;
            string parentPath = path.Substring(0, lastDot);
            return property.serializedObject.FindProperty(parentPath);
        }
    }
}