using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using GameCreator.Runtime.Common;
using Fullscreen.LogicBlock.Runtime;

namespace Fullscreen.LogicBlock.Editor
{
    public static class IconRegistry
    {
        public class IconInfo
        {
            public Type IconType { get; }
            public string DisplayName { get; }
            public Texture2D Preview { get; }

            public IconInfo(Type iconType, string displayName, Texture2D preview)
            {
                IconType = iconType;
                DisplayName = displayName;
                Preview = preview;
            }
        }

        private static List<IconInfo> _cachedIcons;

        public static List<IconInfo> GetAvailableIcons()
        {
            if (_cachedIcons != null && _cachedIcons.Count > 0)
            {
                return _cachedIcons;
            }

            _cachedIcons = new List<IconInfo>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var iconTypes = new List<Type>();

            foreach (var asm in assemblies)
            {
                try
                {
                    var types = asm.GetTypes()
                        .Where(t => typeof(IIcon).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                        .ToList();
                    iconTypes.AddRange(types);
                }
                catch (Exception)
                {
                }
            }

            foreach (var type in iconTypes)
            {
                try
                {
                    IIcon icon = null;

                    var colorConstructor = type.GetConstructor(new[] { typeof(Color), typeof(IIcon) });
                    if (colorConstructor != null)
                    {
                        icon = (IIcon)colorConstructor.Invoke(new object[] { Color.white, null });
                    }
                    else
                    {
                        var colorThemeConstructor = type.GetConstructor(new[] { typeof(ColorTheme.Type), typeof(IIcon) });
                        if (colorThemeConstructor != null)
                        {
                            icon = (IIcon)colorThemeConstructor.Invoke(new object[] { ColorTheme.Type.TextNormal, null });
                        }
                        else
                        {
                            var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
                            if (defaultConstructor != null)
                            {
                                icon = (IIcon)defaultConstructor.Invoke(null);
                            }
                            else
                            {
                                var colorOnlyConstructor = type.GetConstructor(new[] { typeof(Color) });
                                if (colorOnlyConstructor != null)
                                {
                                    icon = (IIcon)colorOnlyConstructor.Invoke(new object[] { Color.white });
                                }
                            }
                        }
                    }

                    if (icon == null)
                    {
                        continue;
                    }

                    if (icon.Texture == null)
                    {
                        continue;
                    }

                    string displayName = FormatIconDisplayName(type.Name);
                    Texture2D preview = icon.Texture;
                    _cachedIcons.Add(new IconInfo(type, displayName, preview));
                }
                catch (Exception)
                {
                }
            }

            if (_cachedIcons.Count == 0)
            {
                try
                {
                    var defaultIcon = new IconReference(Color.red);
                    _cachedIcons.Add(new IconInfo(typeof(IconReference), "Cube Solid", defaultIcon.Texture));
                }
                catch (Exception)
                {
                }
            }

            return _cachedIcons;
        }

        private static string FormatIconDisplayName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return typeName;

            if (typeName.StartsWith("Icon"))
                typeName = typeName.Substring(4);

            var result = new System.Text.StringBuilder();
            result.Append(typeName[0]);
            for (int i = 1; i < typeName.Length; i++)
            {
                if (char.IsUpper(typeName[i]))
                    result.Append(' ');
                result.Append(typeName[i]);
            }
            return result.ToString();
        }
    }
}