using System;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

// When server receives go in game request, go in game and delete request
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class GoInGameServerSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref GoInGameRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
            UnityEngine.Debug.Log(String.Format("Server setting connection {0} to in game",EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value));
            PostUpdateCommands.DestroyEntity(reqEnt);
        });
    }
}