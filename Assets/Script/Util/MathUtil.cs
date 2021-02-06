using Unity.Mathematics;

public static class MathUtil
{
    public static float3 Float3Max()
    {
        float3 value;
        value.x = value.y = value.z = float.MaxValue;
        return value;
    } 
}
