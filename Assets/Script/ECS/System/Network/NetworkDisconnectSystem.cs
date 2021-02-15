using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using System;

public class NetworkDisconnectSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (ref NetworkStreamDisconnected disconnect, ref NetworkIdComponent network_id) =>
            {
                string reson = "";
                switch (disconnect.Reason){
                    case NetworkStreamDisconnectReason.Unknown:
                        reson = "Unkown";
                        break;
                    case NetworkStreamDisconnectReason.BadProtocolVersion:
                        reson = "Bad Protocol Version";
                        break;
                    case NetworkStreamDisconnectReason.ConnectionClose:
                        reson = "Connection Close";
                        break;
                    case NetworkStreamDisconnectReason.InvalidRpc:
                        reson = "InvalidRpc";
                        break;
                }

                // 추후 서버 클라를 구분지어야함.
                string world;
                if (NetworkUtility.IsClientWorld(World))
                {
                    world = "[Client]";
                   
                }
                else
                {
                    world = "[Server]";
                }

                Debug.Log(String.Format("{0} Disconnect network id : {1} reson : {2}",
                                        world, network_id.Value, reson));
            });
    }
}
