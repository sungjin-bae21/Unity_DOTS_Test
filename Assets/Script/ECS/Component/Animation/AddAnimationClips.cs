using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Animation;

using Unity.Assertions;


#if UNITY_EDITOR
using Unity.Animation.Hybrid;

public class AddAnimationClips : MonoBehaviour, IConvertGameObjectToEntity
{
    //public AnimationClip[] clips;
    public AnimationClip clip;

    public void Convert(Entity ent_, EntityManager manager_, GameObjectConversionSystem conversion_system)
    {
        Assert.IsTrue(clip != null);

        conversion_system.DeclareAssetDependency(gameObject, clip);

        manager_.AddComponentData(ent_,
            new AnimationClipsComponent
            {
                clip = conversion_system.BlobAssetStore.GetClip(clip)
            });

        manager_.AddComponent<DeltaTime>(ent_);
    }
}
#endif
