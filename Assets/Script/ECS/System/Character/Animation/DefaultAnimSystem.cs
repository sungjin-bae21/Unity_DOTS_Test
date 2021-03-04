using Unity.Entities;
using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;
using NavJob.Components;
using System;


// 스킬을 제외한 기본 에니메이션을 업데이트한다.
// 그래프의 생성 및 삭제를 관리함.
[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public class DefaultAnimSystem : SystemBase
{
    int a = 0;
    ProcessDefaultAnimationGraph graph_system;
    EndSimulationEntityCommandBufferSystem ECB_system;
    EntityQuery animation_data_query;

    protected override void OnCreate()
    {
        base.OnCreate();
        graph_system = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();

        graph_system.AddRef();
        
        ECB_system = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        animation_data_query = GetEntityQuery(new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(DefaultAnimClipsComponent) },
            All = new ComponentType[] {typeof(PlayClipStateComponent)}
        });

        graph_system.Set.RendererModel = NodeSet.RenderExecutionModel.Islands;

    }

    protected override void OnDestroy()
    {
        if (graph_system == null)
            return;

        graph_system.RemoveRef();
        base.OnDestroy();
    }


    protected override void OnUpdate()
    {
        
        CompleteDependency();
        EntityCommandBuffer ecb = ECB_system.CreateCommandBuffer();

        // DefaultAnimClipsComponent 를 소유한 엔티티가 있다면 PlayClipsStateComponent 를 생성해준다.
        Entities.
            WithName("CreateGraph").
            WithNone<PlayClipStateComponent>().
            WithoutBurst().
            WithStructuralChanges().
            ForEach((Entity ent_, ref Rig rig, ref DefaultAnimClipsComponent clips_comp) =>
            {
                UnityEngine.Debug.Log("Create Graph foreach");
                var state = CreateGraph(ent_, graph_system, ref rig, ref clips_comp);
                ecb.AddComponent(ent_, state);
            }).Run();

            // 애니메이션 상태를 결정.
            Entities.
                WithoutBurst().
                ForEach((Entity ent_, ref DefaultAnimClipsComponent clips_comp_,
                         ref PlayClipStateComponent clip_state_, ref DefaultAnimStateComponent anim_state_) =>
                {

                    // 외부에서 상태를 변경하고 여기서 업데이트한다.
                    // if (anim_state_.state == DefaultAnimState.hit) 
                    // {
                    //     맞앗을떄 애니메이션 변경.
                    // }
                    // else if ( anim_state_.state == DefaultAnimState.Skill) {
                    //     return;
                    // }

                    // 이 조건은 추후 변경에의해 수정.
                    // 현재는 상태가 아래 두개이기 때문에 상태가 있다면 업데이트를 종료한다.
                    // Idle, Run 움직이거나 안움직이거나.
     
                    if (anim_state_.influenced_by_extenal != DefaultAnimState.none)
                    {
                        return;
                    }

                    NavAgent agent = EntityManager.GetComponentData<NavAgent>(ent_);
                    // 공중이나 피격시 처리는 이후에 추가한다.
                    if (agent.status == AgentStatus.Moving &&
                        anim_state_.pre_state != DefaultAnimState.run)
                    {
                         // 다른 애니메이션은 루프를 신경서야함.
                         graph_system.Set.SendMessage(clip_state_.ClipPlayerNode,
                                                      ClipPlayerNode.SimulationPorts.Clip,
                                                      clips_comp_.run);

                        anim_state_.pre_state = DefaultAnimState.run;
                        UnityEngine.Debug.Log("State Change");
                        return;
                    }
                
                    if (agent.status == AgentStatus.Idle &&
                        anim_state_.pre_state != DefaultAnimState.idle)
                    {
                        graph_system.Set.SendMessage(clip_state_.ClipPlayerNode,
                                                     ClipPlayerNode.SimulationPorts.Clip,
                                                     clips_comp_.idle);

                        anim_state_.pre_state = DefaultAnimState.idle;
                    }
                
                }).Run();


        Entities.
            WithName("DestroyGraph").
            WithNone<DefaultAnimClipsComponent>().
            WithoutBurst().
            WithStructuralChanges().
            ForEach((Entity e, ref PlayClipStateComponent state) =>
            {
                UnityEngine.Debug.Log("Destory Graph foreach");
                graph_system.Dispose(state.Graph);
            }).Run();

        if (animation_data_query.CalculateEntityCount() > 0)
            ecb.RemoveComponent(animation_data_query, typeof(PlayClipStateComponent));
    }

    /// <summary>
    /// The graph executes in the ProcessDefaultAnimationGraph system, but because we connect an EntityNode to the output of the ClipPlayerNode,
    /// the AnimatedData buffer gets updated on the entity and can be used in other systems, such as the ProcessLateAnimationGraph system.
    /// </summary>
    /// <param name="ent_">An entity that has a PlayClipComponent and a Rig.</param>
    /// <param name="graph_system_">The ProcessDefaultAnimationGraph.</param>
    /// <param name="rig_">The rig that will get animated.</param>
    /// <param name="play_clip_">The clip to play.</param>
    /// <returns>Returns a StateComponent containing the NodeHandles of the graph.</returns>
    static PlayClipStateComponent CreateGraph(
        Entity ent_,
        ProcessDefaultAnimationGraph graph_system_,
        ref Rig rig_,
        ref DefaultAnimClipsComponent play_clip_
    )
    {
        GraphHandle graph = graph_system_.CreateGraph();
        var data = new PlayClipStateComponent
        {
            Graph = graph,
            ClipPlayerNode = graph_system_.CreateNode<ClipPlayerNode>(graph)
        };

        var deltaTimeNode = graph_system_.CreateNode<ConvertDeltaTimeToFloatNode>(graph);
        var entityNode = graph_system_.CreateNode(graph, ent_);

        var set = graph_system_.Set;

        // Connect kernel ports
        set.Connect(entityNode, deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
        set.Connect(deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Output, data.ClipPlayerNode, ClipPlayerNode.KernelPorts.DeltaTime);
        set.Connect(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Output, entityNode, NodeSetAPI.ConnectionType.Feedback);

        // Send messages to set parameters on the ClipPlayerNode
        set.SetData(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Speed, 1.0f);
        set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration { Mask = ClipConfigurationMask.LoopTime });
        set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Rig, rig_);
        set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, play_clip_.idle);

        return data;
    }
}
