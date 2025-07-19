using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Stats;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Attribute Reference")]
    [Category("LogicBlock/Block Attribute Reference")]
    [Image(typeof(IconAttr), ColorTheme.Type.TextLight)]
    [Description("An attribute reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetAttributeReference : PropertyTypeGetAttribute
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override GameCreator.Runtime.Stats.Attribute Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for attribute.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is GameCreator.Runtime.Stats.Attribute attribute ? attribute : null;
        }

        public override GameCreator.Runtime.Stats.Attribute Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetAttributeReference() : base() { }

        public BlockGetAttributeReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetAttribute Create() => new PropertyGetAttribute(new BlockGetAttributeReference());

        public static PropertyGetAttribute Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetAttribute(new BlockGetAttributeReference(block, instructionListId, referenceName));

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