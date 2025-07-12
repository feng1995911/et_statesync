using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Rotation Reference")]
    [Category("LogicBlock/Block Rotation Reference")]
    [Image(typeof(IconRotation), ColorTheme.Type.TextLight)]
    [Description("A rotation (Quaternion) reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetRotationReference : PropertyTypeGetRotation
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Quaternion Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for rotation.");
                return Quaternion.identity;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Quaternion rotation ? rotation : Quaternion.identity;
        }

        public override Quaternion Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetRotationReference() : base() { }

        public BlockGetRotationReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetRotation Create() => new PropertyGetRotation(new BlockGetRotationReference());

        public static PropertyGetRotation Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetRotation(new BlockGetRotationReference(block, instructionListId, referenceName));

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