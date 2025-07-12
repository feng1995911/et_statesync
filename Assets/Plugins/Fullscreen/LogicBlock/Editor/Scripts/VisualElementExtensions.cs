using UnityEngine.UIElements;
using System.Linq;
using Fullscreen.LogicBlock.Runtime;

namespace Fullscreen.LogicBlock.Editor
{
    public static class VisualElementExtensions
    {
        public static VisualElement FirstChildWithName(this VisualElement element, string name)
        {
            return element.Children().FirstOrDefault(e => e.name == name);
        }
    }
}