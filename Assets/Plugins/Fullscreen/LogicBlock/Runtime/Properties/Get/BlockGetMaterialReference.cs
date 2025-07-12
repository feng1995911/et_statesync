using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Material Reference")]
    [Category("LogicBlock/Block Material Reference")]
    [Image(typeof(IconMaterial), ColorTheme.Type.TextLight)]
    [Description("A material reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetMaterialReference : PropertyTypeGetMaterial
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Material Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for material.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Material material ? material : null;
        }

        public override Material Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetMaterialReference() : base() { }

        public BlockGetMaterialReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetMaterial Create() => new PropertyGetMaterial(new BlockGetMaterialReference());

        public static PropertyGetMaterial Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetMaterial(new BlockGetMaterialReference(block, instructionListId, referenceName));

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