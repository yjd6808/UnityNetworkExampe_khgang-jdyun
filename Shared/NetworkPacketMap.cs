// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 3:00:22
// @PURPOSE     : 패킷 맵
// @EMAIL       : wjdeh10110@gmail.com
// ===============================

using System;
using System.Collections.Generic;

public class NetworkPacketMap
{
    private Dictionary<Type, Action<INetworkPacket>> _PacketMap;        //패킷 맵

    public NetworkPacketMap()
    {
        _PacketMap = new Dictionary<Type, Action<INetworkPacket>>();
    }

    public void Init()
    {
        _PacketMap.Add()
    }

}
