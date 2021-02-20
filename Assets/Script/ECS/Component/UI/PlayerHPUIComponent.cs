using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerHPUIComponent : IComponentData
{
    public int hp;
}
