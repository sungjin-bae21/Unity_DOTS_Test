using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = UnityEngine.RaycastHit;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using System;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
[UpdateAfter(typeof(SampleCubeInput))]
public class ClientMouseInputSystem : ComponentSystem
{
    private const float RAYCAST_DISTANCE = 1000;
    private BuildPhysicsWorld physicsWorld;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();

        physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
    }
    

    protected override void OnUpdate()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Entity localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            return;
        }

        var screenPointToRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        LayerMask ground = LayerMask.GetMask("Default");
        LayerMask mouse_ray = LayerMask.GetMask("MouseRay");
        CollisionFilter collision_filter = LayerUtil.LayerMaskToFilter(mouse_ray, ground);
        var raycast_input = new RaycastInput
        {
            Start = screenPointToRay.origin,
            End = screenPointToRay.origin + (screenPointToRay.direction * RAYCAST_DISTANCE),
            Filter = collision_filter
        };

        if (!physicsWorld.PhysicsWorld.CollisionWorld.CastRay(raycast_input, out var hit))
        {
            Debug.Log("Not mouse click");
            return;
        }

        //Debug.Log(String.Format("client hit position x : {0} y: {1} z : {2}", hit.Position.x, hit.Position.y, hit.Position.z));
        MouseInputCommand input = new MouseInputCommand();
        input.position = hit.Position;

        var req = PostUpdateCommands.CreateEntity();
        PostUpdateCommands.AddComponent(req, input);

        // 타겟을 추가해야 할거같은데;; 예상으로 묵시적으로 서버에 전송될듯?
        // 나중에 추가하자
        PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent());
    }
}