using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

public class MySubSceneLoader : ComponentSystem
{
    private SceneSystem scene_system;
    static bool load = false;

    protected override void OnCreate()
    {
        scene_system = World.GetOrCreateSystem<SceneSystem>();
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!load)
            {
                LoadSubScene(SubSceneReference.instance_.map1);
                load = true;
            }
        }
            
    }

    private void LoadSubScene(SubScene sub_scene_)
    {
        scene_system.LoadSceneAsync(sub_scene_.SceneGUID);
    }
}
