using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Melee;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Skill Reference")]
    [Category("LogicBlock/Block Skill Reference")]
    [Image(typeof(IconMeleeSkill), ColorTheme.Type.TextLight)]
    [Description("A skill reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetSkillReference : PropertyTypeGetSkill
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override Skill Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for skill.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is Skill skill ? skill : null;
        }

        public override Skill Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetSkillReference() : base() { }

        public BlockGetSkillReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetSkill Create() => new PropertyGetSkill(new BlockGetSkillReference());

        public static PropertyGetSkill Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetSkill(new BlockGetSkillReference(block, instructionListId, referenceName));

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