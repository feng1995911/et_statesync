using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
//<Inventory>
using GameCreator.Runtime.Inventory;
//</Inventory>
//<Melee>
using GameCreator.Runtime.Melee;
//</Melee>
//<Shooter>
using GameCreator.Runtime.Shooter;
//</Shooter>
//<Quests>
using GameCreator.Runtime.Quests;
//</Quests>
//<Stats>
using GameCreator.Runtime.Stats;
//</Stats>

namespace Fullscreen.LogicBlock.Runtime
{
    public class InstructionBlockHandler
    {
        private readonly BlockInstructionReference m_SelectedList;
        private static readonly Dictionary<ReferenceWrapper.ReferenceType, Func<Block, string, string, object>> ReferenceCreators = new()
        {
            { ReferenceWrapper.ReferenceType.GameObject, (block, listId, name) => BlockGetGameObjectReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.String, (block, listId, name) => BlockGetStringReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Number, (block, listId, name) => BlockGetNumberReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Boolean, (block, listId, name) => BlockGetBoolReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Color, (block, listId, name) => BlockGetColorReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Vector3, (block, listId, name) => BlockGetPositionReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.AnimationClip, (block, listId, name) => BlockGetAnimationReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.AudioClip, (block, listId, name) => BlockGetAudioReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Material, (block, listId, name) => BlockGetMaterialReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Sprite, (block, listId, name) => BlockGetSpriteReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Texture, (block, listId, name) => BlockGetTextureReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Handle, (block, listId, name) => BlockGetHandleReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Scene, (block, listId, name) => BlockGetSceneReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Scale, (block, listId, name) => BlockGetScaleReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Rotation, (block, listId, name) => BlockGetRotationReference.Create(block, listId, name) },
            { ReferenceWrapper.ReferenceType.Direction, (block, listId, name) => BlockGetDirectionReference.Create(block, listId, name) },
            //<Inventory>
          { ReferenceWrapper.ReferenceType.Item, (block, listId, name) => BlockGetItemReference.Create(block, listId, name) },
          { ReferenceWrapper.ReferenceType.RuntimeItem, (block, listId, name) => BlockGetRuntimeItemReference.Create(block, listId, name) },
          { ReferenceWrapper.ReferenceType.LootTable, (block, listId, name) => BlockGetLootTableReference.Create(block, listId, name) },
         //</Inventory>
            //<Quests>
          { ReferenceWrapper.ReferenceType.Quest, (block, listId, name) => BlockGetQuestReference.Create(block, listId, name) },
         //</Quests>
            //<Shooter>
       { ReferenceWrapper.ReferenceType.ShooterWeapon, (block, listId, name) => BlockGetShooterWeaponReference.Create(block, listId, name) },
      //</Shooter>
            //<Melee>
         { ReferenceWrapper.ReferenceType.MeleeWeapon, (block, listId, name) => BlockGetMeleeWeaponReference.Create(block, listId, name) },
         { ReferenceWrapper.ReferenceType.Shield, (block, listId, name) => BlockGetShieldReference.Create(block, listId, name) },
         { ReferenceWrapper.ReferenceType.Skill, (block, listId, name) => BlockGetSkillReference.Create(block, listId, name) },
        //</Melee>
            //<Stats>
          { ReferenceWrapper.ReferenceType.Attribute, (block, listId, name) => BlockGetAttributeReference.Create(block, listId, name) },
          { ReferenceWrapper.ReferenceType.Formula, (block, listId, name) => BlockGetFormulaReference.Create(block, listId, name) },
          { ReferenceWrapper.ReferenceType.Stat, (block, listId, name) => BlockGetStatReference.Create(block, listId, name) },
          { ReferenceWrapper.ReferenceType.StatusEffect, (block, listId, name) => BlockGetStatusEffectReference.Create(block, listId, name) }
         //</Stats>
        };

        public InstructionBlockHandler(BlockInstructionReference selectedList)
        {
            m_SelectedList = selectedList ?? throw new ArgumentNullException(nameof(selectedList));
        }

        public Reference[] UpdateReferences()
        {
            if (m_SelectedList.Block == null || string.IsNullOrEmpty(m_SelectedList.InstructionId))
            {
                return Array.Empty<Reference>();
            }

            var actionList = m_SelectedList.Block.ActionLists.FirstOrDefault(list =>
                list.UniqueId.Equals(m_SelectedList.InstructionId, StringComparison.Ordinal));

            if (actionList != null)
            {
                return actionList.References
                    .Select(w => new Reference(w.Name, w))
                    .ToArray() ?? Array.Empty<Reference>();
            }

            var branchList = m_SelectedList.Block.BranchLists.FirstOrDefault(list =>
                list.UniqueId.Equals(m_SelectedList.InstructionId, StringComparison.Ordinal));

            return branchList?.References
                .Select(w => new Reference(w.Name, w))
                .ToArray() ?? Array.Empty<Reference>();
        }

        public void UpdateStorageReferences(BlockReferenceStorage storage, NamedInstructionList targetList, Reference[] references, Args args)
        {
            if (storage == null || targetList == null || references == null || args == null) return;

            foreach (var prop in references)
            {
                var runtimeRef = targetList.References
                    .FirstOrDefault(r => r.Name.Equals(prop.ReferenceName, StringComparison.OrdinalIgnoreCase));

                if (runtimeRef == null) continue;

                switch (runtimeRef.Type)
                {
                    case ReferenceWrapper.ReferenceType.GameObject:
                        GameObject go = prop.GameObject.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, go);
                        break;

                    case ReferenceWrapper.ReferenceType.String:
                        string str = prop.String.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, str);
                        break;

                    case ReferenceWrapper.ReferenceType.Number:
                        double number = prop.Decimal.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, number);
                        break;

                    case ReferenceWrapper.ReferenceType.Boolean:
                        bool boolean = prop.Bool.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, boolean);
                        break;

                    case ReferenceWrapper.ReferenceType.Color:
                        Color color = prop.Color.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, color);
                        break;

                    case ReferenceWrapper.ReferenceType.Vector3:
                        Vector3 vector3 = prop.Position.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, vector3);
                        break;

                    case ReferenceWrapper.ReferenceType.AnimationClip:
                        AnimationClip animClip = prop.Animation.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, animClip);
                        break;

                    case ReferenceWrapper.ReferenceType.AudioClip:
                        AudioClip audioClip = prop.Audio.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, audioClip);
                        break;

                    case ReferenceWrapper.ReferenceType.Material:
                        Material material = prop.Material.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, material);
                        break;

                    case ReferenceWrapper.ReferenceType.Sprite:
                        Sprite sprite = prop.Sprite.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, sprite);
                        break;

                    case ReferenceWrapper.ReferenceType.Texture:
                        Texture texture = prop.Texture.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, texture);
                        break;

                    case ReferenceWrapper.ReferenceType.Handle:
                        Handle handle = prop.Handle.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, handle);
                        break;

                    case ReferenceWrapper.ReferenceType.Scene:
                        int sceneIndex = prop.Scene.Get(args);
                        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
                        {
                            string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                            SceneReference scene = new SceneReference { m_Name = sceneName };
                            storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, scene);
                        }
                        break;

                    case ReferenceWrapper.ReferenceType.Scale:
                        Vector3 scale = prop.Scale.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, scale);
                        break;

                    case ReferenceWrapper.ReferenceType.Rotation:
                        Quaternion rotation = prop.Rotation.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, rotation);
                        break;

                    case ReferenceWrapper.ReferenceType.Direction:
                        Vector3 direction = prop.Direction.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, direction);
                        break;

                    //<Inventory>
                  case ReferenceWrapper.ReferenceType.Item:
                      Item item = prop.Item.Get(args);
                      storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, item);
                      break;
                  case ReferenceWrapper.ReferenceType.RuntimeItem:
                      RuntimeItem runtimeItem = prop.RuntimeItem.Get(args);
                      storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, runtimeItem);
                      break;
                  case ReferenceWrapper.ReferenceType.LootTable:
                      LootTable lootTable = prop.LootTable.Get(args);
                      storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, lootTable);
                      break;
                 //</Inventory>
                    //<Quests>
                  case ReferenceWrapper.ReferenceType.Quest:
                  Quest quest = prop.Quest.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, quest);
                  break;
                 //</Quests>
                    //<Shooter>
               case ReferenceWrapper.ReferenceType.ShooterWeapon:
               IWeapon iWeapon = prop.ShooterWeapon.Get(args);
               if (iWeapon is ShooterWeapon shooterWeapon)
               {
               storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, shooterWeapon);
               }
               break;
              //</Shooter>
                    //<Melee>
                 case ReferenceWrapper.ReferenceType.MeleeWeapon:
                 IWeapon iMeleeWeapon = prop.MeleeWeapon.Get(args);
                 if (iMeleeWeapon is MeleeWeapon meleeWeapon)
                 {
                 storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, meleeWeapon);
                 }
                 break;
                 case ReferenceWrapper.ReferenceType.Shield:
                 IShield iShield = prop.Shield.Get(args);
                 if (iShield is Shield shield)
                 {
                 storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, shield);
                 }
                 break;
                 case ReferenceWrapper.ReferenceType.Skill:
                 Skill skill = prop.Skill.Get(args);
                 storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, skill);
                 break;
                //</Melee>
                    //<Stats>
                  case ReferenceWrapper.ReferenceType.Attribute:
                  GameCreator.Runtime.Stats.Attribute attribute = prop.Attribute.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, attribute);
                  break;
                  case ReferenceWrapper.ReferenceType.Formula:
                  Formula formula = prop.Formula.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, formula);
                  break;
                  case ReferenceWrapper.ReferenceType.Stat:
                  Stat stat = prop.Stat.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, stat);
                  break;
                  case ReferenceWrapper.ReferenceType.StatusEffect:
                  StatusEffect statusEffect = prop.StatusEffect.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, statusEffect);
                  break;
                 //</Stats>
                }
            }
        }

        public void UpdateStorageReferences(BlockReferenceStorage storage, NamedBranchList targetList, Reference[] references, Args args)
        {
            if (storage == null || targetList == null || references == null || args == null) return;

            foreach (var prop in references)
            {
                var runtimeRef = targetList.References
                    .FirstOrDefault(r => r.Name.Equals(prop.ReferenceName, StringComparison.OrdinalIgnoreCase));

                switch (runtimeRef.Type)
                {
                    case ReferenceWrapper.ReferenceType.GameObject:
                        GameObject go = prop.GameObject.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, go);
                        break;

                    case ReferenceWrapper.ReferenceType.String:
                        string str = prop.String.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, str);
                        break;

                    case ReferenceWrapper.ReferenceType.Number:
                        double number = prop.Decimal.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, number);
                        break;

                    case ReferenceWrapper.ReferenceType.Boolean:
                        bool boolean = prop.Bool.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, boolean);
                        break;

                    case ReferenceWrapper.ReferenceType.Color:
                        Color color = prop.Color.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, color);
                        break;

                    case ReferenceWrapper.ReferenceType.Vector3:
                        Vector3 vector3 = prop.Position.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, vector3);
                        break;

                    case ReferenceWrapper.ReferenceType.AnimationClip:
                        AnimationClip animClip = prop.Animation.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, animClip);
                        break;

                    case ReferenceWrapper.ReferenceType.AudioClip:
                        AudioClip audioClip = prop.Audio.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, audioClip);
                        break;

                    case ReferenceWrapper.ReferenceType.Material:
                        Material material = prop.Material.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, material);
                        break;

                    case ReferenceWrapper.ReferenceType.Sprite:
                        Sprite sprite = prop.Sprite.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, sprite);
                        break;

                    case ReferenceWrapper.ReferenceType.Texture:
                        Texture texture = prop.Texture.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, texture);
                        break;

                    case ReferenceWrapper.ReferenceType.Handle:
                        Handle handle = prop.Handle.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, handle);
                        break;

                    case ReferenceWrapper.ReferenceType.Scene:
                        int sceneIndex = prop.Scene.Get(args);
                        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
                        {
                            string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                            SceneReference scene = new SceneReference { m_Name = sceneName };
                            storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, scene);
                        }
                        break;

                    case ReferenceWrapper.ReferenceType.Scale:
                        Vector3 scale = prop.Scale.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, scale);
                        break;

                    case ReferenceWrapper.ReferenceType.Rotation:
                        Quaternion rotation = prop.Rotation.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, rotation);
                        break;

                    case ReferenceWrapper.ReferenceType.Direction:
                        Vector3 direction = prop.Direction.Get(args);
                        storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, direction);
                        break;

                    //<Inventory>
                  case ReferenceWrapper.ReferenceType.Item:
                      Item item = prop.Item.Get(args);
                      storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, item);
                      break;
                  case ReferenceWrapper.ReferenceType.RuntimeItem:
                      RuntimeItem runtimeItem = prop.RuntimeItem.Get(args);
                     storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, runtimeItem);
                      break;
                  case ReferenceWrapper.ReferenceType.LootTable:
                      LootTable lootTable = prop.LootTable.Get(args);
                      storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, lootTable);
                      break;
                 //</Inventory>
                    //<Quests>
                  case ReferenceWrapper.ReferenceType.Quest:
                  Quest quest = prop.Quest.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, quest);
                  break;
                 //</Quests>
                    //<Shooter>
               case ReferenceWrapper.ReferenceType.ShooterWeapon:
               IWeapon iWeapon = prop.ShooterWeapon.Get(args);
               if (iWeapon is ShooterWeapon shooterWeapon)
               {
                   storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, shooterWeapon);
               }
               break;
              //</Shooter>
                    //<Melee>
                 case ReferenceWrapper.ReferenceType.MeleeWeapon:
                 IWeapon iMeleeWeapon = prop.MeleeWeapon.Get(args);
                 if (iMeleeWeapon is MeleeWeapon meleeWeapon)
                 {
                     storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, meleeWeapon);
                 }
                 break;
                 case ReferenceWrapper.ReferenceType.Shield:
                 IShield iShield = prop.Shield.Get(args);
                 if (iShield is Shield shield)
                 {
                     storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, shield);
                 }
                 break;
                 case ReferenceWrapper.ReferenceType.Skill:
                 Skill skill = prop.Skill.Get(args);
                 storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, skill);
                 break;
                //</Melee>
                    //<Stats>
                  case ReferenceWrapper.ReferenceType.Attribute:
                  GameCreator.Runtime.Stats.Attribute attribute = prop.Attribute.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, attribute);
                  break;
                  case ReferenceWrapper.ReferenceType.Formula:
                  Formula formula = prop.Formula.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, formula);
                  break;
                  case ReferenceWrapper.ReferenceType.Stat:
                  Stat stat = prop.Stat.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, stat);
                  break;
                  case ReferenceWrapper.ReferenceType.StatusEffect:
                  StatusEffect statusEffect = prop.StatusEffect.Get(args);
                  storage.UpdateReference(targetList.UniqueId, prop.ReferenceName, statusEffect);
                  break;
                 //</Stats>
                }
            }
        }

        public NamedInstructionList CreateNewInstructionList(Block block, string listName, InstructionList instructions, List<ReferenceWrapper> references, string targetInstructionId = null)
        {
            var newActionList = new NamedInstructionList(listName, instructions, references);

            if (!string.IsNullOrEmpty(targetInstructionId))
            {
                var uniqueIdField = typeof(NamedInstructionList).GetField("m_UniqueId", BindingFlags.NonPublic | BindingFlags.Instance);
                uniqueIdField?.SetValue(newActionList, targetInstructionId);
            }

            var actionListsField = typeof(Block).GetField("m_ActionLists", BindingFlags.NonPublic | BindingFlags.Instance);
            if (actionListsField == null) return null;

            var actionLists = (List<NamedInstructionList>)actionListsField.GetValue(block) ?? new List<NamedInstructionList>();
            actionLists.Add(newActionList);
            actionListsField.SetValue(block, actionLists);
            return newActionList;
        }

        public NamedBranchList CreateNewBranchList(Block block, string listName, BranchList branches, List<ReferenceWrapper> references, string targetInstructionId = null)
        {
            var newBranchList = new NamedBranchList(listName, branches, references);

            if (!string.IsNullOrEmpty(targetInstructionId))
            {
                var uniqueIdField = typeof(NamedBranchList).GetField("m_UniqueId", BindingFlags.NonPublic | BindingFlags.Instance);
                uniqueIdField?.SetValue(newBranchList, targetInstructionId);
            }

            var branchListsField = typeof(Block).GetField("m_BranchLists", BindingFlags.NonPublic | BindingFlags.Instance);
            if (branchListsField == null) return null;

            var branchLists = (List<NamedBranchList>)branchListsField.GetValue(block) ?? new List<NamedBranchList>();
            branchLists.Add(newBranchList);
            branchListsField.SetValue(block, branchLists);
            return newBranchList;
        }

        public string GenerateUniqueBlockName(string baseName, GameObject go)
        {
            BlockInstanceTracker tracker = go.GetComponent<BlockInstanceTracker>();
            string newName = baseName;
            int counter = 1;
            while (tracker?.BlockInstance != null && tracker.BlockInstance.name.Equals(newName, StringComparison.OrdinalIgnoreCase))
            {
                newName = $"{baseName}_{counter++}";
            }
            return newName;
        }

        public string GenerateUniqueName(string baseName, NamedInstructionList[] existingLists)
        {
            if (existingLists == null || existingLists.Length == 0) return baseName;

            string newName = baseName;
            int counter = 1;
            while (existingLists.Any(list => list.ListName.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                newName = $"{baseName}_{counter++}";
            }
            return newName;
        }

        public string GenerateUniqueName(string baseName, NamedBranchList[] existingLists)
        {
            if (existingLists == null || existingLists.Length == 0) return baseName;

            string newName = baseName;
            int counter = 1;
            while (existingLists.Any(list => list.ListName.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                newName = $"{baseName}_{counter++}";
            }
            return newName;
        }

        public InstructionList DeepCopyInstructionList(InstructionList source)
        {
            if (source == null) return null;

            try
            {
                string json = JsonUtility.ToJson(source);
                InstructionList newInstructionList = new();
                JsonUtility.FromJsonOverwrite(json, newInstructionList);
                return newInstructionList;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to copy instruction list: {ex.Message}");
                return null;
            }
        }

        public BranchList DeepCopyBranchList(BranchList source)
        {
            if (source == null) return null;

            try
            {
                string json = JsonUtility.ToJson(source);
                BranchList newBranches = new BranchList();
                JsonUtility.FromJsonOverwrite(json, newBranches);
                return newBranches;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to copy branch list: {ex.Message}");
                return null;
            }
        }

        public void UpdateReferencesInInstructionList(Block newBlock, string newListId, InstructionList sourceInstructionList)
        {
            if (sourceInstructionList == null) return;

            var instructionsField = typeof(InstructionList).GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (instructionsField == null) return;

            var instructionList = instructionsField.GetValue(sourceInstructionList) as Instruction[];
            if (instructionList == null) return;

            foreach (var instruction in instructionList)
            {
                if (instruction == null) continue;
                UpdateObjectReferences(instruction, instruction, null, newBlock, newListId, new Stack<object>());
            }
        }

        public void UpdateReferencesInBranchList(Block newBlock, string newListId, BranchList sourceBranches)
        {
            if (sourceBranches == null) return;

            var branchesField = typeof(BranchList).GetField("m_Branches", BindingFlags.NonPublic | BindingFlags.Instance);
            if (branchesField == null) return;

            var branchList = branchesField.GetValue(sourceBranches) as Branch[];
            if (branchList == null) return;

            foreach (var branch in branchList)
            {
                if (branch == null) continue;

                var conditionField = typeof(Branch).GetField("m_ConditionList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (conditionField != null)
                {
                    var condition = conditionField.GetValue(branch);
                    if (condition != null)
                    {
                        UpdateObjectReferences(condition, condition, null, newBlock, newListId, new Stack<object>());
                    }
                }

                var instructionsField = typeof(Branch).GetField("m_InstructionList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (instructionsField != null)
                {
                    var instructions = instructionsField.GetValue(branch) as InstructionList;
                    if (instructions != null)
                    {
                        UpdateReferencesInInstructionList(newBlock, newListId, instructions);
                    }
                }
            }
        }

        private void UpdateObjectReferences(object obj, object rootObj, FieldInfo parentField, Block newBlock, string newListId, Stack<object> visited)
        {
            if (obj == null || visited.Contains(obj)) return;

            visited.Push(obj);
            Type objType = obj.GetType();

            string typeName = objType.Name;
            if (typeName.StartsWith("BlockGet") && typeName.EndsWith("Reference"))
            {
                var listRefField = objType.GetField("m_ListReference", BindingFlags.NonPublic | BindingFlags.Instance);
                if (listRefField != null)
                {
                    var listRef = (PropertyGetBlockListReference)listRefField.GetValue(obj);
                    if (listRef != null && listRef.Block == m_SelectedList.Block && parentField != null && rootObj != null)
                    {
                        var refType = GetReferenceTypeFromClassName(typeName);
                        if (refType.HasValue && ReferenceCreators.TryGetValue(refType.Value, out var creator))
                        {
                            var newBlockRef = creator(newBlock, newListId, listRef.ReferenceName);
                            var propertyField = newBlockRef.GetType().GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (propertyField != null)
                            {
                                parentField.SetValue(rootObj, propertyField.GetValue(newBlockRef));
                                visited.Pop();
                                return;
                            }
                        }
                    }
                }
            }

            foreach (var field in objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(obj);
                if (fieldValue == null) continue;

                if (fieldValue is Array array)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        object element = array.GetValue(i);
                        if (element != null)
                        {
                            UpdateObjectReferences(element, array, null, newBlock, newListId, visited);
                            array.SetValue(element, i);
                        }
                    }
                }
                else if (fieldValue is System.Collections.IList list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        object element = list[i];
                        if (element != null)
                        {
                            UpdateObjectReferences(element, list, null, newBlock, newListId, visited);
                            list[i] = element;
                        }
                    }
                }
                else if (!field.FieldType.IsPrimitive && field.FieldType != typeof(string))
                {
                    UpdateObjectReferences(fieldValue, obj, field, newBlock, newListId, visited);
                }
            }

            visited.Pop();
        }

        private static ReferenceWrapper.ReferenceType? GetReferenceTypeFromClassName(string className)
        {
            return className switch
            {
                "BlockGetGameObjectReference" => ReferenceWrapper.ReferenceType.GameObject,
                "BlockGetStringReference" => ReferenceWrapper.ReferenceType.String,
                "BlockGetNumberReference" => ReferenceWrapper.ReferenceType.Number,
                "BlockGetBoolReference" => ReferenceWrapper.ReferenceType.Boolean,
                "BlockGetColorReference" => ReferenceWrapper.ReferenceType.Color,
                "BlockGetPositionReference" => ReferenceWrapper.ReferenceType.Vector3,
                "BlockGetAnimationReference" => ReferenceWrapper.ReferenceType.AnimationClip,
                "BlockGetAudioReference" => ReferenceWrapper.ReferenceType.AudioClip,
                "BlockGetMaterialReference" => ReferenceWrapper.ReferenceType.Material,
                "BlockGetSpriteReference" => ReferenceWrapper.ReferenceType.Sprite,
                "BlockGetTextureReference" => ReferenceWrapper.ReferenceType.Texture,
                "BlockGetHandleReference" => ReferenceWrapper.ReferenceType.Handle,
                "BlockGetSceneReference" => ReferenceWrapper.ReferenceType.Scene,
                "BlockGetScaleReference" => ReferenceWrapper.ReferenceType.Scale,
                "BlockGetRotationReference" => ReferenceWrapper.ReferenceType.Rotation,
                "BlockGetDirectionReference" => ReferenceWrapper.ReferenceType.Direction,
                //<Inventory>
              "BlockGetItemReference" => ReferenceWrapper.ReferenceType.Item,
              "BlockGetRuntimeItemReference" => ReferenceWrapper.ReferenceType.RuntimeItem,
              "BlockGetLootTableReference" => ReferenceWrapper.ReferenceType.LootTable,
             //</Inventory>
                //<Quests>
              "BlockGetQuestReference" => ReferenceWrapper.ReferenceType.Quest,
             //</Quests>
                //<Shooter>
           "BlockGetShooterWeaponReference" => ReferenceWrapper.ReferenceType.ShooterWeapon,
          //</Shooter>
                //<Melee>
             "BlockGetMeleeWeaponReference" => ReferenceWrapper.ReferenceType.MeleeWeapon,
             "BlockGetShieldReference" => ReferenceWrapper.ReferenceType.Shield,
             "BlockGetSkillReference" => ReferenceWrapper.ReferenceType.Skill,
            //</Melee>
                //<Stats>
              "BlockGetAttributeReference" => ReferenceWrapper.ReferenceType.Attribute,
              "BlockGetFormulaReference" => ReferenceWrapper.ReferenceType.Formula,
              "BlockGetStatReference" => ReferenceWrapper.ReferenceType.Stat,
              "BlockGetStatusEffectReference" => ReferenceWrapper.ReferenceType.StatusEffect,
             //</Stats>
                _ => null
            };
        }
    }
}