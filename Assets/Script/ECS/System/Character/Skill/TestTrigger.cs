using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Assertions;
using System;


[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class SkillCollisionSystem : SystemBase
{
    BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    StepPhysicsWorld m_StepPhysicsWorldSystem;
    EntityQuery m_TriggerGroup;

    protected override void OnCreate()
    {
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();

        m_TriggerGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(ArthasArrowShowerComp)
            }
        });
    }

    struct Skill_ArrowShowerJob : ITriggerEventsJob
    {
        public ComponentDataFromEntity<ArthasArrowShowerComp> SkillGroup;
        public ComponentDataFromEntity<HpComponent> HpGroup;

        public void Execute(TriggerEvent triggerEvent)
        {

            Entity skill = Entity.Null;
            Entity hp = Entity.Null;

            if (SkillGroup.HasComponent(triggerEvent.EntityA) &&
                HpGroup.HasComponent(triggerEvent.EntityB))
            {
                skill = triggerEvent.EntityA;
                hp = triggerEvent.EntityB;
            }

            if (HpGroup.HasComponent(triggerEvent.EntityA) &&
                SkillGroup.HasComponent(triggerEvent.EntityB))
            {
                hp = triggerEvent.EntityA;
                skill = triggerEvent.EntityB;
            }

            Assert.IsTrue(skill != Entity.Null && hp != Entity.Null);

            ArthasArrowShowerComp skill_comp = SkillGroup[skill];
            HpComponent hp_comp = HpGroup[hp];

            if (!skill_comp.excute)
            {
                return;
            }
        }
    }

    protected override void OnUpdate()
    {
        if (m_TriggerGroup.CalculateEntityCount() == 0)
        {
            return;
        }

        Dependency = new Skill_ArrowShowerJob
        {
            SkillGroup = GetComponentDataFromEntity<ArthasArrowShowerComp>(),
            HpGroup = GetComponentDataFromEntity<HpComponent>()
        }.Schedule(
            m_StepPhysicsWorldSystem.Simulation, ref m_BuildPhysicsWorldSystem.PhysicsWorld, Dependency);
    }
}
