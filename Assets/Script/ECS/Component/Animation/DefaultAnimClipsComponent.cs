using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

// 기본 애니메이션만 들어간다.
// 스킬과 같은 애니메이션은 기본이 아니다.
public struct DefaultAnimClipsComponent : IComponentData
{
    public BlobAssetReference<Clip> idle;
    public BlobAssetReference<Clip> run;
}


public struct PlayClipStateComponent : ISystemStateComponentData
{
    public GraphHandle Graph;
    public NodeHandle<ClipPlayerNode> ClipPlayerNode;
}
