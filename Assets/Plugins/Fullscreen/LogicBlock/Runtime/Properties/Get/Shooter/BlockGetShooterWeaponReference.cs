using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Shooter;


namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Shooter Weapon Reference")]
    [Category("LogicBlock/Block Shooter Weapon Reference")]
    [Image(typeof(IconPistol), ColorTheme.Type.TextLight)]
    [Description("A weapon shooter reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetShooterWeaponReference : PropertyTypeGetWeapon
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override IWeapon Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for weapon.");
                return null;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}".Replace(" ", "_").ToLowerInvariant();
            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);

            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);
            return reference is IWeapon weapon ? weapon : null;
        }

        public override IWeapon Get(GameObject gameObject)
        {
            return Get(new Args(gameObject));
        }

        public BlockGetShooterWeaponReference() : base() { }

        public BlockGetShooterWeaponReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetWeapon Create() => new PropertyGetWeapon(new BlockGetShooterWeaponReference());

        public static PropertyGetWeapon Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetWeapon(new BlockGetShooterWeaponReference(block, instructionListId, referenceName));

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