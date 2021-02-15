using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct HpComponent : IComponentData
{
    public int hp;
}
