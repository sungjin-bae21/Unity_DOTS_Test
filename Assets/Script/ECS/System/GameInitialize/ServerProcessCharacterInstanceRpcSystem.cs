using UnityEngine;
using Unity.NetCode;
using Unity.Entities;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerCharacterInstanceRpcSystem : ComponentSystem
{

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<GhostPrefabCollectionComponent>();
    }


    protected override void OnUpdate()
    {
        Entities.ForEach(
            (Entity ent_, ref CharacterInstanceRpc instance_info_, ref ReceiveRpcCommandRequestComponent req_) =>
            {
                Entity character_prefab =
                    GhostPrefabLoader.GetCharacterPrefab(EntityManager,
                                                         GetSingletonEntity<GhostPrefabCollectionComponent>(),
                                                         instance_info_.character_class);
                Entity character = EntityManager.Instantiate(character_prefab);
#if UNITY_EDITOR
                EntityManager.SetName(character, "PlayerCharacter");
#endif
                int network_id = EntityManager.GetComponentData<NetworkIdComponent>(req_.SourceConnection).Value;
                EntityManager.SetComponentData(character,
                    new GhostOwnerComponent { NetworkId = network_id });

                Entity skill_collections = GetOrCreateSkillCollectionBuffer(network_id);

                var buffer = EntityManager.GetBuffer<InGameSkillPrefabBuffer>(skill_collections);
                var skill_prefab =
                    GhostPrefabLoader.GetSkillPerfab(EntityManager,
                                                     GetSingletonEntity<GhostPrefabCollectionComponent>(),
                                                     instance_info_.skill1);

                buffer.Add(new InGameSkillPrefabBuffer { Value = skill_prefab, skill_name = instance_info_.skill1 });
                PostUpdateCommands.DestroyEntity(ent_);
            });
    }


    public Entity GetOrCreateSkillCollectionBuffer(int network_id_)
    {
        Entity ent = Entity.Null;
        Entities.ForEach(
            (Entity ent_, ref SkillToNetworkIDComponent info_) => {
                if (info_.network_id == network_id_)
                {
                    ent = ent_;
                    return;
                }
            });

        if (ent != Entity.Null)
        {
            return ent;
        }

        ent = EntityManager.CreateEntity();
        EntityManager.AddComponent<SkillToNetworkIDComponent>(ent);
        EntityManager.SetComponentData<SkillToNetworkIDComponent>(ent, new SkillToNetworkIDComponent { network_id = network_id_ });
        EntityManager.AddBuffer<InGameSkillPrefabBuffer>(ent);
        return ent;
    }

}