using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct UIEntityToNetworkIDComponent : IComponentData
{
    public int network_id;
}
