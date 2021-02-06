using Unity.NetCode;
using Unity.Mathematics;


public struct MouseInputComponent : ICommandData
{
    public uint Tick { get; set; }
    public float3 position;
    public bool isNew;
}


