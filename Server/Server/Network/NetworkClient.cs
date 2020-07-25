// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 5:47:38
// @PURPOSE     : 서버에 접속한 클라이언트 객체
// @EMAIL       : wjdeh10110@gmail.com
// ===============================

using NetworkShared;
using Server;
using System;
using System.Net.Sockets;
using System.Threading;

public class NetworkClient
{
    public long ID { get; set; }
    public TcpClient TcpClient { get; set; }
    public bool Connected { get; private set; }

    private Thread _PacketReceiveThread;
    private NetworkStream _Stream;

    private const ushort MaxReceiveByteCount = 4092;                        //최대 수신 가능한 바이트
    private const ushort MaxSendByteCount = 4092;                           //최대 송신 가능한 바이트

    private byte[] _ReciveBytes;                                            //수신 버퍼
    private byte[] _SendBytes;                                              //송신 버퍼

    public NetworkClient(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        Connected = true;

        _Stream = TcpClient.GetStream();
        _PacketReceiveThread = new Thread(OnReceivePacket);
        _PacketReceiveThread.IsBackground = true;
        _PacketReceiveThread.Start();
        _ReciveBytes = new byte[MaxReceiveByteCount];
        _SendBytes = new byte[MaxSendByteCount];
        
    }

    public void Disconnect()
    {
        Connected = false;
        if (TcpClient.Connected)
            TcpClient.Close();
        Console.WriteLine(ID + " 유저가 접속을 종료했습니다");
    }
    
    public void Send(INetworkPacket networkPacket)
    {
        _SendBytes = networkPacket.ToByteArray();
        _Stream.Write(_SendBytes, 0, _SendBytes.Length);
    }

    private void OnReceivePacket()
    {
        while (Connected)
        {
            int readBytesCount = 0;
            try
            {
                readBytesCount = _Stream.Read(_ReciveBytes, 0, _ReciveBytes.Length);
            }
            catch
            {
                NetworkServer.Get().Disconnect(this);
            }

            if (readBytesCount > 0)
            {
                INetworkPacket networkPacket = _ReciveBytes.ToNetworkPacket();
                NetworkServer.Get().OnReceivePacket(this, networkPacket);
            }
            else
            {
                break;
            }
        }
        NetworkServer.Get().Disconnect(this);
    }
}
