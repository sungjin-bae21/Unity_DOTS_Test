using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class RequestInstanceCharacter : ComponentSystem
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
                CharacterInstanceInfo info =
                    new CharacterInstanceInfo(CharacterClass.Arthas,
                                              "Arthas_ArrowShower",
                                              "none",
                                              "none",
                                              "none");
                PostUpdateCommands.AddComponent(req, info);
                PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
            });
    }
}
