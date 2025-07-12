#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Fullscreen.LogicBlock.Runtime;

namespace Fullscreen.LogicBlock.Editor
{
    [InitializeOnLoad]
    public static class BlockReferenceManagerEditorCleanup
    {
        static BlockReferenceManagerEditorCleanup()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (BlockReferenceManager.Instance != null)
                {
                    Object.DestroyImmediate(BlockReferenceManager.Instance.gameObject);
                }
            }
        }
    }
}
#endif
