using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct CharacterComponent : IComponentData
{
    public CharacterClass character_class;
}
