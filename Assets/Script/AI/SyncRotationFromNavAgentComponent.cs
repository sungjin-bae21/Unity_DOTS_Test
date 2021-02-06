using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace NavJob.Components
{
    [GenerateAuthoringComponent]
    public struct SyncRotationFromNavAgent : IComponentData {
        public bool aa;
    }

}