using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Melee;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Shield Reference")]
    [Category("LogicBlock/Block Shield Reference")]
    [Image(typeof(IconShieldSolid), ColorTheme.Type.TextLight)]
    [Description("A shield reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetShieldReference : PropertyTypeGetShield
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override IShield Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for shield.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Shield shield ? shield : null;
        }

        public override IShield Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetShieldReference() : base() { }

        public BlockGetShieldReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetShield Create() => new PropertyGetShield(new BlockGetShieldReference());

        public static PropertyGetShield Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetShield(new BlockGetShieldReference(block, instructionListId, referenceName));

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