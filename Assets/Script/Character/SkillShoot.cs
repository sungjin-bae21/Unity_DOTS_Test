using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SkillShootSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (!Input.GetKeyUp(KeyCode.F))
        {
            return;
        }

        Entities.ForEach(
            (Entity ent, ref NetworkIdComponent network_id) =>
            {
                var req = PostUpdateCommands.CreateEntity();
                ExcuteSkillRpc info =
                    new ExcuteSkillRpc("Arthas_ArrowShower");
                PostUpdateCommands.AddComponent(req, info);
                PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
            });
    }
}
