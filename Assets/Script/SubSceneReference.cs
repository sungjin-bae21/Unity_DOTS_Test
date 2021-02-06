using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Scenes;

public class SubSceneReference : MonoBehaviour
{
    public static SubSceneReference instance_ { get; private set; }
    
    public SubScene map1;

    private void Awake()
    {
        instance_ = this;
    }
}
