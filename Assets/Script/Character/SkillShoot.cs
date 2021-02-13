using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SkillShootSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        // Rpc 를 추가해야한다.ㄴ

    }
}
