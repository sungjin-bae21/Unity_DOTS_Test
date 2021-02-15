using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct DamageableComponent : IComponentData
{
    public bool is_player;
}
