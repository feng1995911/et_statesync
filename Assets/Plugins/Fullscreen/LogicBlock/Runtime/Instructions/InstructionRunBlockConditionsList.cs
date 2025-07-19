using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Fullscreen.LogicBlock.Runtime
{
    [Version(0, 0, 1)]
    [Title("Run Conditions Block")]
    [Description("Runs a conditions list from a Block")]
    [Category("LogicBlock/Run Conditions Block List")]
    [Parameter("Conditions List", "The conditions list to run from the Block")]
    [Parameter("Target", "The GameObject to use as the target")]
    [Parameter("Instance Block", "If true, instances the conditions list to support running the same block concurrently")]
    [Keywords("Execute", "Condition", "Branch", "Asset", "Block", "ScriptableObject")]
    [Image(typeof(IconBlockConditions), ColorTheme.Type.White)]
    [Serializable]
    public class InstructionRunBlockConditionList : Instruction
    {
        [SerializeField] private BlockInstructionReference m_SelectedList = new BlockInstructionReference();
        [SerializeField] private bool m_InstanceBlock;
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
        [SerializeField] private Reference[] m_References = Array.Empty<Reference>();
        private readonly InstructionBlockHandler m_Handler;

        public InstructionRunBlockConditionList()
        {
            m_Handler = new InstructionBlockHandler(m_SelectedList);
        }

        public override string Title
        {
            get
            {
                if (m_SelectedList == null)
                    return "Run (No Branch List Selected)";

                string branchName = null;
                try
                {
                    branchName = m_SelectedList.GetInstructionName();
                }
                catch
                {
                }

                string blockName = m_SelectedList.Block != null ? m_SelectedList.Block.name : null;

                if (string.IsNullOrEmpty(branchName) && string.IsNullOrEmpty(blockName))
                    return "Run [Invalid Branch List: Missing List & Block]";

                if (string.IsNullOrEmpty(branchName))
                    return $"Run [Missing Branch List] from {blockName}";

                if (string.IsNullOrEmpty(blockName))
                    return $"Run {branchName} [Missing Block]";

                return $"Run {branchName} from {blockName}" + (m_InstanceBlock ? " (Instance)" : "");
            }
        }

        private void OnValidate()
        {
            if (m_SelectedList.Block == null || string.IsNullOrEmpty(m_SelectedList.InstructionId)) return;
            m_References = m_Handler.UpdateReferences();
        }

        protected override async Task Run(Args args)
        {
            if (m_SelectedList.Block == null || string.IsNullOrEmpty(m_SelectedList.InstructionId)) return;

            Block targetBlock = m_SelectedList.Block;
            string targetInstructionId = m_SelectedList.InstructionId;

            if (m_InstanceBlock)
            {
                GameObject selfGO = args.Self;
                if (selfGO == null) return;

                string baseBlockName = $"Block_{selfGO.GetInstanceID()}";
                string newBlockName = m_Handler.GenerateUniqueBlockName(baseBlockName, selfGO);

                BlockInstanceTracker tracker = selfGO.GetComponent<BlockInstanceTracker>();
                bool createNewBlock = tracker == null || tracker.BlockInstance == null;

                if (!createNewBlock)
                {
                    var branchList = tracker.BlockInstance.BranchLists.FirstOrDefault(list =>
                        list.UniqueId.Equals(m_SelectedList.InstructionId, StringComparison.Ordinal));

                    if (branchList != null)
                    {
                        targetBlock = tracker.BlockInstance;
                        targetInstructionId = branchList.UniqueId;
                    }
                    else
                    {
                        var sourceList = m_SelectedList.Block.BranchLists.FirstOrDefault(list =>
                            list.UniqueId.Equals(m_SelectedList.InstructionId, StringComparison.Ordinal));

                        NamedBranchList newBranchList = sourceList != null
                            ? m_Handler.CreateNewBranchList(
                                tracker.BlockInstance,
                                sourceList.ListName,
                                m_Handler.DeepCopyBranchList(sourceList.Branches),
                                sourceList.References,
                                m_SelectedList.InstructionId)
                            : m_Handler.CreateNewBranchList(
                                tracker.BlockInstance,
                                m_SelectedList.GetInstructionName(),
                                new BranchList(),
                                new List<ReferenceWrapper>(),
                                m_SelectedList.InstructionId);

                        if (newBranchList == null) return;
                        m_Handler.UpdateReferencesInBranchList(tracker.BlockInstance, newBranchList.UniqueId, newBranchList.Branches);
                        targetBlock = tracker.BlockInstance;
                        targetInstructionId = newBranchList.UniqueId;
                    }
                }

                if (createNewBlock)
                {
                    Block newBlock = ScriptableObject.CreateInstance<Block>();
                    newBlock.name = newBlockName;

                    var uniqueIdField = newBlock.GetType().GetField("m_UniqueId", BindingFlags.Public | BindingFlags.Instance);
                    if (uniqueIdField == null)
                    {
                        ScriptableObject.DestroyImmediate(newBlock);
                        return;
                    }
                    uniqueIdField.SetValue(newBlock, new UniqueID(newBlockName));

                    var sourceList = m_SelectedList.Block.BranchLists.FirstOrDefault(list =>
                        list.UniqueId.Equals(m_SelectedList.InstructionId, StringComparison.Ordinal));

                    NamedBranchList newBranchList = sourceList != null
                        ? m_Handler.CreateNewBranchList(
                            newBlock,
                            sourceList.ListName,
                            m_Handler.DeepCopyBranchList(sourceList.Branches),
                            sourceList.References,
                            m_SelectedList.InstructionId)
                        : m_Handler.CreateNewBranchList(
                            newBlock,
                            m_SelectedList.GetInstructionName(),
                            new BranchList(),
                            new List<ReferenceWrapper>(),
                            m_SelectedList.InstructionId);

                    if (newBranchList == null)
                    {
                        ScriptableObject.DestroyImmediate(newBlock);
                        return;
                    }

                    m_Handler.UpdateReferencesInBranchList(newBlock, newBranchList.UniqueId, newBranchList.Branches);

                    tracker = tracker ?? selfGO.AddComponent<BlockInstanceTracker>();
                    if (tracker.BlockInstance != null)
                    {
                        ScriptableObject.DestroyImmediate(tracker.BlockInstance);
                    }
                    tracker.SetBlock(newBlock);

                    targetBlock = newBlock;
                    targetInstructionId = newBranchList.UniqueId;
                }
            }

            var targetListFinal = targetBlock.BranchLists.FirstOrDefault(list =>
                list.UniqueId.Equals(targetInstructionId, StringComparison.Ordinal));

            if (targetListFinal == null)
            {
                var sourceList = m_SelectedList.Block.BranchLists.FirstOrDefault(list =>
                    list.UniqueId.Equals(targetInstructionId, StringComparison.Ordinal));

                targetListFinal = sourceList != null
                    ? m_Handler.CreateNewBranchList(
                        targetBlock,
                        sourceList.ListName,
                        m_Handler.DeepCopyBranchList(sourceList.Branches),
                        sourceList.References,
                        targetInstructionId)
                    : m_Handler.CreateNewBranchList(
                        targetBlock,
                        m_SelectedList.GetInstructionName(),
                        new BranchList(),
                        new List<ReferenceWrapper>(),
                        targetInstructionId);

                if (targetListFinal == null) return;
                m_Handler.UpdateReferencesInBranchList(targetBlock, targetListFinal.UniqueId, targetListFinal.Branches);
                targetInstructionId = targetListFinal.UniqueId;
            }

            if (targetListFinal.Branches == null) return;

            var branchesField = targetListFinal.Branches.GetType().GetField("m_Branches", BindingFlags.NonPublic | BindingFlags.Instance);
            if (branchesField == null || (branchesField.GetValue(targetListFinal.Branches) as Branch[])?.Length == 0) return;

            string storageKey = $"{targetBlock.ID.String}_{targetInstructionId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            GameObject targetGO = m_Target.Get(args);
            Args newArgs = new Args(args.Self, targetGO);

            m_Handler.UpdateStorageReferences(storage, targetListFinal, m_References, newArgs);

            await targetListFinal.Branches.Evaluate(newArgs);
        }
    }
}