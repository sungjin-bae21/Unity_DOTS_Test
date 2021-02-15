using UnityEngine;
using Unity.Entities;

public struct SkillToNetworkIDComponent : IComponentData
{
    public int network_id;
}
