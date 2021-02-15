using Unity.Entities;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct NavTargetComponent : IComponentData
{
    public bool has_target;
}