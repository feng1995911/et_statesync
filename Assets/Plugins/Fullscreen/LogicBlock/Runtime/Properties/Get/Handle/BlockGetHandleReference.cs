using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Characters;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Handle Reference")]
    [Category("LogicBlock/Block Handle Reference")]
    [Image(typeof(IconHandle), ColorTheme.Type.TextLight)]
    [Description("A handle reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetHandleReference : PropertyTypeGetHandle
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Handle Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for handle.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Handle handle ? handle : null;
        }

        public override Handle Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetHandleReference() : base() { }

        public BlockGetHandleReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetHandle Create() => new PropertyGetHandle(new BlockGetHandleReference());

        public static PropertyGetHandle Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetHandle(new BlockGetHandleReference(block, instructionListId, referenceName));

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