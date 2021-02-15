using UnityEngine;
using Unity.Entities;
using Unity.NetCode;


// 스킬 유지, 삭제에 관련된 시스템.
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ArrowShowerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (Entity ent_, ref ArthasArrowShowerComp skill_comp_) =>
            {
                float time = skill_comp_.time + Time.DeltaTime;
                if (time < skill_comp_.interval_time)
                {
                    skill_comp_.time = time;
                    skill_comp_.excute = false;
                    return;
                }

                if (skill_comp_.tick == skill_comp_.max_tick)
                {
                    PostUpdateCommands.DestroyEntity(ent_);
                    return;
                }

                skill_comp_.time = 0;
                skill_comp_.tick += 1;
                skill_comp_.excute = true;
            });       
    }
}
