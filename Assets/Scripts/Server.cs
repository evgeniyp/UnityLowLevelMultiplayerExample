using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

internal class ConnectedClient
{
    public int ConnectionId;
    public string PlayerName;
    public int Tick;
    public Player Instance;
}

public class Server : MonoBehaviour
{
    public GameObject PlayerPrefab;

    #region Connection properties
    private int _hostID;
    private int _reliableChannel;
    private int _unreliableChannel;
    private byte _error;
    #endregion

    private bool _isStarted;
    private int _tick;

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

    private void Update()
    {
        if (!_isStarted)
            return;

        while (true)
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);
            if (_error != 0)
                Debug.Log($"NetworkError: {(NetworkError)_error}");
            switch (recData)
            {
                case NetworkEventType.Nothing:
                    return;
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
    }

    private void FixedUpdate()
    {
        _tick++;

        var positionsArr = _clients.Values.Select(s => $"{s.ConnectionId}={s.Instance.Position.x};{s.Instance.Position.y};{s.Instance.Position.z}");
        var positionsStr = string.Join("|", positionsArr);
        Broadcast($"{CommandAliases.PlayersPosition}|{_tick}|{positionsStr}", _unreliableChannel);
    }

    private void Broadcast(string message, int channelId, int? exceptConnectionId = null)
    {
        //Debug.Log("Broadcasting: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        var clients = exceptConnectionId.HasValue
            ? _clients.Where(w => w.Key != exceptConnectionId.Value)
            : _clients;
        foreach (var client in clients)
        {
            NetworkTransport.Send(_hostID, client.Key, channelId, msg, msg.Length, out _error);
        }
    }

    private void Send(string message, int channelId, int connectionId)
    {
        //Debug.Log("Sending to player " + connectionId + ": " + message);
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
                _clients[connectionId].Instance.Name = playerName;

                Broadcast($"{CommandAliases.PlayerConnected}|{connectionId}={playerName}", _reliableChannel, connectionId);
                var players = _clients.Where(w => !string.IsNullOrEmpty(w.Value.PlayerName)).Select(s => $"{s.Value.ConnectionId}={s.Value.PlayerName}");
                Send($"{CommandAliases.Players}|{string.Join("|", players)}", _reliableChannel, connectionId);
                break;
            case CommandAliases.MyPosition:
                //var tick = int.Parse(msg[1]);
                //var client = _clients[connectionId];

                //if (tick < client.Tick && client.Tick != 0)
                //{
                //    Debug.LogWarning($"Incoming tick {tick} is from the past, and not initial tick. Expecting {client.Tick + 1}. Skipping.");
                //    break;
                //}

                //client.Tick = tick;
                //var position = new Vector3(float.Parse(msg[2]), float.Parse(msg[3]), float.Parse(msg[4]));
                //client.Position = position;
                //client.Instance.transform.position = position;
                break;
            case CommandAliases.MyInput:
                var tick = int.Parse(msg[1]);
                var client = _clients[connectionId];

                if (tick < client.Tick && client.Tick != 0)
                {
                    Debug.LogWarning($"Incoming tick {tick} is from the past, and not initial tick. Expecting {client.Tick + 1}. Skipping.");
                    break;
                }

                client.Tick = tick;
                var input = new Vector3(float.Parse(msg[2]), float.Parse(msg[3]), float.Parse(msg[4]));
                client.Instance.UserInput = input;
                break;
            default:
                break;
        }
    }

    private void OnConnection(int connectionId)
    {
        Debug.Log("Player " + connectionId + " has connected");
        var instance = Instantiate(PlayerPrefab).GetComponent<Player>();
        instance.GetComponentInChildren<TextMesh>().text = "";
        var connectedClient = new ConnectedClient() { ConnectionId = connectionId, Instance = instance };
        _clients[connectionId] = connectedClient;
        Send(CommandAliases.AskName + "|" + connectionId, _reliableChannel, connectionId);
    }

    private void OnDisconnection(int connectionId)
    {
        Debug.Log("Player " + connectionId + " has disconnected");
        var instance = _clients[connectionId].Instance;
        _clients[connectionId].Instance = null;
        _clients.Remove(connectionId);
        Destroy(instance.gameObject);
        Broadcast($"{CommandAliases.PlayerDisconnected}|{connectionId}", _reliableChannel, connectionId);
    }

    private void OnApplicationQuit()
    {
        foreach (var client in _clients.Values)
        {
            NetworkTransport.Disconnect(_hostID, client.ConnectionId, out _error);
        }
    }
}
