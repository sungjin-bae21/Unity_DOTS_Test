using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using NavJob.Components;
using Unity.NetCode;

namespace NavJob.Systems
{

    [UpdateBefore(typeof(NavAgentSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class NavAgentFromPositionSyncSystem : JobComponentSystem
    {

        [BurstCompile]
        [RequireComponentTag(typeof(SyncPositionFromNavAgent))]
        struct SyncPosition : IJobForEach<Translation, NavAgent>
        {
            public void Execute(ref Translation position, [ReadOnly] ref NavAgent agent)
            {
                if (position.Value.Equals(agent.position))
                {
                    return;
                }
                position.Value = agent.position;
            }
        }

        // Any previously scheduled jobs reading/writing from Rotation or writing to RotationSpeed 
        // will automatically be included in the inputDeps dependency.
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new SyncPosition();
            return job.Schedule(this, inputDeps);
        }
    }


    [UpdateBefore(typeof(NavAgentSystem))]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class NavAgentFromRotationSyncSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(SyncRotationFromNavAgent))]
        struct SyncRotation : IJobForEach<Rotation, NavAgent>
        {
            public void Execute(ref Rotation rotation, [ReadOnly] ref NavAgent agent)
            {
                if (rotation.Value.Equals(agent.rotation))
                {
                    return;
                }
                rotation.Value = agent.rotation;
            }
        }

        // Any previously scheduled jobs reading/writing from Rotation or writing to RotationSpeed 
        // will automatically be included in the inputDeps dependency.
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new SyncRotation();
            return job.Schedule(this, inputDeps);
        }
    }
}