using System;
using System.Reflection;
using UnityEditor;

namespace Fullscreen.LogicBlock.Editor
{
    public static class SerializedPropertyExtensions
    {
        public static object GetTargetObject(this SerializedProperty property)
        {
            if (property == null) return null;

            string[] parts = property.propertyPath.Replace(".Array.data[", "[").Split('.');
            object obj = property.serializedObject.targetObject;

            foreach (string part in parts)
            {
                if (part.Contains("["))
                {
                    string fieldName = part.Substring(0, part.IndexOf('['));
                    int index = int.Parse(part.Substring(part.IndexOf('[') + 1, part.Length - fieldName.Length - 2));

                    FieldInfo field = obj?.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field == null) return null;

                    var list = field.GetValue(obj) as System.Collections.IList;
                    if (list == null || index >= list.Count) return null;
                    obj = list[index];
                }
                else
                {
                    FieldInfo field = obj?.GetType().GetField(part, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field == null) return null;
                    obj = field.GetValue(obj);
                }

                if (obj == null) return null;
            }

            return obj;
        }

        public static SerializedProperty GetArrayProperty(this SerializedProperty property)
        {
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf(".");
            if (lastDot >= 0)
            {
                string arrayPath = path.Substring(0, lastDot);
                return property.serializedObject.FindProperty(arrayPath);
            }
            return null;
        }
    }
}
