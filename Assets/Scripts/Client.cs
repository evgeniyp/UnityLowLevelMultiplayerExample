using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    private int _connectionID;
    private int _hostID;
    private int _webHostID;
    private int _reliableChannel;
    private int _unreliableChannel;
    private bool _isConnected;
    private bool _isStarted;
    private float _connectionTime;
    private byte _error;

    private string _playerName;

    public void Connect()
    {
        string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (string.IsNullOrEmpty(pName))
        {
            Debug.Log("You must enter a name!");
            return;
        }

        _playerName = pName;

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        _reliableChannel = cc.AddChannel(QosType.Reliable);
        _unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, Consts.MAX_CONNECTION);

        _hostID = NetworkTransport.AddHost(topo, Consts.PORT, null);
        _connectionID = NetworkTransport.Connect(_hostID, "127.0.0.1", Consts.PORT, 0, out _error);

        _connectionTime = Time.time;
        _isConnected = true;
    }

    void Update()
    {
        if (!_isConnected)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.Nothing:         //1
                break;
            case NetworkEventType.ConnectEvent:    //2
                break;
            case NetworkEventType.DataEvent:       //3
                break;
            case NetworkEventType.DisconnectEvent: //4
                break;
        }
    }
}
