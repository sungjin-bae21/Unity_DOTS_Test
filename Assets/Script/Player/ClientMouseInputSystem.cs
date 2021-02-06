using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = UnityEngine.RaycastHit;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
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

        //NativeList<RaycastHit> raycast_hit = new NativeList<RaycastHit>();
        //RaycastHit raycast_hit = new RaycastHit();
        if (!physicsWorld.PhysicsWorld.CollisionWorld.CastRay(raycast_input, out var hit))
        {
            Debug.Log("Not mouse click");
            return;
        }

        //Debug.Log(String.Format("client hit position x : {0} y: {1} z : {2}", hit.Position.x, hit.Position.y, hit.Position.z));
        MouseInputComponent input = new MouseInputComponent();
        input.isNew = true;
        input.position = hit.Position;
        input.Tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        DynamicBuffer<MouseInputComponent> inputBuffer =
            EntityManager.GetBuffer<MouseInputComponent>(localInput);
        inputBuffer.AddCommandData(input);

        /*
        var results = new NativeArray<RaycastHit>(1, Allocator.TempJob);
        var commands = new NativeArray<RaycastCommand>(1, Allocator.TempJob);

        commands[0] = new RaycastCommand(screenPointToRay.origin, screenPointToRay.direction);
        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));
        handle.Complete();        

        var input = default(MouseInputComponent);
        input.position = results[0].point; ;
        input.Tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        var inputBuffer = EntityManager.GetBuffer<MouseInputComponent>(localInput);
        inputBuffer.AddCommandData(input);

        results.Dispose();
        commands.Dispose();
        */
    }
}