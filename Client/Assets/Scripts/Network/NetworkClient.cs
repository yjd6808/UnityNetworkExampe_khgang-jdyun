// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 1:03:46
// @PURPOSE     : 클라이언트
// ===============================


using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NetworkClient
{
    private readonly TcpClient _TcpClient;                          //서버와 연결된 소켓 클라이언트
    private readonly long _ID;                                      //클라이언트 고유 식별 번호

    private const ushort MaxReceiveByteCount = 4092;                //최대 수신 가능한 바이트
    private const ushort MaxSendByteCount = 4092;                   //최대 송신 가능한 바이트

    private readonly byte[] _ReciveBytes;                           //수신 버퍼
    private readonly byte[] _SendBytes;                             //송신 버퍼

    private readonly Thread _ReceiveThread;                         //수신만 담당하는 쓰레드
    private readonly Thread _SendThread;                            //송신만 담당하는 쓰레드

    private readonly NetworkPacketStreamer _SendPacketStreamer;     //송신 패킷을 처리하는 클래스
    private readonly NetworkPacketStreamer _RecivePacketStreamer;   //수신 패킷을 처리하는 클래스

    private NetworkStream _Stream;

    public NetworkClient()
    {
        _ReceiveThread = new Thread(ReceiveThreadFunction);
        _SendThread = new Thread(SendThreadFunction);
        _SendPacketStreamer = new NetworkPacketStreamer();
        _RecivePacketStreamer = new NetworkPacketStreamer();
        _ReciveBytes = new byte[MaxReceiveByteCount];
        _SendBytes = new byte[MaxSendByteCount];
    }


    public void Connect(IPAddress ipAddress, ushort port)
    {
        try
        {
            Debug.Assert(_TcpClient.Connected, "연결되있는데요?");
            _TcpClient.Connect(ipAddress, (int)port);
            _Stream = _TcpClient.GetStream();
            _ReceiveThread.Start();
            _SendThread.Start();
        }
        catch (Exception e)
        {
            Debug.WriteLine("서버 접속 실패\n" + e.Message);
        }
    }

    public void Disconnect()
    {
        if (_TcpClient.Connected)
            _TcpClient.Close();
        _ReceiveThread.Join();
        _SendThread.Join();
    }

    public void Send(INetworkPacket packet)
    {
        _SendPacketStreamer.RegisterPacket(packet);
    }

    [NetworkThreadAttribute(NetworkThreadType.Receive)]
    private void ReceiveThreadFunction()
    {
        while (_TcpClient.Connected)
        {
            int readByte = _Stream.Read(_ReciveBytes, 0, MaxReceiveByteCount);

            //수신한 바이트가 있을 경우
            if (readByte > 0)
                _RecivePacketStreamer.RegisterPacket(_ReceiveThre);
        }
    }

    [NetworkThreadAttribute(NetworkThreadType.Send)]
    private void SendThreadFunction()
    {

    }
}
