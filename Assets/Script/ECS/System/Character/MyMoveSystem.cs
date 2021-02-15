using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using NavJob.Components;
using NavJob.Systems;
using UnityEngine;
using Unity.NetCode;
using System;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class MyMoveSystem : ComponentSystem
{
    EntityQuery taget_quey;
    protected override void OnCreate()
    {
        taget_quey = GetEntityQuery(typeof(NavTargetComponent), typeof(Translation));
    }

    protected override void OnUpdate()
    {

        Entities.ForEach(
            (Entity ent, ref Translation trans, ref NavAgent agent) =>
            {
                
                if (Input.GetKeyDown(KeyCode.K))
                {
                    Debug.Log("Setting Destination");
                    // 가장 가까운 타겟 확인.
                    NativeArray<Translation> target_translations =
                        taget_quey.ToComponentDataArray<Translation>(Allocator.Temp);

                    int lenght = target_translations.Length;
                    float min_sqr_len = float.MaxValue;
                    int min_idx = 0;
                    for (int i = 0; i < lenght; ++i)
                    {
                        Vector3 offset = trans.Value - target_translations[i].Value;
                        float sqr_len = offset.sqrMagnitude;

                        if (sqr_len < min_sqr_len)
                        {
                            min_sqr_len = sqr_len;
                            min_idx = i;
                        }
                    }

                    /*Debug.Log(String.Format("SetDestination x : {0} , y : {1}, z: {2}",
                        target_translations[min_idx].Value.x,
                        target_translations[min_idx].Value.y,
                        target_translations[min_idx].Value.z));*/

                    NavAgentSystem.SetDestinationStatic(ent, agent, target_translations[min_idx].Value);
                    target_translations.Dispose();
                }
            });
    }
}
