using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class AddTranslation : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity ent_, EntityManager manager_, GameObjectConversionSystem conversion_system_)
    {
        Vector3 pos = transform.position;
        Quaternion quaternion = transform.rotation;


        Translation trans = new Translation {
            Value = new float3(pos.x, pos.y, pos.z)
        };

        Rotation rot = new Rotation
        {
            Value = quaternion
        };

        if (!manager_.HasComponent<Translation>(ent_))
        {
            Debug.Log("Create Translation");
            manager_.AddComponent<Translation>(ent_);
        }

        if (!manager_.HasComponent<Rotation>(ent_))
        {
            Debug.Log("Create Rotation");
            manager_.AddComponent<Rotation>(ent_);
        }

        if (!manager_.HasComponent<HpComponent>(ent_))
        {
            Debug.Log("Create Hp Component");
            manager_.AddComponent<HpComponent>(ent_);
        }

        if (!manager_.HasComponent<LocalToWorld>(ent_))
        {
            Debug.Log("Create Locatltoworld");
            manager_.AddComponent<LocalToWorld>(ent_);
        }

        manager_.SetComponentData(ent_, trans);
        manager_.SetComponentData(ent_, rot);

        manager_.SetComponentData(ent_, new HpComponent { hp = 10 });
        manager_.SetComponentData(ent_,
            new LocalToWorld
            {
                Value = float4x4.TRS(transform.position, transform.rotation, transform.lossyScale)
            });

        Debug.Log("??????");
    }
}
