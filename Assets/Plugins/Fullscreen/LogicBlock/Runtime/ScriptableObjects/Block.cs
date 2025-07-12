using System;
using System.Linq;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace Fullscreen.LogicBlock.Runtime
{
    [CreateAssetMenu(fileName = "New Block", menuName = "Game Creator/LogicBlock/Block", order = 1)]
    [Icon("Assets/Plugins/Fullscreen/LogicBlock/Runtime/Icons/Block.png")]
    public class Block : ScriptableObject
    {
        [SerializeField] private List<NamedInstructionList> m_ActionLists = new List<NamedInstructionList>();
        [SerializeField] private List<NamedBranchList> m_BranchLists = new List<NamedBranchList>();
        [SerializeField] public UniqueID m_UniqueId;

        public IReadOnlyList<NamedInstructionList> ActionLists => m_ActionLists;
        public IReadOnlyList<NamedBranchList> BranchLists => m_BranchLists;
        public IdString ID => this.m_UniqueId.Get;

        public async void Execute(string listName, Args args)
        {
            foreach (var actionList in m_ActionLists)
            {
                if (actionList.ListName.Equals(listName, StringComparison.OrdinalIgnoreCase))
                {
                    if (actionList.Instructions != null)
                    {
                        await actionList.Instructions.Run(args);
                    }
                    return;
                }
            }

            foreach (var branchList in m_BranchLists)
            {
                if (branchList.ListName.Equals(listName, StringComparison.OrdinalIgnoreCase))
                {
                    if (branchList.Branches != null)
                    {
                        await branchList.Branches.Evaluate(args);
                    }
                    return;
                }
            }
        }

        public List<string> GetInstructionListNames()
        {
            var names = new List<string>();
            foreach (var list in m_ActionLists)
            {
                if (!string.IsNullOrEmpty(list.ListName))
                {
                    names.Add(list.ListName);
                }
            }
            return names;
        }

        public List<string> GetBranchListNames()
        {
            var names = new List<string>();
            foreach (var list in m_BranchLists)
            {
                if (!string.IsNullOrEmpty(list.ListName))
                {
                    names.Add(list.ListName);
                }
            }
            return names;
        }

        public void SetUniqueID(string newId)
        {
            m_UniqueId = new UniqueID(newId);
        }
    }

    [Serializable]
    public class NamedInstructionList : INamedList
    {
        [SerializeField] public string m_ListName;
        [SerializeField] public InstructionList m_Instructions;
        [SerializeField] public List<ReferenceWrapper> m_References = new();
        [SerializeField] private string m_UniqueId;
        [SerializeField] private string m_IconTypeName;
        [SerializeField] private ColorTheme.Type m_IconColor;

        public List<ReferenceWrapper> References => m_References;
        public string ListName => m_ListName;
        public string UniqueId => m_UniqueId;
        public InstructionList Instructions => m_Instructions;
        public string IconTypeName => m_IconTypeName;
        public ColorTheme.Type IconColor => m_IconColor;

        public NamedInstructionList()
        {
            m_UniqueId = Guid.NewGuid().ToString();
            m_IconTypeName = "DefaultIcon";
            m_IconColor = ColorTheme.Type.TextNormal;
        }

        public NamedInstructionList(string listName, InstructionList instructions, List<ReferenceWrapper> references)
        {
            m_ListName = listName;
            m_Instructions = instructions;
            m_References = references ?? new List<ReferenceWrapper>();
            m_UniqueId = Guid.NewGuid().ToString();
            m_IconTypeName = "DefaultIcon";
            m_IconColor = ColorTheme.Type.TextNormal;
        }

        public void RegenerateUniqueId()
        {
            if (string.IsNullOrEmpty(m_UniqueId))
            {
                m_UniqueId = Guid.NewGuid().ToString();
            }
        }

        public void SetIconData(string iconTypeName, ColorTheme.Type color)
        {
            m_IconTypeName = iconTypeName;
            m_IconColor = color;
        }
    }

    [Serializable]
    public class NamedBranchList : INamedList
    {
        [SerializeField] private string m_ListName = "BranchList";
        [SerializeField] private List<ReferenceWrapper> m_References = new();
        [SerializeField] private BranchList m_Branches = new BranchList();
        [SerializeField] private string m_UniqueId;
        [SerializeField] private string m_IconTypeName;
        [SerializeField] private ColorTheme.Type m_IconColor;
        public List<ReferenceWrapper> References => m_References;
        public string ListName => m_ListName;
        public string UniqueId => m_UniqueId;
        public BranchList Branches => m_Branches;
        public string IconTypeName => m_IconTypeName;
        public ColorTheme.Type IconColor => m_IconColor;

        public NamedBranchList()
        {
            m_UniqueId = Guid.NewGuid().ToString();
            m_IconTypeName = "DefaultIcon";
            m_IconColor = ColorTheme.Type.TextNormal;
        }

        public NamedBranchList(string listName, BranchList branches, List<ReferenceWrapper> references)
        {
            m_ListName = listName;
            m_Branches = branches ?? new BranchList();
            m_References = references ?? new List<ReferenceWrapper>();
            m_UniqueId = Guid.NewGuid().ToString();
            m_IconTypeName = "DefaultIcon";
            m_IconColor = ColorTheme.Type.TextNormal;
        }

        public void RegenerateUniqueId()
        {
            if (string.IsNullOrEmpty(m_UniqueId))
            {
                m_UniqueId = Guid.NewGuid().ToString();
            }
        }

        public void SetIconData(string iconTypeName, ColorTheme.Type color)
        {
            m_IconTypeName = iconTypeName;
            m_IconColor = color;
        }
    }
}