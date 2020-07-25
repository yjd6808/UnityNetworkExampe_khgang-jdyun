// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 3:08:44
// @PURPOSE     : 패킷모음
// @EMAIL       : wjdeh10110@gmail.com
// ===============================

using System;

[Serializable]
public class PtkChatMessage : INetworkPacket
{
    public long ID { get; set; }
    public string Message { get; set; }

    public PtkChatMessage(long id, string msg)
    {
        this.ID = id;
        this.Message = msg;
    }
}


[Serializable]
public class PtkChatMessageAck : INetworkPacket
{
    public long ID { get; set; }
    public string Message { get; set; }

    public PtkChatMessageAck(long id, string msg)
    {
        this.ID = id;
        this.Message = msg;
    }

}
