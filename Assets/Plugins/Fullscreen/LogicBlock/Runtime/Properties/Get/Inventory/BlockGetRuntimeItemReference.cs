using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Inventory;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Runtime Item Reference")]
    [Category("LogicBlock/Block Runtime Item Reference")]
    [Image(typeof(IconItem), ColorTheme.Type.TextLight)]
    [Description("A runtime item reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetRuntimeItemReference : PropertyTypeGetRuntimeItem
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override RuntimeItem Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for runtime item.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is RuntimeItem runtimeItem ? runtimeItem : null;
        }

        public override RuntimeItem Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetRuntimeItemReference() : base() { }

        public BlockGetRuntimeItemReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetRuntimeItem Create() => new PropertyGetRuntimeItem(new BlockGetRuntimeItemReference());

        public static PropertyGetRuntimeItem Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetRuntimeItem(new BlockGetRuntimeItemReference(block, instructionListId, referenceName));

        public override string String
        {
            get
            {
                if (m_ListReference.Block == null) return "(none)";
                if (string.IsNullOrEmpty(m_ListReference.InstructionListId)) return $"{m_ListReference.Block.name}/(no list)";
                if (string.IsNullOrEmpty(m_ListReference.ReferenceName)) return $"{m_ListReference.Block.name}/{m_ListReference.GetInstructionListName()}/(no reference)";
                return $"{m_ListReference.Block.name}/{m_ListReference.GetInstructionListName()}/{m_ListReference.ReferenceName}";
            }
        }
    }
}