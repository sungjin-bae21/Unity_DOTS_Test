using Unity.Entities;
using Unity.Collections;


[GenerateAuthoringComponent]
public struct SkillShape_Rect : IComponentData
{
    public float with;
    public float height;

    public SkillShape_Rect(float with_, float height_)
    {
        with = with_;
        height = height_;
    }
}
