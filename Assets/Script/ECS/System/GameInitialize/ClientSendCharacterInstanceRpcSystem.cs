using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class ClientSendCharacterInstanceRpcSystem : ComponentSystem
{

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
    }


    protected override void OnUpdate()
    {
        if (!Input.GetKeyUp(KeyCode.A))
        {
            return;
        }

        Entities.ForEach(
            (Entity ent, ref NetworkIdComponent network_id) =>
            {
                var req = PostUpdateCommands.CreateEntity();
                CharacterInstanceRpc info =
                    new CharacterInstanceRpc(CharacterClass.Arthas,
                                              "Arthas_ArrowShower",
                                              "none",
                                              "none",
                                              "none");
                PostUpdateCommands.AddComponent(req, info);
                PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
            });
    }
}
