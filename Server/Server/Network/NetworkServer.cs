// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 5:46:57
// @PURPOSE     : 서버 객체, 접속한 클라이언트 관리 및 패킷 전송
// @EMAIL       : wjdeh10110@gmail.com
// ===============================


using NetworkShared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class NetworkServer
    {
        private static NetworkServer Server;

        private readonly TcpListener _TcpListener;                               //서버 소켓 객체
        private readonly Thread _TcpClientAcceptThread;                          //연결 요청을 수신받는 쓰레드
        private readonly Thread _TcpPacketProcessThread;                         //전송, 수신 대기중인 패킷들을 처리하는 쓰레드

        private readonly LinkedList<NetworkClient> _ConnectWaitingClient;        //연결되었지만 아직 확인되지 않은 클라이언트
        private readonly object _ConnectWaitingClientLocker;

        private readonly NetworkPacketStreamer _TcpReceivePacketStreamer;        //송신 패킷을 간접적으로 처리하는 클래스
        private readonly NetworkPacketStreamer _TcpSendPacketStreamer;           //수신 패킷을 간접적으로 처리하는 클래스
        private readonly NetworkPacketMap _NetworkPacketMap;                     //수신 패킷을 구분하여 처리해주는 클리스

        private const ushort MaxReceiveByteCount = 4092;                         //최대 수신 가능한 바이트
        private const ushort MaxSendByteCount = 4092;                            //최대 송신 가능한 바이트

        private byte[] _ReciveBytes;                                             //수신 버퍼
        private byte[] _SendBytes;                                               //송신 버퍼

        public Dictionary<long, NetworkClient> ConnectedClients;                 //연결되었고 확인된 클라이언트
        public readonly object ConnectedClientsLocker;
        public bool Running;

        public static NetworkServer Get()
        {
            if (Server == null)
                Server = new NetworkServer();
            return Server;
        }

        public NetworkServer()
        {
            _TcpListener = new TcpListener(12345);
            _ConnectWaitingClient = new LinkedList<NetworkClient>();
            _ConnectWaitingClientLocker = new object();

            _TcpClientAcceptThread = new Thread(AcceptTcpClientThreadFunc);
            _TcpClientAcceptThread.IsBackground = true;
            _TcpPacketProcessThread = new Thread(ProcessTcpClientPacketFunc);
            _TcpPacketProcessThread.IsBackground = true;

            _TcpReceivePacketStreamer = new NetworkPacketStreamer(ReceivePacket);
            _TcpSendPacketStreamer = new NetworkPacketStreamer(SendPacket);

            _ReciveBytes = new byte[MaxReceiveByteCount];
            _SendBytes = new byte[MaxSendByteCount];

            _NetworkPacketMap = new NetworkPacketMap();
            _NetworkPacketMap.Init();

            Running = false;
            ConnectedClients = new Dictionary<long, NetworkClient>();
            ConnectedClientsLocker = new object();
        }

        public void Start()
        {
            Running = true;

            _TcpListener.Start();
            _TcpClientAcceptThread.Start();
            _TcpPacketProcessThread.Start();
        }

        public void Stop()
        {
            _TcpListener.Stop();
            Running = true;
            ConnectedClients.Clear();
        }

        /// <summary>
        /// Tcp 클라이언트들 접속받는 쓰레드 함수
        /// </summary>
        private void AcceptTcpClientThreadFunc()
        {
            while (Running)
            {
                TcpClient accptClient = _TcpListener.AcceptTcpClient();
                lock (_ConnectWaitingClientLocker)
                {
                    _ConnectWaitingClient.AddLast(new NetworkClient(accptClient));
                }
            }
        }

        public void OnReceivePacket(NetworkClient client, INetworkPacket packet)
        {
            if (packet.GetType() == typeof(PtkClientConnect))
            {
                lock (_ConnectWaitingClientLocker)
                {
                    if (_ConnectWaitingClient.Contains(client))
                        _ConnectWaitingClient.Remove(client);
                }

                lock (ConnectedClientsLocker)
                {
                    if (ConnectedClients.ContainsKey(packet.ID))
                        ConnectedClients[packet.ID] = client;
                    else
                        ConnectedClients.Add(packet.ID, client);
                }
            }

            _TcpReceivePacketStreamer.RegisterPacket(packet.ID, packet);
        }

        private void ProcessTcpClientPacketFunc()
        {
            while (Running)
            {
                _TcpReceivePacketStreamer.FlushPacket();
                _TcpSendPacketStreamer.FlushPacket();
            }
        }

        private void SendPacket(NetworkPacketPair pair)
        {
            if (ConnectedClients.TryGetValue(pair.ID, out NetworkClient client))
                client.Send(pair.Packet);
        }

        private void ReceivePacket(NetworkPacketPair pair)
        {
            if (ConnectedClients.TryGetValue(pair.ID, out NetworkClient client))
                _NetworkPacketMap.Proccess(client, pair.Packet);
        }


        /* =========================================================================== */

        //특정 클라한테만 전송
        public void SendTo(long id, INetworkPacket packet)
        {
            _TcpSendPacketStreamer.RegisterPacket(id, packet);
        }

        //특정 클라한테만 전송
        public void SendTo(NetworkClient client, INetworkPacket packet)
        {
            _TcpSendPacketStreamer.RegisterPacket(client.ID, packet);
        }

        /// <summary>
        /// 모든 클라한테 전송
        /// </summary>
        public void Broadcast(INetworkPacket packet)
        {
            foreach (NetworkClient client in ConnectedClients.Values)
                SendTo(client, packet);
        }

        /// <summary>
        /// 모든 클라한테 전송
        /// </summary>
        /// <param name="packet">전송하는 패킷</param>
        /// <param name="except">제외할 클라들</param>
        public void BroadcastExcept(INetworkPacket packet, NetworkClient[] except)
        {
            foreach (NetworkClient client in ConnectedClients.Values.Except(except))
                SendTo(client, packet);
        }

        public void BroadcastExcept(INetworkPacket packet, long[] except)
        {
            foreach (NetworkClient client in ConnectedClients.Values.Where(x => !except.Contains(x.ID)))
                SendTo(client, packet);
        }

       
        public void Disconnect(NetworkClient client)
        {
            //접속 대기중인 클라의 경우 리스트에서 제거해줌
            lock (_ConnectWaitingClientLocker)
            {
                if (_ConnectWaitingClient.Contains(client))
                    _ConnectWaitingClient.Remove(client);
            }

            //연결된 클라 리스트에서 제거해줌
            lock (ConnectedClientsLocker)
            {
                if (ConnectedClients.ContainsKey(client.ID))
                {
                    ConnectedClients.Remove(client.ID);
                    client.Disconnect();
                }
            }
        }
    }
}