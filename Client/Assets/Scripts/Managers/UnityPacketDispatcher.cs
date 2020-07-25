using NetworkShared;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class UnityPacketDispatcher : MonoBehaviour
{
    private static UnityPacketDispatcher Dispatcher;

    private ConcurrentQueue<INetworkPacket> _PacketQueue;

    void Start()
    {
        Dispatcher = this;

        _PacketQueue = new ConcurrentQueue<INetworkPacket>();
    }

    public static UnityPacketDispatcher Get() => Dispatcher;

    public void Enqueue(INetworkPacket packet)
    {
        _PacketQueue.Enqueue(packet);
    }

    // Update is called once per frame
    void Update()
    {
        FlushQueue();
    }

    void FlushQueue()
    {
        while (_PacketQueue != null && !_PacketQueue.IsEmpty)
        {
            if (_PacketQueue.TryDequeue(out INetworkPacket packet))
                UnityPacketProcessor.Get().Proccess(packet);
        }
    }
}
