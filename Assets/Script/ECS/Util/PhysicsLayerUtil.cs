using UnityEngine;
using Unity.Physics;
using UnityEngine.Rendering;

public static class PhysicsLayerUtil
{

    public static CollisionFilter LayerMaskToFilter(LayerMask belong_to, LayerMask collision_with)
    {
        CollisionFilter filter = new CollisionFilter()
        {
            BelongsTo = (uint)belong_to.value,
            CollidesWith = (uint)collision_with.value
        };
        return filter;
    }

}