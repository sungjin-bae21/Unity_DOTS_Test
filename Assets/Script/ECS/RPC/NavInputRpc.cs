using Unity.NetCode;
using Unity.Mathematics;


public struct NavInputCommand : IRpcCommand
{
    public float3 position;
}


