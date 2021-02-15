using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

public static class GhostPrefabLoader
{
    public static Entity GetCharacterPrefab(EntityManager manager_, Entity collection_,
                                            CharacterClass character_class_)
    {
        DynamicBuffer<GhostPrefabBuffer> prefabs =
            manager_.GetBuffer<GhostPrefabBuffer>(collection_);

        foreach (GhostPrefabBuffer prefab in prefabs)
        {
            if (!manager_.HasComponent<CharacterComp>(prefab.Value))
            {
                continue;
            }

            CharacterComp comp = manager_.GetComponentData<CharacterComp>(prefab.Value);
            if (comp.character_class != character_class_)
            {
                continue;
            }

            return prefab.Value;
        }

        Debug.LogError("Not exist character gost prefab");
        return Entity.Null;
    }

     
    public static Entity GetSkillPerfab(EntityManager manager_, Entity collection_,
                                        FixedString64 skill_)
    {
        if (skill_ == "none")
        {
            Debug.Log("GetSkillPrefab but Skill info none");
            return Entity.Null;
        }

        DynamicBuffer<GhostPrefabBuffer> prefabs =
           manager_.GetBuffer<GhostPrefabBuffer>(collection_);

        foreach (GhostPrefabBuffer prefab in prefabs)
        {
            if (!manager_.HasComponent<SkillComp>(prefab.Value))
            {
                continue;
            }

            SkillComp comp = manager_.GetComponentData<SkillComp>(prefab.Value);
            if (comp.name != skill_)
            {
                continue;
            }

            return prefab.Value;
        }

        Debug.LogError("Not exist skill gost prefab");
        return Entity.Null;
    }
}
