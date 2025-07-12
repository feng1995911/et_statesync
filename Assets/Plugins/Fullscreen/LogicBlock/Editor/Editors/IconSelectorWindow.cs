using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using GameCreator.Runtime.Common;

namespace Fullscreen.LogicBlock.Editor
{
    public class IconSelectorWindow : EditorWindow
    {
        private const float ICON_SIZE = 32f;
        private const float PADDING = 10f;
        private const float LABEL_HEIGHT = 20f;
        private const float MIN_WINDOW_WIDTH = 350f;
        private const float MIN_WINDOW_HEIGHT = 400f;

        private string _listId;
        private Action<IconInfo> _onSelect;
        private TextField _searchField;
        private EnumField _colorField;
        private VisualElement _iconGrid;
        private ScrollView _scrollView;
        private string _searchQuery = "";
        private ColorTheme.Type _selectedColor = ColorTheme.Type.TextNormal;

        public class IconInfo
        {
            public IconRegistry.IconInfo ImageInfo { get; }
            public ColorTheme.Type Color { get; }

            public IconInfo(IconRegistry.IconInfo imageInfo, ColorTheme.Type color)
            {
                ImageInfo = imageInfo;
                Color = color;
            }
        }

        public static void ShowWindow(string listId, Action<IconInfo> onSelect)
        {
            var window = GetWindow<IconSelectorWindow>("Select Icon");
            window._listId = listId;
            window._onSelect = onSelect;
            window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            _searchField = new TextField
            {
                label = "Search Icons",
                style = {
                    marginTop = 10,
                    marginBottom = PADDING
                }
            };
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _searchQuery = evt.newValue?.ToLower() ?? "";
                UpdateIconGrid();
            });
            root.Add(_searchField);

            _colorField = new EnumField("Icon Color", _selectedColor);
            _colorField.style.marginBottom = PADDING;
            _colorField.RegisterValueChangedCallback(evt =>
            {
                _selectedColor = (ColorTheme.Type)evt.newValue;
                UpdateIconGrid();
            });
            root.Add(_colorField);

            _scrollView = new ScrollView
            {
                style = { flexGrow = 1 }
            };
            root.Add(_scrollView);

            _iconGrid = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    justifyContent = Justify.SpaceEvenly
                }
            };
            _scrollView.Add(_iconGrid);

            Button cancelButton = new Button(() => Close())
            {
                text = "Cancel",
                style = { height = 30, marginTop = PADDING }
            };
            cancelButton.AddToClassList("unity-button");
            root.Add(cancelButton);

            UpdateIconGrid();
        }

        private void UpdateIconGrid()
        {
            _iconGrid.Clear();
            var iconChoices = IconRegistry.GetAvailableIcons();
            if (iconChoices == null || iconChoices.Count == 0)
            {
                Label noIconsLabel = new Label("No icons available.")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, alignSelf = Align.Center }
                };
                _iconGrid.Add(noIconsLabel);
                return;
            }

            foreach (var iconInfo in iconChoices)
            {
                if (iconInfo.DisplayName.ToLower().StartsWith("overlay"))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(_searchQuery) &&
                    !iconInfo.DisplayName.ToLower().Contains(_searchQuery))
                {
                    continue;
                }

                VisualElement iconContainer = new VisualElement
                {
                    style =
                    {
                        width = ICON_SIZE + 4,
                        alignItems = Align.Center,
                        marginBottom = 4,
                        marginRight = 4
                    }
                };

                Button iconButton = new Button(() =>
                {
                    _onSelect?.Invoke(new IconInfo(iconInfo, _selectedColor));
                    Close();
                })
                {
                    style =
                    {
                        width = ICON_SIZE,
                        height = ICON_SIZE,
                        paddingTop = 2,
                        paddingBottom = 2,
                        paddingLeft = 2,
                        paddingRight = 2,
                        justifyContent = Justify.Center,
                        alignItems = Align.Center,
                        borderTopWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                        borderTopLeftRadius = 4,
                        borderTopRightRadius = 4,
                        borderBottomLeftRadius = 4,
                        borderBottomRightRadius = 4
                    }
                };

                Image iconImage = new Image
                {
                    image = GetColoredTexture(iconInfo, _selectedColor),
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        width = ICON_SIZE * 0.7f,
                        height = ICON_SIZE * 0.7f,
                    }
                };

                iconButton.Add(iconImage);
                iconContainer.Add(iconButton);


                Label iconLabel = new Label(iconInfo.DisplayName)
                {
                    style =
                    {
                        height = StyleKeyword.Auto,
                        maxWidth = ICON_SIZE + PADDING,
                        unityTextAlign = TextAnchor.UpperCenter,
                        fontSize = 10,
                        whiteSpace = WhiteSpace.Normal,
                        overflow = Overflow.Hidden,
                        unityFontStyleAndWeight = FontStyle.Normal,
                        color = new Color(0.6f, 0.6f, 0.6f)
                    }
                };

                iconContainer.Add(iconLabel);

                _iconGrid.Add(iconContainer);
            }

            if (_iconGrid.childCount == 0)
            {
                Label noResultsLabel = new Label("No icons match the search criteria.")
                {
                    style = { alignSelf = Align.Center }
                };
                _iconGrid.Add(noResultsLabel);
            }
        }

        private Texture2D GetColoredTexture(IconRegistry.IconInfo iconInfo, ColorTheme.Type color)
        {
            try
            {
                var constructor = iconInfo.IconType.GetConstructor(new[] { typeof(ColorTheme.Type), typeof(IIcon) });
                IIcon icon = null;
                if (constructor != null)
                {
                    icon = (IIcon)constructor.Invoke(new object[] { color, null });
                }
                else
                {
                    constructor = iconInfo.IconType.GetConstructor(new[] { typeof(Color), typeof(IIcon) });
                    if (constructor != null)
                    {
                        icon = (IIcon)constructor.Invoke(new object[] { ColorTheme.Get(color), null });
                    }
                    else
                    {
                        return iconInfo.Preview;
                    }
                }

                return icon?.Texture ?? iconInfo.Preview;
            }
            catch (Exception)
            {
                return iconInfo.Preview;
            }
        }
    }
}