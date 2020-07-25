// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 12:55:18
// @PURPOSE     : 쓰레드세이프 패킷 큐를 통한 네트워크 스트리밍 처리
// @EMAIL       : wjdeh10110@gmail.com
// ===============================


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using NetworkShared;

public class NetworkPacketStreamer
{
    private readonly ConcurrentQueue<INetworkPacket> _PackerProcesser;       //쓰레드에 안전한 패킷 큐
    private readonly Action<INetworkPacket> _PacketProccessAction;           //해당 패킷에 대한 처리 함수

    public NetworkPacketStreamer(Action<INetworkPacket> processAction)
    {
        _PackerProcesser = new ConcurrentQueue<INetworkPacket>();
        _PacketProccessAction = processAction;
    }

    public void RegisterPacket(INetworkPacket packet)
    {
        _PackerProcesser.Enqueue(packet);
    }

    public void FlushPacket()
    {
        while (_PackerProcesser.IsEmpty == false)
        {
            if (_PackerProcesser.TryDequeue(out INetworkPacket networkPacket))
                _PacketProccessAction(networkPacket);
        }
    }
}
