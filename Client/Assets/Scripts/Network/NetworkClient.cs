// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 1:03:46
// @PURPOSE     : 클라이언트
// ===============================


using NetworkShared;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkClient
{
    public long ID { get; private set; }

    private TcpClient _TcpClient;                          //서버와 연결된 소켓 클라이언트

    private const ushort MaxReceiveByteCount = 4092;                //최대 수신 가능한 바이트
    private const ushort MaxSendByteCount = 4092;                   //최대 송신 가능한 바이트

    private byte[] _ReciveBytes;                           //수신 버퍼
    private byte[] _SendBytes;                             //송신 버퍼

    private Thread _ReceiveThread;                         //수신만 담당하는 쓰레드
    private Thread _SendThread;                            //송신만 담당하는 쓰레드

    private readonly NetworkPacketStreamer _SendPacketStreamer;     //송신 패킷을 간접적으로 처리하는 클래스
    private readonly NetworkPacketStreamer _RecivePacketStreamer;   //수신 패킷을 간접적으로 처리하는 클래스

    private NetworkStream _Stream;

    public NetworkClient()
    {
        _SendPacketStreamer = new NetworkPacketStreamer(SendPacket);
        _RecivePacketStreamer = new NetworkPacketStreamer(ReceivePacket);
        _ReciveBytes = new byte[MaxReceiveByteCount];
        _SendBytes = new byte[MaxSendByteCount];
    }

    public static NetworkClient Get() => Network.Get().Client;
    public void BeginConnect(IPAddress ipAddress, ushort port)
    {
        //이미 연결된 경우는 바로 반환
        if (_TcpClient != null && _TcpClient.Connected)
        {
            Debug.LogError("이미 서버와 연결되어 있습니다.");
            return;
        }

        try
        {
            ID = DateTime.Now.Ticks;

            _TcpClient = new TcpClient();
            _TcpClient.BeginConnect(ipAddress, (int)port, new AsyncCallback(ConnectCallback), _TcpClient);
            Debug.Log("서버에 접속을 시도합니다");
        }
        catch (Exception e)
        {
            Debug.LogError("서버 접속 실패\n" + e.Message);
        }
    }

    private void ConnectCallback(IAsyncResult result)
    {
        if (_TcpClient.Connected)
        {
            _Stream = _TcpClient.GetStream();
            _ReceiveThread = new Thread(ReceiveThreadFunction) { IsBackground = true };
            _SendThread = new Thread(SendThreadFunction) { IsBackground = true };
            _ReceiveThread.Start();
            _SendThread.Start();
            Send(new PtkClientConnect(ID));
            Debug.Log("서버 접속에 성공하였습니다.");
        }
        else
        {
            Debug.Log("서버 접속에 실패하였습니다.");
        }
    }

    public void Disconnect()
    {
        try
        {
            //연결중이라면 해제한다.
            if (_TcpClient.Connected)
            {
                _TcpClient.Client.Disconnect(true);
                Debug.Log("서버 접속 해제 성공!");
            }
        }
        catch( Exception e)
        {
            Debug.LogError("접속 해제 실패\n" + e.Message);
        }
    }

    public void Send(INetworkPacket packet)
    {
        _SendPacketStreamer.RegisterPacket(packet);
    }

    private void ReceiveThreadFunction()
    {
        while (_TcpClient.Connected)
        {
            int readBytesCount = 0;
            try
            {
                readBytesCount = _Stream.Read(_ReciveBytes, 0, MaxReceiveByteCount);
            }
            catch 
            {
                Debug.LogError("서버와 연결이 강제적으로 끊어졌당께!");
                break;
            }

            //수신한 바이트가 있을 경우
            if (readBytesCount > 0)
            {
                _RecivePacketStreamer.RegisterPacket(_ReciveBytes.ToNetworkPacket());
                _RecivePacketStreamer.FlushPacket();
            }
            else
            {
                break;
            }
        }

        Disconnect();
    }

    private void SendThreadFunction()
    {
        while (_TcpClient.Connected)
            _SendPacketStreamer.FlushPacket();
    }

    private void SendPacket(INetworkPacket packet)
    {
        //연결되어 있지 않다면 안보냄
        if (!_TcpClient.Connected)
            return;

        _SendBytes = packet.ToByteArray();
        _Stream.Write(_SendBytes, 0, _SendBytes.Length);
        Debug.Log(packet.GetType() + " 패킷 전송완료");
    }

    private void ReceivePacket(INetworkPacket packet)
    {
        UnityPacketDispatcher.Get().Enqueue(packet);
    }
}
