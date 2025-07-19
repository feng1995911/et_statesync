using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Fullscreen.LogicBlock.Editor
{
    [InitializeOnLoad]
    public static class LogicBlockIntegrationDetector
    {
        private const string InventoryPackagePath = "Assets/Plugins/Fullscreen/LogicBlock/Integration/InventoryIntegration.unitypackage";
        private const string MeleePackagePath = "Assets/Plugins/Fullscreen/LogicBlock/Integration/MeleeIntegration.unitypackage";
        private const string ShooterPackagePath = "Assets/Plugins/Fullscreen/LogicBlock/Integration/ShooterIntegration.unitypackage";
        private const string QuestsPackagePath = "Assets/Plugins/Fullscreen/LogicBlock/Integration/QuestsIntegration.unitypackage";
        private const string StatsPackagePath = "Assets/Plugins/Fullscreen/LogicBlock/Integration/StatsIntegration.unitypackage";

        private static readonly string DetectionFilePath = Path.Combine(Application.dataPath, "Plugins/Fullscreen/LogicBlock/Integration/Detected.v1.0.5.txt");
        private static readonly string[] ScriptsToProcess = new[]
        {
            "Assets/Plugins/Fullscreen/LogicBlock/Runtime/Scripts/InstructionBlockHandler.cs",
            "Assets/Plugins/Fullscreen/LogicBlock/Runtime/References/Reference.cs",
            "Assets/Plugins/Fullscreen/LogicBlock/Editor/Scripts/ReferenceUtility.cs"
        }; 
 
        static LogicBlockIntegrationDetector()
        {
            if (!File.Exists(DetectionFilePath))
            {
                DetectAndInstall();
                Directory.CreateDirectory(Path.GetDirectoryName(DetectionFilePath));
                File.WriteAllText(DetectionFilePath, "true");
            }
        }

        [MenuItem("Game Creator/LogicBlock/Detect And Install Integrations")]
        public static void DetectAndInstall()
        {
            bool inventoryInstalled = IsGameCreatorInventoryInstalled();
            bool meleeInstalled = IsGameCreatorMeleeInstalled();
            bool shooterInstalled = IsGameCreatorShooterInstalled();
            bool questsInstalled = IsGameCreatorQuestsInstalled();
            bool statsInstalled = IsGameCreatorStatsInstalled();

            ProcessModule("Inventory", inventoryInstalled, InventoryPackagePath);
            ProcessModule("Melee", meleeInstalled, MeleePackagePath);
            ProcessModule("Shooter", shooterInstalled, ShooterPackagePath);
            ProcessModule("Quests", questsInstalled, QuestsPackagePath);
            ProcessModule("Stats", statsInstalled, StatsPackagePath);
        }

        private static void ProcessModule(string moduleName, bool isInstalled, string packagePath)
        {
            if (isInstalled)
            {
                ImportPackage(packagePath);
            }
            ProcessScriptsForModule(moduleName, isInstalled);
        }
        private static bool IsGameCreatorInventoryInstalled()
        {
            return Type.GetType("GameCreator.Runtime.Inventory.Item, GameCreator.Runtime.Inventory") != null;
        }

        private static bool IsGameCreatorMeleeInstalled()
        {
            return Type.GetType("GameCreator.Runtime.Melee.MeleeWeapon, GameCreator.Runtime.Melee") != null;
        }

        private static bool IsGameCreatorShooterInstalled()
        {
            return Type.GetType("GameCreator.Runtime.Shooter.ShooterWeapon, GameCreator.Runtime.Shooter") != null;
        }

        private static bool IsGameCreatorQuestsInstalled()
        {
            return Type.GetType("GameCreator.Runtime.Quests.Quest, GameCreator.Runtime.Quests") != null;
        }

        private static bool IsGameCreatorStatsInstalled()
        {
            return Type.GetType("GameCreator.Runtime.Stats.Formula, GameCreator.Runtime.Stats") != null;
        }
        private static void ProcessScriptsForModule(string moduleName, bool enable)
        {
            foreach (string scriptPath in ScriptsToProcess)
            {
                if (!File.Exists(scriptPath)) continue;

                string scriptContent = File.ReadAllText(scriptPath);
                string pattern = $@"(//\s*<{moduleName}>\s*\n)([\s\S]*?)(//\s*</{moduleName}>)";

                string newContent = Regex.Replace(scriptContent, pattern, match =>
                {
                    string startTag = match.Groups[1].Value;
                    string innerContent = match.Groups[2].Value;
                    string endTag = match.Groups[3].Value;

                    string processedContent = enable
                        ? Regex.Replace(innerContent, @"^\s*//\s?", "", RegexOptions.Multiline)
                        : Regex.Replace(innerContent, @"^(?!\s*//)", "//", RegexOptions.Multiline);

                    return $"{startTag}{processedContent}{endTag}";
                });

                if (scriptContent != newContent)
                {
                    File.WriteAllText(scriptPath, newContent);
                    AssetDatabase.Refresh();
                }
            }
        }

        private static void ImportPackage(string packagePath)
        {
            if (File.Exists(packagePath))
            {
                AssetDatabase.ImportPackage(packagePath, false);
            }
        }
    }
} 