using NetworkShared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnityPacketProcessor : MonoBehaviour
{
    private static UnityPacketProcessor PacketProcessor;

    private Dictionary<Type, Action<INetworkPacket>> _PacketMap;        //패킷 맵
    void Start()
    {
        PacketProcessor = this;
        _PacketMap = new Dictionary<Type, Action<INetworkPacket>>();

        //초기화
        _PacketMap.Add(typeof(PtkChatMessageAck), new Action<INetworkPacket>(PtkChatMessageAck));
    }

    public static UnityPacketProcessor Get() => PacketProcessor;

    public void Proccess(INetworkPacket packet)
    {
        if (_PacketMap.TryGetValue(packet.GetType(), out Action<INetworkPacket> func))
            func(packet);
        else
            Debug.LogError(packet.GetType() + " 메시지가 도착함 해당 패킷에 대한 기능이 패킷 맵에 등록되어 있지 않아요!");
    }

    void PtkChatMessageAck(INetworkPacket packet)
    {
        PtkChatMessageAck ptkChatMessageAck = packet as PtkChatMessageAck;
        GameObject.Find("ChatView").GetComponent<InputField>().text += ptkChatMessageAck.NickName + " : " + ptkChatMessageAck.Message + "\n";
    }
}
