using System.Net;
using UnityEngine;

/// <summary>
/// 심플 네트워크 싱글톤 클래스
/// 매니저라고 생각하면됨
/// </summary>
public class Network
{
    //static 변수로 선언한 이유
    //1. 씬 전환등으로 객체의 소멸을 막기위해 
    //2. 가비지 컬렉터의 임의로 객체를 삭제하는 것을 막기위해
    //3. 언제 어디서든 편한 호출을 위해
    private static Network NetworkInstance;
    
    private bool _Running;
    public NetworkClient Client { get; private set; }

    public static Network Get()
    {
        if (NetworkInstance == null)
        {
            NetworkInstance = new Network();
            NetworkInstance.Init();
        }

        return NetworkInstance;
    }

    private void Init()
    {
        Client = new NetworkClient();
    }

    public void Start()
    {
        _Running = true;
        Client.BeginConnect(IPAddress.Parse( NetworkConstant.ServerIP), NetworkConstant.ServerPort);
    }

    public void Stop()
    {
        Client.Disconnect();
        _Running = false;
    }
}
