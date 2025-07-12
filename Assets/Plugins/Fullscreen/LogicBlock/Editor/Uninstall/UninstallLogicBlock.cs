using GameCreator.Editor.Installs;
using UnityEditor;

namespace Fullscreen.LogicBlock.Editor
{
    public static class UninstallLogicBlock
    {
        private const string UNINSTALL_TITLE = "Are you sure you want to uninstall {0}";
        private const string UNINSTALL_MSG = "This operation cannot be undone";
        
        [MenuItem(
            itemName: "Game Creator/Uninstall/LogicBlock",
            isValidateFunction: false,
            priority: UninstallManager.PRIORITY
        )]
        
        private static void Uninstall()
        {
            UninstallManager.Uninstall("LogicBlock");

            var moduleFolder = "LogicBlock";
            var path = "Assets/Plugins/Fullscreen/" + moduleFolder;
            if (!AssetDatabase.IsValidFolder(path)) return;

            var delete = EditorUtility.DisplayDialog(
                string.Format(UNINSTALL_TITLE, moduleFolder),
                UNINSTALL_MSG, 
                "Yes", "Cancel"
            );
            
            if (!delete) return;
            
            AssetDatabase.MoveAssetToTrash(path);
        }
    }
}