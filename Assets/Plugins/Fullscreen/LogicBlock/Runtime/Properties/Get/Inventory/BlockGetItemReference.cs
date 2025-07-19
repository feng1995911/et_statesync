using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Item Reference")]
    [Category("LogicBlock/Block Item Reference")]
    [Image(typeof(IconItem), ColorTheme.Type.Green)]
    [Description("An Item reference from a Block asset's runtime storage")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetItemReference : PropertyTypeGetItem
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Item Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for Item.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Item runtimeItem ? runtimeItem : null;
        }

        public override Item Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetItemReference() : base() { }

        public BlockGetItemReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetItem Create() => new PropertyGetItem(new BlockGetItemReference());

        public static PropertyGetItem Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetItem(new BlockGetItemReference(block, instructionListId, referenceName));

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