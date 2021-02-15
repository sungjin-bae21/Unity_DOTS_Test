using UnityEngine;
using Unity.Entities;

public class AddSkillComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public string skill_name;

    public void Convert(Entity ent, EntityManager manager, GameObjectConversionSystem conversion_system)
    {
        manager.AddComponentData(ent, new SkillComponent() { name = new Unity.Collections.FixedString64(skill_name)});
    }
}