using UnityEngine;
using System.Collections.Generic;


namespace Fullscreen.LogicBlock.Runtime
{
    public interface INamedList
    {
        string ListName { get; }
        string UniqueId { get; }
        List<ReferenceWrapper> References { get; }
    }
}
