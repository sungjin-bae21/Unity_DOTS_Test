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
            if (!manager_.HasComponent<CharacterComponent>(prefab.Value))
            {
                continue;
            }

            CharacterComponent comp = manager_.GetComponentData<CharacterComponent>(prefab.Value);
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
            if (!manager_.HasComponent<SkillComponent>(prefab.Value))
            {
                continue;
            }

            SkillComponent comp = manager_.GetComponentData<SkillComponent>(prefab.Value);
            if (comp.name != skill_)
            {
                continue;
            }

            return prefab.Value;
        }

        Debug.LogError("Not exist skill gost prefab");
        return Entity.Null;
    }


    public static Entity GetUIDataPrefab(EntityManager manager_, Entity collection_)
    {
        DynamicBuffer<GhostPrefabBuffer> prefabs =
          manager_.GetBuffer<GhostPrefabBuffer>(collection_);

        foreach (GhostPrefabBuffer prefab in prefabs)
        {
            if (!manager_.HasComponent<UIEntityToNetworkIDComponent>(prefab.Value))
            {
                continue;
            }

            return prefab.Value;
        }

        Debug.LogError("Not exist UIEntity gost prefab");
        return Entity.Null;

    }
}
