using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Animation;

using Unity.Assertions;


#if UNITY_EDITOR
using Unity.Animation.Hybrid;

public class AddAnimClips : MonoBehaviour, IConvertGameObjectToEntity
{
    //public AnimationClip[] clips;
    public AnimationClip idle;
    public AnimationClip run;

    public void Convert(Entity ent_, EntityManager manager_, GameObjectConversionSystem conversion_system)
    {
        Assert.IsTrue(idle != null);
        Assert.IsTrue(run != null);

        conversion_system.DeclareAssetDependency(gameObject, idle);
        conversion_system.DeclareAssetDependency(gameObject, run);

        manager_.AddComponentData(ent_,
            new DefaultAnimClipsComponent
            {
                idle = conversion_system.BlobAssetStore.GetClip(idle),
                run = conversion_system.BlobAssetStore.GetClip(run)
            });

        manager_.AddComponent<DeltaTime>(ent_);
    }
}
#endif
