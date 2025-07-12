using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fullscreen.LogicBlock.Runtime
{
    public class BlockReferenceManager : MonoBehaviour
    {
        private static BlockReferenceManager _instance;
        private static bool _isQuitting = false;
        private static bool _hasWarned = false;

        [SerializeField]
        private Dictionary<string, BlockReferenceStorage> _storages = new Dictionary<string, BlockReferenceStorage>();

        public static BlockReferenceManager Instance
        {
            get
            {
                if (_isQuitting)
                {
                    if (!_hasWarned)
                    {
                        _hasWarned = true;
                    }
                    return null;
                }

                if (_instance == null)
                {
                    var go = new GameObject("BlockReferenceManager");
                    go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild;
                    _instance = go.AddComponent<BlockReferenceManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public BlockReferenceStorage GetOrCreateStorage(string storageKey)
        {
            if (!_storages.TryGetValue(storageKey, out var storage))
            {
                storage = new BlockReferenceStorage();
                _storages[storageKey] = storage;
            }
            return storage;
        }

        public void CacheStorage(string storageKey, BlockReferenceStorage storage)
        {
            _storages[storageKey] = storage;
        }

        public void RemoveStorage(string storageKey)
        {
            if (_storages.ContainsKey(storageKey))
            {
                _storages.Remove(storageKey);
            }
        }

        public IReadOnlyDictionary<string, BlockReferenceStorage> Storages => _storages;

        public void LogAllStorages()
        {
            if (_storages.Count == 0)
            {
                return;
            }

            foreach (var kvp in _storages)
            {
                var storage = kvp.Value;
                var references = storage.References;
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            _isQuitting = true;
        }

        private void Awake()
        {
            _isQuitting = false;
            _hasWarned = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetIsQuittingOnLoad()
        {
            _isQuitting = false;
            _hasWarned = false;
        }

        public static void ResetInstance()
        {
            if (_instance != null)
            {
                DestroyImmediate(_instance.gameObject);
                _instance = null;
            }
        }
    }
}
