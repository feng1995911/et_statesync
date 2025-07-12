using UnityEngine;
using System.Linq;
using System;

namespace Fullscreen.LogicBlock.Runtime
{
    public class BlockInstanceTracker : MonoBehaviour
    {
        public Block BlockInstance { get; private set; }

        private void Awake()
        {
#if UNITY_EDITOR
            hideFlags = HideFlags.HideInInspector;
#endif
        }

        public void SetBlock(Block block)
        {
            BlockInstance = block;
        }

        private void OnDestroy()
        {
            if (BlockInstance == null) return;

            var manager = BlockReferenceManager.Instance;
            if (manager == null) return;

            string blockIdPrefix = $"{BlockInstance.ID.String}_".ToLowerInvariant();
            var keysToRemove = manager.Storages.Keys
                .Where(key => key.StartsWith(blockIdPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                manager.RemoveStorage(key);
            }

            ScriptableObject.DestroyImmediate(BlockInstance);
        }
    }
}