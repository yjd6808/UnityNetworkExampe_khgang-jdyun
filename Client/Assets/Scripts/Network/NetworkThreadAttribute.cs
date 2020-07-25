// ===============================
// @AUTHOR      : 윤정도
// @CREATE DATE : 2020-07-25 오후 2:27:41
// @PURPOSE     : 함수가 쓰레드 전용임을 나타냄
// @EMAIL       : wjdeh10110@gmail.com
// ===============================


using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum NetworkThreadType
{
    Send,
    Receive
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class NetworkThreadAttribute : Attribute
{
    private static Dictionary<NetworkThreadType, NetworkThreadAttribute> UsableThreadMethod = new Dictionary<NetworkThreadType, NetworkThreadAttribute>();

    public NetworkThreadAttribute(NetworkThreadType networkThreadType) 
    {
        if (UsableThreadMethod.ContainsKey(networkThreadType))
            Debug.Assert(true, "이미 해당 타입은 사용되고 있습니다");
        UsableThreadMethod.Add(networkThreadType, this);
    }
}
