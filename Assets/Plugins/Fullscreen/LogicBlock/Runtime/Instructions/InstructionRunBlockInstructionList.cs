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
    [Title("Run Instruction Block")]
    [Description("Runs an instruction list from a Block")]
    [Category("LogicBlock/Run Instruction Block List")]
    [Parameter("Instruction List", "The instruction list to run from the Block")]
    [Parameter("Target", "The GameObject to use as the target")]
    [Parameter("Instance Block", "If true, it instances the instruction list to support running same block at the same time")]
    [Keywords("Execute", "Action", "Run", "Asset", "Block", "ScriptableObject")]
    [Image(typeof(IconBlockActions), ColorTheme.Type.White)]
    [Serializable]
    public class InstructionRunBlockInstructionList : Instruction
    {
        [SerializeField] private BlockInstructionReference m_SelectedList = new BlockInstructionReference();
        [SerializeField] private bool m_InstanceBlock;
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
        [SerializeField] private Reference[] m_References = Array.Empty<Reference>();
        private readonly InstructionBlockHandler m_Handler;

        public InstructionRunBlockInstructionList()
        {
            m_Handler = new InstructionBlockHandler(m_SelectedList);
        }

        public override string Title
        {
            get
            {
                if (m_SelectedList == null)
                    return "Run (No List Selected)";

                string instructionName = null;
                try
                {
                    instructionName = m_SelectedList.GetInstructionName();
                }
                catch
                {
                }

                string blockName = m_SelectedList.Block != null ? m_SelectedList.Block.name : null;

                if (string.IsNullOrEmpty(instructionName) && string.IsNullOrEmpty(blockName))
                    return "Run [Invalid List: Missing List & Block]";

                if (string.IsNullOrEmpty(instructionName))
                    return $"Run [Missing List] from {blockName}";

                if (string.IsNullOrEmpty(blockName))
                    return $"Run {instructionName} [Missing Block]";

                return $"Run {instructionName} from {blockName}" + (m_InstanceBlock ? " (Instance)" : "");
            }
        }

        private void OnValidate()
        {
            if (m_SelectedList.Block == null || string.IsNullOrEmpty(m_SelectedList.InstructionId)) return;
            m_References = m_Handler.UpdateReferences();
        }

        protected override async System.Threading.Tasks.Task Run(Args args)
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
                    var actionList = tracker.BlockInstance.ActionLists.FirstOrDefault(list =>
                        list.UniqueId.Equals(m_SelectedList.InstructionId, StringComparison.Ordinal));

                    if (actionList != null)
                    {
                        targetBlock = tracker.BlockInstance;
                        targetInstructionId = actionList.UniqueId;
                    }
                    else
                    {
                        var sourceList = m_SelectedList.Block.ActionLists.FirstOrDefault(list =>
                            list.UniqueId.Equals(m_SelectedList.InstructionId, StringComparison.Ordinal));

                        NamedInstructionList newActionList = sourceList != null
                            ? m_Handler.CreateNewInstructionList(
                                tracker.BlockInstance,
                                sourceList.ListName,
                                m_Handler.DeepCopyInstructionList(sourceList.Instructions),
                                sourceList.References,
                                m_SelectedList.InstructionId)
                            : m_Handler.CreateNewInstructionList(
                                tracker.BlockInstance,
                                m_SelectedList.GetInstructionName(),
                                new InstructionList(),
                                new List<ReferenceWrapper>(),
                                m_SelectedList.InstructionId);

                        if (newActionList == null) return;
                        m_Handler.UpdateReferencesInInstructionList(tracker.BlockInstance, newActionList.UniqueId, newActionList.Instructions);
                        targetBlock = tracker.BlockInstance;
                        targetInstructionId = newActionList.UniqueId;
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

                    var sourceList = m_SelectedList.Block.ActionLists.FirstOrDefault(list =>
                        list.UniqueId.Equals(m_SelectedList.InstructionId, StringComparison.Ordinal));

                    NamedInstructionList newActionList = sourceList != null
                        ? m_Handler.CreateNewInstructionList(
                            newBlock,
                            sourceList.ListName,
                            m_Handler.DeepCopyInstructionList(sourceList.Instructions),
                            sourceList.References,
                            m_SelectedList.InstructionId)
                        : m_Handler.CreateNewInstructionList(
                            newBlock,
                            m_SelectedList.GetInstructionName(),
                            new InstructionList(),
                            new List<ReferenceWrapper>(),
                            m_SelectedList.InstructionId);

                    if (newActionList == null)
                    {
                        ScriptableObject.DestroyImmediate(newBlock);
                        return;
                    }

                    m_Handler.UpdateReferencesInInstructionList(newBlock, newActionList.UniqueId, newActionList.Instructions);

                    tracker = tracker ?? selfGO.AddComponent<BlockInstanceTracker>();
                    if (tracker.BlockInstance != null)
                    {
                        ScriptableObject.DestroyImmediate(tracker.BlockInstance);
                    }
                    tracker.SetBlock(newBlock);

                    targetBlock = newBlock;
                    targetInstructionId = newActionList.UniqueId;
                }
            }

            var targetListFinal = targetBlock.ActionLists.FirstOrDefault(list =>
                list.UniqueId.Equals(targetInstructionId, StringComparison.Ordinal));

            if (targetListFinal == null)
            {
                var sourceList = m_SelectedList.Block.ActionLists.FirstOrDefault(list =>
                    list.UniqueId.Equals(targetInstructionId, StringComparison.Ordinal));

                targetListFinal = sourceList != null
                    ? m_Handler.CreateNewInstructionList(
                        targetBlock,
                        sourceList.ListName,
                        m_Handler.DeepCopyInstructionList(sourceList.Instructions),
                        sourceList.References,
                        targetInstructionId)
                    : m_Handler.CreateNewInstructionList(
                        targetBlock,
                        m_SelectedList.GetInstructionName(),
                        new InstructionList(),
                        new List<ReferenceWrapper>(),
                        targetInstructionId);

                if (targetListFinal == null) return;
                m_Handler.UpdateReferencesInInstructionList(targetBlock, targetListFinal.UniqueId, targetListFinal.Instructions);
                targetInstructionId = targetListFinal.UniqueId;
            }

            if (targetListFinal.Instructions == null) return;

            var instructionsField = targetListFinal.Instructions.GetType().GetField("m_Instructions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (instructionsField == null || (instructionsField.GetValue(targetListFinal.Instructions) as Instruction[])?.Length == 0) return;

            string storageKey = $"{targetBlock.ID.String}_{targetInstructionId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            GameObject targetGO = m_Target.Get(args);
            Args newArgs = new(args.Self, targetGO);

            m_Handler.UpdateStorageReferences(storage, targetListFinal, m_References, newArgs);

            await targetListFinal.Instructions.Run(newArgs);
        }
    }
}