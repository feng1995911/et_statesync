using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Stats;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Stat Reference")]
    [Category("LogicBlock/Block Stat Reference")]
    [Image(typeof(IconStat), ColorTheme.Type.TextLight)]
    [Description("A stat reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetStatReference : PropertyTypeGetStat
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Stat Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for stat.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Stat stat ? stat : null;
        }

        public override Stat Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetStatReference() : base() { }

        public BlockGetStatReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetStat Create() => new PropertyGetStat(new BlockGetStatReference());

        public static PropertyGetStat Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetStat(new BlockGetStatReference(block, instructionListId, referenceName));

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