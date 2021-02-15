using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using NavJob.Components;
using NavJob.Systems;
using UnityEngine;
using System;
using Unity.Mathematics;
using Unity.Collections;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerProcessNavInputRpcSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;

        Entities.ForEach(
            (Entity ent, ref NavInputRpc cmd, ref ReceiveRpcCommandRequestComponent req) =>
            {
                float3 pos = cmd.position;
                //Debug.Log(String.Format("Server Mouse Input data com x : {0} , y: {1} z: {2}", pos.x, pos.y, pos.z));
                int client_id = EntityManager.GetComponentData<NetworkIdComponent>(req.SourceConnection).Value;
                // Debug.Log(String.Format("client id : {0}", client_id));
                MovePlayerAgent(client_id, cmd.position);
                PostUpdateCommands.DestroyEntity(ent);
            });
    }


    void MovePlayerAgent(int client_id, float3 pos)
    {
        Entities.ForEach(
            (ref CommandTargetComponent command_target, ref NetworkIdComponent network_id) =>
            {
                if (network_id.Value != client_id)
                {
                    return;
                }

                Entity ent = command_target.targetEntity;
                if (ent == Entity.Null)
                {
                    Debug.LogError("Not Init target entity");
                    return;
                }

                NavAgent agent =  EntityManager.GetComponentData<NavAgent>(ent);
                NavAgentSystem.SetDestinationStatic(ent, agent, pos);
            });
        
    }
}
