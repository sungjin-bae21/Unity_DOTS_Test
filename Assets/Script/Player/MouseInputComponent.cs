using Unity.NetCode;
using Unity.Mathematics;


public struct MouseInputCommand : IRpcCommand
{
    public float3 position;
}


