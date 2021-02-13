using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct CharacterComp : IComponentData
{
    public CharacterClass character_class;
}
