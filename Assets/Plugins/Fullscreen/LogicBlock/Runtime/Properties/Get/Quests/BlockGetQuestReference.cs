using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Quests;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Quest Reference")]
    [Category("LogicBlock/Block Quest Reference")]
    [Image(typeof(IconQuestSolid), ColorTheme.Type.TextLight)]
    [Description("A quest reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetQuestReference : PropertyTypeGetQuest
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Quest Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for quest.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Quest quest ? quest : null;
        }

        public override Quest Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetQuestReference() : base() { }

        public BlockGetQuestReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetQuest Create() => new PropertyGetQuest(new BlockGetQuestReference());

        public static PropertyGetQuest Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetQuest(new BlockGetQuestReference(block, instructionListId, referenceName));

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