using UnityEngine;
using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Collections;
using Unity.Entities;


public struct AnimationClipsComponent : IComponentData
{
    public BlobAssetReference<Clip> clip;
}


public struct PlayClipStateComponent : ISystemStateComponentData
{
    public GraphHandle Graph;
    public NodeHandle<ClipPlayerNode> ClipPlayerNode;
}
