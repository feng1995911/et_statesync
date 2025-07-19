using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Direction Reference")]
    [Category("LogicBlock/Block Direction Reference")]
    [Image(typeof(IconVector3), ColorTheme.Type.TextLight)]
    [Description("A direction (Vector3) reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetDirectionReference : PropertyTypeGetDirection
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Vector3 Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for direction.");
                return Vector3.zero;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Vector3 direction ? direction : Vector3.zero;
        }

        public override Vector3 Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetDirectionReference() : base() { }

        public BlockGetDirectionReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetDirection Create() => new PropertyGetDirection(new BlockGetDirectionReference());

        public static PropertyGetDirection Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetDirection(new BlockGetDirectionReference(block, instructionListId, referenceName));

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