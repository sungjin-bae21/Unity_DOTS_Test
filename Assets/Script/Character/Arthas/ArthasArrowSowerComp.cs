using Unity.Entities;
using UnityEngine;


[GenerateAuthoringComponent]
public struct ArthasArrowShowerComp : IComponentData
{
    public float radius;
    public float tick;
    public float interval;
    public float duration;

    public ArthasArrowShowerComp(float radius_, float tick_, float interval_, float duration_)
    {
        radius = radius_;
        tick = tick_;
        interval = interval_;
        duration = duration_;
    }
}
