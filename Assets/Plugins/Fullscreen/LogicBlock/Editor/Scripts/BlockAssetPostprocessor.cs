using UnityEditor;
using UnityEngine;
using Fullscreen.LogicBlock.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fullscreen.LogicBlock.Editor
{
    public class BlockAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            string[] allBlockGUIDs = AssetDatabase.FindAssets("t:Block");
            HashSet<string> existingIDs = new HashSet<string>();

            foreach (string guid in allBlockGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Block existingBlock = AssetDatabase.LoadAssetAtPath<Block>(path);
                if (existingBlock != null && !string.IsNullOrEmpty(existingBlock.ID.String))
                {
                    existingIDs.Add(existingBlock.ID.String);
                }
            }

            foreach (string path in importedAssets)
            {
                if (!path.EndsWith(".asset")) continue;

                Block block = AssetDatabase.LoadAssetAtPath<Block>(path);
                if (block == null || string.IsNullOrEmpty(block.ID.String)) continue;

                int matchCount = 0;
                foreach (string guid in allBlockGUIDs)
                {
                    string otherPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (otherPath == path) continue;

                    Block otherBlock = AssetDatabase.LoadAssetAtPath<Block>(otherPath);
                    if (otherBlock != null && otherBlock.ID.String == block.ID.String)
                    {
                        matchCount++;
                    }
                }

                if (matchCount > 0)
                {
                    string oldId = block.ID.String;
                    block.SetUniqueID(Guid.NewGuid().ToString());

                    foreach (var actionList in block.ActionLists)
                    {
                        actionList.RegenerateUniqueId();
                    }

                    foreach (var branchList in block.BranchLists)
                    {
                        branchList.RegenerateUniqueId();
                    }

                    EditorUtility.SetDirty(block);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
