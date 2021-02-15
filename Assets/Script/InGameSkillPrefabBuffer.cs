using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public struct InGameSkillPrefabBuffer : IBufferElementData
{
	public Entity Value;
	public FixedString64 skill_name;
}