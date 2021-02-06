#region

using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using NavJob.Components;
using Unity.NetCode;

#endregion

namespace NavJob.Systems
{

    ///[DisableAutoCreation]
    [UpdateInGroup(typeof(ServerSimulationSystemGroup))]
    public class NavAgentSystem : JobComponentSystem
    {

        private struct AgentData
        {
            public int index;
            public Entity entity;
            public NavAgent agent;
        }

        private NativeQueue<AgentData> needsWaypoint;
        private ConcurrentDictionary<int, Vector3[]> waypoints = new ConcurrentDictionary<int, Vector3[]>();
        private NativeHashMap<int, AgentData> pathFindingData;
        [BurstCompile]
        private struct DetectNextWaypointJob : IJobForEachWithEntity<NavAgent>
        {
            public int navMeshQuerySystemVersion;

            public void Execute(Entity entity, int index, ref NavAgent agent)
            {
                //Debug.Log("DectectNextWayPointJob working");
                if (agent.remainingDistance - agent.stoppingDistance > 0 || agent.status != AgentStatus.Moving)
                {
                    return;
                }
                if (agent.nextWaypointIndex != agent.totalWaypoints)
                {
                    agent.nextWayPoint = true;
                }
                else if (navMeshQuerySystemVersion != agent.queryVersion || agent.nextWaypointIndex == agent.totalWaypoints)
                {
                    agent.totalWaypoints = 0;
                    agent.currentWaypoint = 0;
                    agent.status = AgentStatus.Idle;
                }
            }


        }

        private struct SetNextWaypointJob : IJobForEachWithEntity<NavAgent>
        {

            public void Execute(Entity entity, int index, ref NavAgent agent)
            {
                //Debug.Log("Set NextWayPointJob working");
                if (agent.nextWayPoint)
                {
                    if (NavAgentSystem.instance.waypoints.TryGetValue(entity.Index, out Vector3[] currentWaypoints))
                    {

                        agent.currentWaypoint = currentWaypoints[agent.nextWaypointIndex];
                        agent.remainingDistance = Vector3.Distance(agent.position, agent.currentWaypoint);
                        agent.nextWaypointIndex++;
                        agent.nextWayPoint = false;
                    }
                }
            }


        }

        [BurstCompile]
        private struct MovementJob : IJobForEachWithEntity<NavAgent>
        {
            private readonly float dt;
            private readonly float3 up;
            private readonly float3 one;



            public MovementJob(float dt)
            {
                this.dt = dt;
                up = Vector3.up;
                one = Vector3.one;
            }

            public void Execute(Entity entity, int index, ref NavAgent agent)
            {
                //Debug.Log("Movement jop update");
                if (agent.status != AgentStatus.Moving)
                {
                    return;
                }

                if (agent.remainingDistance > 0)
                {
                    
                    agent.currentMoveSpeed = Mathf.Lerp(agent.currentMoveSpeed, agent.moveSpeed, dt * agent.acceleration);
                    // todo: deceleration
                    if (agent.nextPosition.x != Mathf.Infinity)
                    {
                        agent.position = agent.nextPosition;
                    }
                    var heading = (Vector3)(agent.currentWaypoint - agent.position);
                    agent.remainingDistance = heading.magnitude;
                    
                    var targetRotation = Quaternion.LookRotation(heading, up).eulerAngles;
                    targetRotation.x = targetRotation.z = 0;
                    agent.rotation = Quaternion.Euler(targetRotation);
                    /*
                    Debug.Log(
                    string.Format("rotation value : {0} {1} {2} {3}", agent.rotation.x, agent.rotation.y, agent.rotation.z, agent.rotation.w)
                    );
                    */
                    
                    
                    var forward = math.forward(agent.rotation) * agent.currentMoveSpeed * dt;
                    //Debug.Log("move forward x value : " + (agent.currentMoveSpeed * dt).ToString());
                    agent.nextPosition = agent.position + forward;
                    //Debug.Log("agent next position x : " + agent.nextPosition.x.ToString());
                }
                else if (agent.nextWaypointIndex == agent.totalWaypoints)
                {
                    
                    agent.nextPosition = new float3 { x = Mathf.Infinity, y = Mathf.Infinity, z = Mathf.Infinity };
                    agent.status = AgentStatus.Idle;

                }
            }


        }

        private struct InjectData
        {

            [ReadOnly] public NativeArray<Entity> Entities;
            public NativeArray<NavAgent> Agents;
        }




        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //Debug.Log("WTF NavAgent System not working");
            var dt = Time.DeltaTime;
            inputDeps = new DetectNextWaypointJob { navMeshQuerySystemVersion = querySystem.Version }.Schedule(this, inputDeps);
            inputDeps = new SetNextWaypointJob().Schedule(this, inputDeps);
            inputDeps = new MovementJob(dt).Schedule(this, inputDeps);
            return inputDeps;
        }

        /// <summary>
        /// Used to set an agent destination and start the pathfinding process
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="agent"></param>
        /// <param name="destination"></param>
        public void SetDestination(Entity entity, NavAgent agent, Vector3 destination, int areas = -1)
        {
            if (pathFindingData.TryAdd(entity.Index, new AgentData { index = entity.Index, entity = entity, agent = agent }))
            {
                agent.status = AgentStatus.PathQueued;
                agent.destination = destination;
                agent.queryVersion = querySystem.Version;
                EntityManager.SetComponentData(entity, agent);
                querySystem.RequestPath(entity.Index, agent.position, agent.destination, areas);
            }
        }

        /// <summary>
        /// Static counterpart of SetDestination
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="agent"></param>
        /// <param name="destination"></param>
        public static void SetDestinationStatic(Entity entity, NavAgent agent, Vector3 destination, int areas = -1)
        {
            
            //Debug.Log(string.Format("NavAgentSystem destination x : {0} y: {1} , z : {2}", destination.x, destination.y, destination.z));
            instance.SetDestination(entity, agent, destination, areas);
        }

        protected static NavAgentSystem instance;
        private NavMeshQuerySystem querySystem;
        private EntityQuery navAgentQuery;
        protected override void OnCreate()
        {
            querySystem = EntityManager.World.GetOrCreateSystem<NavMeshQuerySystem>();
            World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(querySystem);
            instance = this;
            querySystem.RegisterPathResolvedCallback(OnPathSuccess);
            querySystem.RegisterPathFailedCallback(OnPathError);
            needsWaypoint = new NativeQueue<AgentData>(Allocator.Persistent);
            pathFindingData = new NativeHashMap<int, AgentData>(0, Allocator.Persistent);
            navAgentQuery = GetEntityQuery(typeof(NavAgent));
        }

        protected override void OnDestroy()
        {
            needsWaypoint.Dispose();
            pathFindingData.Dispose();
        }

        private void SetWaypoint(Entity entity, NavAgent agent, Vector3[] newWaypoints)
        {
            waypoints[entity.Index] = newWaypoints;
            agent.status = AgentStatus.Moving;
            agent.nextWaypointIndex = 1;
            agent.totalWaypoints = newWaypoints.Length;
            agent.currentWaypoint = newWaypoints[0];
            agent.remainingDistance = Vector3.Distance(agent.position, agent.currentWaypoint);
            EntityManager.SetComponentData(entity, agent);
        }

        private void OnPathSuccess(int index, Vector3[] waypoints)
        {
            //Debug.Log("Path success");
            if (pathFindingData.TryGetValue(index, out AgentData entry))
            {
                SetWaypoint(entry.entity, entry.agent, waypoints);
                pathFindingData.Remove(index);
            }
        }

        private void OnPathError(int index, PathfindingFailedReason reason)
        {
            //Debug.Log("Path error ");
            if (pathFindingData.TryGetValue(index, out AgentData entry))
            {
                entry.agent.status = AgentStatus.Idle;
                EntityManager.SetComponentData(entry.entity, entry.agent);
                pathFindingData.Remove(index);
            }
        }
    }
}