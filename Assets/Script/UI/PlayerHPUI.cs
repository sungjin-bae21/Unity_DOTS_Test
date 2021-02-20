using Unity.Entities;
using UnityEngine;
using Unity.Collections;

public class PlayerHPUI : MonoBehaviour
{
    private EntityManager manager;
    private EntityQuery hp_ui_query;

    void Start()
    {
        // 여러 클라이언트가 동작하려나?
        // 클라이언트에서만 동작해야한다.
        foreach (var world in World.All)
        {
            if (NetworkUtility.IsClientWorld(world))
            {
                manager = world.EntityManager;
                break;
            }
        }

        var query = new EntityQueryDesc
        {
            Any = new ComponentType[] { typeof(PlayerHPUIComponent) }
        };
        hp_ui_query = manager.CreateEntityQuery(query);
    }


    void LateUpdate()
    {
        var hp_ui_entitys = hp_ui_query.ToEntityArray(Allocator.TempJob);
        Debug.Log("size : " + hp_ui_entitys.Length.ToString());
        for (int i = 0; i < hp_ui_entitys.Length; i++)
        {
            Debug.Log("Success get hp ui entity");
        }
        hp_ui_entitys.Dispose();
    }
}
