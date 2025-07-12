using System;
using System.Linq;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace Fullscreen.LogicBlock.Runtime
{
    [Serializable]
    public class PropertyGetBlockListReference
    {
        [SerializeField] private Block m_Block;
        [SerializeField] private string m_InstructionListId = "";
        [SerializeField] private string m_ReferenceId = "";
        [SerializeField] private string m_ReferenceName = "";

        public Block Block => m_Block;
        public string InstructionListId => m_InstructionListId;
        public string ReferenceId => m_ReferenceId;
        public string ReferenceName => m_ReferenceName;

        public void SetBlock(Block block) => m_Block = block;
        public void SetInstructionListId(string id) => m_InstructionListId = id;
        public void SetReferenceId(string id) => m_ReferenceId = id;
        public void SetReferenceName(string name) => m_ReferenceName = name;

        public string GetInstructionListName()
        {
            if (m_Block == null || string.IsNullOrEmpty(m_InstructionListId)) return "(no list)";
            foreach (var actionList in m_Block.ActionLists)
            {
                if (actionList.UniqueId == m_InstructionListId) return actionList.ListName;
            }
            foreach (var branchList in m_Block.BranchLists)
            {
                if (branchList.UniqueId == m_InstructionListId) return branchList.ListName;
            }
            return "(no list)";
        }
    }
}