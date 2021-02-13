using UnityEngine;
using Unity.NetCode;
using Unity.Entities;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class CharacterInstanceSystem : ComponentSystem
{

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<GhostPrefabCollectionComponent>();
    }


    protected override void OnUpdate()
    {
        Entities.ForEach(
            (Entity ent_, ref CharacterInstanceInfo instance_info_, ref ReceiveRpcCommandRequestComponent reqSrc) =>
            {
                // 캐릭터 생성.
                var character =
                    GhostPrefabLoader.GetCharacterInstance(EntityManager,
                                                           GetSingletonEntity<GhostPrefabCollectionComponent>(),
                                                           instance_info_.character_class);
                int network_id = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;
                EntityManager.SetComponentData(character,
                    new GhostOwnerComponent { NetworkId = network_id });

                PostUpdateCommands.AddBuffer<CubeInput>(character);
                PostUpdateCommands.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent { targetEntity = character });


                Entity skill_collections = GetOrCreateSkillCollectionBuffer(network_id);

                var buffer = EntityManager.GetBuffer<InGameSkillPrefabBuffer>(skill_collections);
                var skill_prefab =
                    GhostPrefabLoader.GetSkillPerfab(EntityManager,
                                                     GetSingletonEntity<GhostPrefabCollectionComponent>(),
                                                     instance_info_.skill1);

                buffer.Add(new InGameSkillPrefabBuffer { Value = skill_prefab });
                PostUpdateCommands.DestroyEntity(ent_);
            });
    }

    public Entity GetOrCreateSkillCollectionBuffer(int network_id_)
    {
        Entity ent = Entity.Null;
        Entities.ForEach(
            (Entity ent_, ref NetworkIDWithSkillPrefabs info_) => {
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
        EntityManager.AddComponent<NetworkIDWithSkillPrefabs>(ent);
        EntityManager.SetComponentData<NetworkIDWithSkillPrefabs>(ent, new NetworkIDWithSkillPrefabs { network_id = network_id_ });
        EntityManager.AddBuffer<InGameSkillPrefabBuffer>(ent);
        return ent;
    }

}