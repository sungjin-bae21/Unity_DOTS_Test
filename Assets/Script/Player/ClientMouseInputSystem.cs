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
        var raycast_input = new RaycastInput
        {
            Start = screenPointToRay.origin,
            End = screenPointToRay.origin + (screenPointToRay.direction * RAYCAST_DISTANCE),
            /*Filter = new CollisionFilter
            {
                BelongsTo = 1u << 6,
                CollidesWith = 1u << 3,
            }*/
            Filter = CollisionFilter.Default
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
        PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent());
    }
}