using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Logic.Constant;
using static Logic.Constant.CONSTANT;
using System;
using Google.Protobuf;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Commu : MonoBehaviour
{
    private static int port = 8889;
    static Thread operationThread;
    public static DateTime lastSendTime;
    public Communication.CAPI.API ClientCommunication;
    public Transform player;
    float x, y;
    // Start is called before the first frame update
    void Start()
    {
        lastSendTime = new DateTime();
        ClientCommunication = new Communication.CAPI.API();
        ClientCommunication.Initialize();
        ClientCommunication.ReceiveMessage += OnReceive;
        ClientCommunication.ConnectServer(new IPEndPoint(IPAddress.Loopback, port));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
            SendMove(Direction.Right);
        else if (Input.GetKey(KeyCode.W))
            SendMove(Direction.Up);
        else if (Input.GetKey(KeyCode.A))
            SendMove(Direction.Left);
        else if (Input.GetKey(KeyCode.S))
            SendMove(Direction.Down);
    }
    void LateUpdate()
    {
        player.position = new Vector3(x, 0.5f, y);
    }
    void SendMove(Direction direction)
    {
        UnityEngine.Debug.Log("send:" + direction);
        ClientCommunication.SendMessage(
                new MessageToServer
                {
                    CommandType = (int)COMMAND_TYPE.MOVE,
                    Parameter1 = (int)direction,
                    Parameter2 = 0
                }
            );
        lastSendTime = DateTime.Now;
    }
    public void OnReceive(IMessage message)
    {
        UnityEngine.Debug.Log("received:" + message.ToString());
        if (!(message is MessageToClient)) throw new Exception("Recieve Error !");
        MessageToClient msg = message as MessageToClient;
        //this.id.Item1 = msg.PlayerIDAgent;
        //this.id.Item2 = msg.PlayerIDClient;
        x = (float)BitConverter.Int64BitsToDouble(msg.PlayerPositionX);
        y = (float)BitConverter.Int64BitsToDouble(msg.PlayerPositionY);
        
        UnityEngine.Debug.Log("Player " + msg.PlayerIDAgent.ToString() + " , " + msg.PlayerIDClient.ToString() + "  position: " + $"{x},{y}");
    }
    void OnDestroy()
    {
        typeof(Communication.Proto.Constants).Assembly.GetType("Communication.Proto.IDClient").GetMethod("Disconnect").Invoke(
            typeof(Communication.CAPI.API).GetField("client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ClientCommunication), new object[] { }
            );
        UnityEngine.Debug.Log("exit");
    }
}
