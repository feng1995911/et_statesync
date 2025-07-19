using System;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;


namespace Fullscreen.LogicBlock.Runtime
{
    [Serializable]
    public class BlockReferenceStorage
    {
        [Serializable]
        public class ReferenceEntry
        {
            public string InstructionListName;
            public string ReferenceName;
            public object Value;

            public ReferenceEntry(string listName, string refName, object value)
            {
                InstructionListName = listName;
                ReferenceName = refName;
                Value = value;
            }
        }

        private List<ReferenceEntry> _references = new List<ReferenceEntry>();

        public void UpdateReference(string listName, string refName, object value)
        {
            if (string.IsNullOrEmpty(listName) || string.IsNullOrEmpty(refName)) return;

            var existing = _references.Find(r =>
                r.InstructionListName.Equals(listName, StringComparison.OrdinalIgnoreCase) &&
                r.ReferenceName.Equals(refName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Value = value;
            }
            else
            {
                _references.Add(new ReferenceEntry(listName, refName, value));
            }
        }

        public bool HasReference(string listName, string refName)
        {
            return _references.Exists(r =>
                r.InstructionListName.Equals(listName, StringComparison.OrdinalIgnoreCase) &&
                r.ReferenceName.Equals(refName, StringComparison.OrdinalIgnoreCase));
        }

        public object GetReference(string listName, string refName)
        {
            if (string.IsNullOrEmpty(listName) || string.IsNullOrEmpty(refName)) return null;

            return _references.Find(r =>
                r.InstructionListName.Equals(listName, StringComparison.OrdinalIgnoreCase) &&
                r.ReferenceName.Equals(refName, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public IReadOnlyList<ReferenceEntry> References => _references;
    }
}