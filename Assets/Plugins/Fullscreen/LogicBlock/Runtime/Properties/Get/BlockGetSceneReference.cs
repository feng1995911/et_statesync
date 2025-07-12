using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Fullscreen.LogicBlock.Runtime
{
    [Title("Block Scene Reference")]
    [Category("LogicBlock/Block Scene Reference")]
    [Image(typeof(IconUnity), ColorTheme.Type.TextLight)]
    [Description("A scene build index reference from a Block")]
    [Serializable]
    [HideLabelsInEditor]
    public class BlockGetSceneReference : PropertyTypeGetScene
    {
        [SerializeField] private PropertyGetBlockListReference m_ListReference = new PropertyGetBlockListReference();

        public override int Get(Args args)
        {
            if (m_ListReference == null || m_ListReference.Block == null ||
                string.IsNullOrEmpty(m_ListReference.InstructionListId) ||
                string.IsNullOrEmpty(m_ListReference.ReferenceName))
            {
                Debug.LogWarning("Invalid Block reference parameters for scene.");
                return -1;
            }

            string storageKey = $"{m_ListReference.Block.ID.String}_{m_ListReference.InstructionListId}"
                .Replace(" ", "_")
                .ToLowerInvariant();

            var storage = BlockReferenceManager.Instance.GetOrCreateStorage(storageKey);
            object reference = storage.GetReference(m_ListReference.InstructionListId, m_ListReference.ReferenceName);

            if (reference is SceneReference sceneReference)
            {
                string sceneName = sceneReference.Name;
                if (string.IsNullOrEmpty(sceneName))
                {
                    Debug.LogWarning("SceneReference has an empty or null name.");
                    return -1;
                }

                // Find the scene build index by name
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    if (name == sceneName)
                    {
                        return i;
                    }
                }

                Debug.LogWarning($"Scene '{sceneName}' not found in build settings.");
                return -1;
            }

            Debug.LogWarning("Block reference is not a SceneReference.");
            return -1;
        }

        public override int Get(GameObject gameObject) => Get(new Args(gameObject));

        public BlockGetSceneReference() : base() { }

        public BlockGetSceneReference(Block block, string instructionListId, string referenceName) : this()
        {
            m_ListReference = new PropertyGetBlockListReference();
            m_ListReference.SetBlock(block);
            m_ListReference.SetInstructionListId(instructionListId);
            m_ListReference.SetReferenceName(referenceName);
        }

        public static PropertyGetScene Create() => new PropertyGetScene(new BlockGetSceneReference());

        public static PropertyGetScene Create(Block block, string instructionListId, string referenceName)
            => new PropertyGetScene(new BlockGetSceneReference(block, instructionListId, referenceName));

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