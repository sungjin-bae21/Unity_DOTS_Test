using Unity.Entities;
using Unity.NetCode;

public static class NetworkUtility
{
    public static World GetServerWorld()
    {
        return GetWorld<ClientSimulationSystemGroup>();
    }

    public static World GetClientWorld()
    {
        return GetWorld<ClientSimulationSystemGroup>();
    }

    private static World GetWorld<TWorld>() where TWorld : ComponentSystemGroup
    {
        foreach (var world in World.All)
        {
            if (world.GetExistingSystem<TWorld>() != null)
            {
                return world;
            }
        }
        return null;
    }

    public static bool IsClientWorld(World world)
    {
        return world.GetExistingSystem<ClientSimulationSystemGroup>() != null;
    }

    public static bool IsServerWorld(World world)
    {
        return world.GetExistingSystem<ServerSimulationSystemGroup>() != null;
    }
}