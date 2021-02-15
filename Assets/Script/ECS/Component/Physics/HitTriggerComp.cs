using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct HitTriggerComp : IComponentData
{
    public bool is_player;
}
