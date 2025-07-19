using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block GameObject Reference")]
    [Category("LogicBlock/Block GameObject Reference")]
    [Image(typeof(IconCubeSolid), ColorTheme.Type.TextLight)]
    [Description("A GameObject reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetGameObjectReference : PropertyTypeGetGameObject
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override GameObject Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                return null;
            }

            string blockIdString = m_ListReference.Block.ID.String;
            if (string.IsNullOrEmpty(blockIdString))
            {
                return null;
            }

            string storageKey = $"{blockIdString}_{m_ListReference.InstructionListId}"
                .Replace(" ", "_").ToLowerInvariant();

            if (BlockReferenceManager.Instance == null)
            {
                return null;
            }

            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);
            if (storage == null)
            {
                return null;
            }

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            if (reference == null)
            {
                return null;
            }

            if (reference is not GameObject runtimeGO)
            {
                return null;
            }

            return runtimeGO;
        }

        public override GameObject Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetGameObjectReference() : base() { }

        public BlockGetGameObjectReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetGameObject Create() => new PropertyGetGameObject(new BlockGetGameObjectReference());

        public static PropertyGetGameObject Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetGameObject(new BlockGetGameObjectReference(block, instructionListId, referenceName));

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