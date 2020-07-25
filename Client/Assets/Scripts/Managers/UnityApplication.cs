using NetworkShared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityApplication : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        NetworkClient.Get().Send(new PtkClientDisconnect(NetworkClient.Get().GetID()));
    }
}
