using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Assertions;
using System;


[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class SkillArrowShowerSystem : SystemBase
{
    BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    StepPhysicsWorld step_physics_world;
    EntityQuery trigger_group;

    protected override void OnCreate()
    {
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        step_physics_world = World.GetOrCreateSystem<StepPhysicsWorld>();

        trigger_group = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(ArthasArrowShowerComponent)
            }
        });
    }

    struct Skill_ArrowShowerJob : ITriggerEventsJob
    {
        public ComponentDataFromEntity<ArthasArrowShowerComponent> skill_group;
        public ComponentDataFromEntity<HpComponent> hp_group;

        public void Execute(TriggerEvent trigger_event_)
        {

            Entity skill = Entity.Null;
            Entity hp = Entity.Null;

            if (skill_group.HasComponent(trigger_event_.EntityA) &&
                hp_group.HasComponent(trigger_event_.EntityB))
            {
                skill = trigger_event_.EntityA;
                hp = trigger_event_.EntityB;
            }

            if (hp_group.HasComponent(trigger_event_.EntityA) &&
                skill_group.HasComponent(trigger_event_.EntityB))
            {
                hp = trigger_event_.EntityA;
                skill = trigger_event_.EntityB;
            }

            Assert.IsTrue(skill != Entity.Null && hp != Entity.Null);

            ArthasArrowShowerComponent skill_comp = skill_group[skill];
            HpComponent hp_comp = hp_group[hp];

            if (!skill_comp.excute)
            {
                return;
            }
        }
    }

    protected override void OnUpdate()
    {
        if (trigger_group.CalculateEntityCount() == 0)
        {
            return;
        }

        Dependency = new Skill_ArrowShowerJob
        {
            skill_group = GetComponentDataFromEntity<ArthasArrowShowerComponent>(),
            hp_group = GetComponentDataFromEntity<HpComponent>()
        }.Schedule(
            step_physics_world.Simulation, ref m_BuildPhysicsWorldSystem.PhysicsWorld, Dependency);
    }
}
