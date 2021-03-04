using UnityEngine;
using Unity.Entities;

public enum DefaultAnimState
{
    none,
    idle,
    run,
    skill
};

[GenerateAuthoringComponent]
public struct DefaultAnimStateComponent : IComponentData
{
    public DefaultAnimState influenced_by_extenal;
    public DefaultAnimState pre_state ;
}
