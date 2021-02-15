using UnityEngine;
using Unity.Entities;

using NavJob.Components;
public class AddNavAgentComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public float radius = 1;
    public float move_speed = 1;
    public float accelerate = 1;

    public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversion_system)
    {
        NavAgent agent_cmp = new NavAgent(transform.position,
                                          transform.rotation,
                                          radius,
                                          move_speed,
                                          accelerate);

        manager.AddComponentData(entity, agent_cmp);
        manager.AddComponentData(entity, new SyncPositionFromNavAgent());
        manager.AddComponentData(entity, new SyncRotationFromNavAgent());
    }
}
