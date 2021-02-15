using Unity.Entities;

[GenerateAuthoringComponent]
public struct ArthasArrowShowerComponent : IComponentData
{
    public float radius;
    public int max_tick;
    public float interval_time;

    // 스킬 지속에 관한 정보.
    public float time;
    public int tick;
    public bool excute;

    public ArthasArrowShowerComponent(float radius_, int max_tick_, float interval_time_,
                                      float time_ = 0, int tick_ = 0, bool excute_ = false)
    {
        radius = radius_;
        max_tick = max_tick_;
        interval_time = interval_time_;
        time = time_;
        tick = tick_;
        excute = excute_;
    }
}
