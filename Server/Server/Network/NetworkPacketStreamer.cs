// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 6:27:44   
// @PURPOSE     : 네트워크 패킷 스트리머
// ===============================

using NetworkShared;
using System;
using System.Collections.Concurrent;

namespace Server
{
    public struct NetworkPacketPair
    {
        public long ID;
        public INetworkPacket Packet;
    }

    public class NetworkPacketStreamer
    {
        private readonly ConcurrentQueue<NetworkPacketPair> _PackerProcesser;          //쓰레드에 안전한 패킷 큐
        private readonly Action<NetworkPacketPair> _PacketProccessAction;              //해당 패킷에 대한 처리 함수

        public NetworkPacketStreamer(Action<NetworkPacketPair> processAction)
        {
            _PackerProcesser = new ConcurrentQueue<NetworkPacketPair>();
            _PacketProccessAction = processAction;
        }

        public void RegisterPacket(long id, INetworkPacket packet)
        {
            _PackerProcesser.Enqueue(new NetworkPacketPair() {  ID = id, Packet = packet });
        }

        public void FlushPacket()
        {
            while (_PackerProcesser.IsEmpty == false)
            {
                if (_PackerProcesser.TryDequeue(out NetworkPacketPair networkPacket))
                    _PacketProccessAction(networkPacket);
            }
        }
    }
}
