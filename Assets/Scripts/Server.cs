using System.Linq;
using System.Collections.Generic;
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
    #region Connection properties
    private int _hostID;
    private int _reliableChannel;
    private int _unreliableChannel;
    private byte _error;
    #endregion

    private bool _isStarted;

    private readonly Dictionary<int, ConnectedClient> _clients = new Dictionary<int, ConnectedClient>();

    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();
        _reliableChannel = cc.AddChannel(QosType.ReliableSequenced);
        _unreliableChannel = cc.AddChannel(QosType.Unreliable);
        HostTopology topo = new HostTopology(cc, Consts.MAX_CONNECTION);
        _hostID = NetworkTransport.AddHost(topo, Consts.PORT, null);
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

    private void Broadcast(string message, int channelId, int? exceptConnectionId = null)
    {
        Debug.Log("Broadcasting: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        var clients = exceptConnectionId.HasValue
            ? _clients.Where(w => w.Key != exceptConnectionId.Value)
            : _clients;
        foreach (var client in clients)
        {
            NetworkTransport.Send(_hostID, client.Value.ConnectionId, channelId, msg, msg.Length, out _error);
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
            case CommandAliases.AnswerName: // client answering name: ANSN|<NAME>
                var playerName = msg[1];
                _clients[connectionId].PlayerName = playerName;
                Broadcast($"{CommandAliases.PlayerConnected}|{connectionId}={playerName}", _reliableChannel, connectionId);
                var players = _clients.Where(w => !string.IsNullOrEmpty(w.Value.PlayerName)).Select(s => $"{s.Value.ConnectionId}={s.Value.PlayerName}");
                Send($"{CommandAliases.Players}|{string.Join("|", players)}", _reliableChannel, connectionId);
                break;
            case CommandAliases.MyPosition:
                Broadcast($"{CommandAliases.PlayerPosition}|{connectionId}|{msg[1]}|{msg[2]}|{msg[3]}", _reliableChannel, connectionId);
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
        Send(CommandAliases.AskName + "|" + connectionId, _reliableChannel, connectionId);
    }

    private void OnDisconnection(int connectionId)
    {
        Debug.Log("Player " + connectionId + " has disconnected");
        _clients.Remove(connectionId);
        Broadcast($"{CommandAliases.PlayerDisconnected}|{connectionId}", _reliableChannel);
    }
}
