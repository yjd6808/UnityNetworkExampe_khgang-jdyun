// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 7:28:15   
// @PURPOSE     : 서버의 패킷 맵
// ===============================


using System;
using System.Collections.Generic;
using NetworkShared;

namespace Server
{
    public class NetworkPacketMap
    {
        private readonly Dictionary<Type, Action<NetworkClient, INetworkPacket>> _PacketMap;        //패킷 맵

        public NetworkPacketMap()
        {
            _PacketMap = new Dictionary<Type, Action<NetworkClient, INetworkPacket>>();
        }

        public void Init()
        {
            _PacketMap.Add(typeof(PtkClientConnect), new Action<NetworkClient, INetworkPacket>(PtkClientConnect));
            _PacketMap.Add(typeof(PtkChatMessage), new Action<NetworkClient, INetworkPacket>(PtkChatMessage));
            _PacketMap.Add(typeof(PtkClientDisconnect), new Action<NetworkClient, INetworkPacket>(PtkClientDisconnect));
        }

        public void Proccess(NetworkClient client, INetworkPacket packet)
        {
            if (_PacketMap.TryGetValue(packet.GetType(), out Action<NetworkClient, INetworkPacket> func))
                func(client, packet);
        }

        //---------------------------------------------------------------------



        void PtkClientConnect(NetworkClient client, INetworkPacket packet)
        {
            client.ID = packet.ID;
            Console.WriteLine(client.ID + " 유저가 접속했습니다");
        }

        void PtkChatMessage(NetworkClient client, INetworkPacket packet)
        {
            PtkChatMessage ptkChatMessageAck = packet as PtkChatMessage;
            NetworkServer.Get().Broadcast(new PtkChatMessageAck(packet.ID, ptkChatMessageAck.NickName, ptkChatMessageAck.Message));
        }

        void PtkClientDisconnect(NetworkClient client, INetworkPacket packet)
        {
            NetworkServer.Get().Disconnect(client);
            Console.WriteLine(client.ID + " 유저가 접속을 종료했습니다.");
        }
    }

}
