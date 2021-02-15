using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using UnityEngine;
using System;


[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerSkillExcute : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (Entity ent_, ref ExcuteSkillRpc instance_info_, ref ReceiveRpcCommandRequestComponent reqSrc) =>
            {
                int network_id = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;

                Entity ent = CreateSkill(network_id, instance_info_.skill_name);
                if (ent == Entity.Null)
                {
                    Debug.Log("Failed to create skill, reson : Not found skill prefab");
                }
#if UNITY_EDITOR
                EntityManager.SetName(ent, String.Format("{0}_Instanced", instance_info_.skill_name));
#endif
                EntityManager.DestroyEntity(ent_);
            });
    }


    protected Entity CreateSkill(int network_id_, FixedString64 skill_name_)
    {
        Entity ent = Entity.Null;
        Entities.ForEach(
           (Entity ent_, ref NetworkIDWithSkillPrefabs info_) => {
               if (info_.network_id == network_id_)
               {
                   var buffer = EntityManager.GetBuffer<InGameSkillPrefabBuffer>(ent_);
                   foreach (InGameSkillPrefabBuffer prefab in buffer)
                   {
                       if (prefab.skill_name == skill_name_)
                       {
                           ent = EntityManager.Instantiate(prefab.Value);
                           return;
                       }
                   }  
               }
           });

        return ent;
    }
}
