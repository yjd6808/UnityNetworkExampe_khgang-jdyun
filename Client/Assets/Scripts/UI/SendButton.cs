using NetworkShared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendButton : MonoBehaviour
{
    public InputField NickNameField;
    public InputField MessageField;

    public void OnClick()
    {
        NetworkClient client = NetworkClient.Get();
        client.Send(new PtkChatMessage(client.ID, NickNameField.text, MessageField.text));
    }
}
