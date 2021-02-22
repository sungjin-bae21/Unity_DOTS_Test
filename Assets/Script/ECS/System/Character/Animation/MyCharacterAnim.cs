using Unity.Entities;
using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.NetCode;
using UnityEngine;


[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public class MyCharacterAnim : SystemBase
{
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
            None = new ComponentType[] { typeof(AnimationClipsComponent)},
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

        Entities.
            WithName("CreateGraph").
            WithNone<PlayClipStateComponent>().
            WithoutBurst().
            WithStructuralChanges().
            ForEach((Entity ent_, ref Rig rig, ref AnimationClipsComponent clips_comp) =>
            {
                UnityEngine.Debug.Log("Create Graph foreach");
                var state = CreateGraph(ent_, graph_system, ref rig, ref clips_comp);
                ecb.AddComponent(ent_, state);
            }).Run();


        Entities.
            WithName("UpdateGraph").
            WithChangeFilter<PlayClipStateComponent>().
            WithoutBurst().
            ForEach((Entity e, ref AnimationClipsComponent clips_comp, ref PlayClipStateComponent state) =>
            {
                UnityEngine.Debug.Log("Update Graph foreach");
                graph_system.Set.SendMessage(state.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, clips_comp.clip);
            }).Run();

        Entities.
            WithName("DestroyGraph").
            WithNone<AnimationClipsComponent>().
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
        ref AnimationClipsComponent play_clip_
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
        set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, play_clip_.clip);

        return data;
    }
}
