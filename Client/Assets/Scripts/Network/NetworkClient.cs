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

using UnityEngine;

public class NetworkClient
{
    private TcpClient _TcpClient;                          //서버와 연결된 소켓 클라이언트
    private long _ID;                                      //클라이언트 고유 식별 번호

    private const ushort MaxReceiveByteCount = 4092;                //최대 수신 가능한 바이트
    private const ushort MaxSendByteCount = 4092;                   //최대 송신 가능한 바이트

    private byte[] _ReciveBytes;                           //수신 버퍼
    private byte[] _SendBytes;                             //송신 버퍼

    private Thread _ReceiveThread;                         //수신만 담당하는 쓰레드
    private Thread _SendThread;                            //송신만 담당하는 쓰레드

    private readonly NetworkPacketStreamer _SendPacketStreamer;     //송신 패킷을 간접적으로 처리하는 클래스
    private readonly NetworkPacketStreamer _RecivePacketStreamer;   //수신 패킷을 간접적으로 처리하는 클래스

    private bool Connected;
    private NetworkStream _Stream;

    public NetworkClient()
    {
        _SendPacketStreamer = new NetworkPacketStreamer(SendPacket);
        _RecivePacketStreamer = new NetworkPacketStreamer(ReceivePacket);
        _ReciveBytes = new byte[MaxReceiveByteCount];
        _SendBytes = new byte[MaxSendByteCount];
    }

    public static NetworkClient Get() => Network.Get().Client;
    public long GetID() => _ID;

    public void Connect(IPAddress ipAddress, ushort port)
    {
        try
        {
            Connected = true;

            _ID = DateTime.Now.Ticks;
            _TcpClient = new TcpClient();
            _TcpClient.Connect(ipAddress, (int)port);
            _Stream = _TcpClient.GetStream();
            _ReceiveThread = new Thread(ReceiveThreadFunction) { IsBackground = true };
            _SendThread = new Thread(SendThreadFunction) { IsBackground = true };
            _ReceiveThread.Start();
            _SendThread.Start();
            Send(new PtkClientConnect(_ID));
            Debug.Log("서버 접속 성공!");
        }
        catch (Exception e)
        {
            Debug.LogError("서버 접속 실패\n" + e.Message);
        }
    }

    public void Disconnect()
    {
        try
        {
            Connected = false;
            if (_TcpClient.Connected)
                _TcpClient.Client.Disconnect(true);
            Debug.Log("서버 접속 해제 성공!");
        }
        catch( Exception e)
        {
            Debug.LogError("접속 해제 실패\n" + e.Message);
        }
    }

    public void Send(INetworkPacket packet)
    {
        _SendPacketStreamer.RegisterPacket(packet);
        Debug.Log(packet.GetType() + " 패킷 전송완료");
    }

    private void ReceiveThreadFunction()
    {
        while (Connected)
        {
            int readBytesCount = 0;
            try
            {
                readBytesCount = _Stream.Read(_ReciveBytes, 0, MaxReceiveByteCount);
            }
            catch 
            {
                //갑자기 서버 닫힌 경우
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
        while (Connected)
            _SendPacketStreamer.FlushPacket();
    }

    private void SendPacket(INetworkPacket packet)
    {
        _SendBytes = packet.ToByteArray();
        _Stream.Write(_SendBytes, 0, _SendBytes.Length);
    }

    private void ReceivePacket(INetworkPacket packet)
    {
        UnityPacketDispatcher.Get().Enqueue(packet);
    }
}
