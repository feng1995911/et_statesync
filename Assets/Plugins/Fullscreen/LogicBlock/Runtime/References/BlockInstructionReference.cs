using System;
using System.Linq;
using UnityEngine;

namespace Fullscreen.LogicBlock.Runtime
{
    [Serializable]
    public class BlockInstructionReference
    {
        [SerializeField] private Block m_Block;
        [SerializeField] private string m_InstructionName;
        [SerializeField] private string m_InstructionId;

        public Block Block => m_Block;
        public string InstructionName => m_InstructionName;
        public string InstructionId => m_InstructionId;

        public BlockInstructionReference()
        {
            m_Block = null;
            m_InstructionName = string.Empty;
            m_InstructionId = string.Empty;
        }

        public void SetInstruction(string name, Block block)
        {
            m_Block = block;
            m_InstructionName = name;
            var targetList = block?.ActionLists.FirstOrDefault(list =>
                list.ListName.Equals(name, StringComparison.OrdinalIgnoreCase));
            m_InstructionId = targetList?.UniqueId ?? string.Empty;
        }

        public string GetInstructionName()
        {
            if (string.IsNullOrEmpty(m_InstructionId)) return m_InstructionName;
            var targetList = m_Block?.ActionLists.FirstOrDefault(list =>
                list.UniqueId.Equals(m_InstructionId, StringComparison.Ordinal));
            string name = targetList != null ? targetList.ListName : m_InstructionName;
            return name;
        }
    }
}