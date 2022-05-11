using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern void sendClickSignal(string str);

    public void uSendMsg()
    {
        string input = GameObject.Find("Canvas/InputField/Text").GetComponent<Text>().text;
        sendClickSignal(input);
    }

    public void uReceiveMsg(string content)
    {
        GameObject showObject = GameObject.Find("Canvas/Image/Text");
        showObject.GetComponent<Text>().text = content;
    }
}
