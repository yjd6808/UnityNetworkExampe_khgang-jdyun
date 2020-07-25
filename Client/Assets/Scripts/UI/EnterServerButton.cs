using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterServerButton : MonoBehaviour
{
    public void OnClick()
    {
        Network.Get().Start();
    }
}
