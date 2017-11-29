﻿using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

internal class ConnectedClient
{
    public int ConnectionId;
    public string PlayerName;
}

public class Server : MonoBehaviour
{
    private int _hostID;
    private int _webHostId;
    private int _reliableChannel;
    private int _unreliableChannel;
    private bool _isStarted;
    private byte _error;

    private readonly Dictionary<int, ConnectedClient> _clients = new Dictionary<int, ConnectedClient>();

    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();
        _reliableChannel = cc.AddChannel(QosType.Reliable);
        _unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, Consts.MAX_CONNECTION);

        _hostID = NetworkTransport.AddHost(topo, Consts.PORT, null);
        _webHostId = NetworkTransport.AddWebsocketHost(topo, Consts.PORT, null);

        _isStarted = true;

        Debug.Log("Server started");
    }

    void Update()
    {
        if (!_isStarted)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:
                OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:
                OnData(connectionId, Encoding.Unicode.GetString(recBuffer, 0, dataSize));
                break;
            case NetworkEventType.DisconnectEvent:
                OnDisconnection(connectionId);
                break;
        }
    }

    private void Send(string message, int channelId, int connectionId)
    {
        Debug.Log("Sending to player " + connectionId + ": " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(_hostID, connectionId, channelId, msg, msg.Length, out _error);
    }

    private void OnData(int connectionId, string data)
    {
        Debug.Log("Player " + connectionId + " has sent: " + data);

        var msg = data.Split('|');
        switch (msg[0])
        {
            case CommandAliases.AnswerName:
                var playerName = msg[1];
                _clients[connectionId].PlayerName = playerName;
                break;
            default:
                break;
        }
    }

    private void OnConnection(int connectionId)
    {
        Debug.Log("Player " + connectionId + " has connected");
        var connectedClient = new ConnectedClient() { ConnectionId = connectionId };
        _clients[connectionId] = connectedClient;

        Send(CommandAliases.AskName, _reliableChannel, connectionId);
    }

    private void OnDisconnection(int connectionId)
    {
        Debug.Log("Player " + connectionId + " has disconnected");
        _clients.Remove(connectionId);
    }
}
