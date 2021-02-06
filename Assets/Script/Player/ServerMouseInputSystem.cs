using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using NavJob.Components;
using NavJob.Systems;
using UnityEngine;
using System;
using Unity.Mathematics;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class ServerMouseInputSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;

        Entities.ForEach(
            (Entity ent, DynamicBuffer<MouseInputComponent> inputBuffer, ref NavAgent agent) =>
            {
                Debug.Log(String.Format("MouseInput buffer length {0}", inputBuffer.Length));
                MouseInputComponent input;
                inputBuffer.GetDataAtTick(tick, out input);
                if (!input.isNew)
                {
                    return;
                }

                

                float3 pos = input.position;
                //Debug.Log(String.Format("Ghost prediction recv input data x : {0} y: {1} z: {2} ", pos.x, pos.y, pos.z));
                Debug.Log(ent.Index.ToString());
                NavAgentSystem.SetDestinationStatic(ent, agent, input.position);
            });
    }
}
