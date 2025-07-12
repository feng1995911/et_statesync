using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Color Reference")]
    [Category("LogicBlock/Block Color Reference")]
    [Image(typeof(IconColor), ColorTheme.Type.TextLight)]
    [Description("A color reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetColorReference : PropertyTypeGetColor
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Color Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for color.");
                return Color.clear;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Color color ? color : Color.clear;
        }

        public override Color Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetColorReference() : base() { }

        public BlockGetColorReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetColor Create() => new PropertyGetColor(new BlockGetColorReference());

        public static PropertyGetColor Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetColor(new BlockGetColorReference(block, instructionListId, referenceName));

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