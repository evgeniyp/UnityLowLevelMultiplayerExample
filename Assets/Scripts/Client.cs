using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

internal class NetworkPlayer
{
    public int PlayerId;
    public string PlayerName;
    public Player Instance;
}

public class Client : MonoBehaviour
{
    public GameObject PlayerPrefab;

    #region Connection properties
    private int _connectionId;
    private int _hostID;
    private int _reliableChannel;
    private int _unreliableChannel;
    private byte _error;
    #endregion

    private bool _isConnected;
    private bool _isStarted;

    private int _tick;

    private readonly Dictionary<int, NetworkPlayer> _players = new Dictionary<int, NetworkPlayer>();

    private string _playerName;
    private int _playerId;

    private GameObject _canvas;

    private void Start()
    {
        _canvas = GameObject.Find("Canvas");
    }

    public void Connect()
    {
        if (_isConnected)
            return;

        string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (string.IsNullOrEmpty(pName))
        {
            Debug.Log("You must enter a name!");
            return;
        }

        _playerName = pName;

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();
        _reliableChannel = cc.AddChannel(QosType.ReliableSequenced);
        _unreliableChannel = cc.AddChannel(QosType.Unreliable);
        HostTopology topo = new HostTopology(cc, Consts.MAX_CONNECTION);
        _hostID = NetworkTransport.AddHost(topo, 0);
        _connectionId = NetworkTransport.Connect(_hostID, "127.0.0.1", Consts.PORT, 0, out _error);

        if ((NetworkError)_error == NetworkError.Ok)
        {
            _isConnected = true;
            Debug.Log("Connected");
        }
    }

    void Update()
    {
        if (!_isConnected)
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
                case NetworkEventType.DataEvent:
                    OnData(Encoding.Unicode.GetString(recBuffer, 0, dataSize));
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("Disconnected");
                    _isConnected = false;
                    DestroyAllPlayers();
                    _canvas.SetActive(true);
                    _isStarted = false;
                    break;
            }
        }
    }

    public void FixedUpdate()
    {
        if (_isStarted == false)
            return;

        //var me = _players[_playerId].Instance;
        //var position = me.transform.position;
        //Send($"{CommandAliases.MyPosition}|{_tick}|{position.x}|{position.y}|{position.z}", _unreliableChannel);
    }

    private void Send(string message, int channelId)
    {
        //Debug.Log("Sending to server: " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(_hostID, _connectionId, channelId, msg, msg.Length, out _error);
    }

    private void OnData(string data)
    {
        //Debug.Log("Server has sent: " + data);

        var msg = data.Split('|');
        switch (msg[0])
        {
            case CommandAliases.AskName: // server asking name and sending an ID: ASKN|<ID>
                _playerId = int.Parse(msg[1]);
                GameObject.Find("PlayerId").GetComponent<Text>().text = msg[1];
                Send(CommandAliases.AnswerName + '|' + _playerName, _reliableChannel);
                break;
            case CommandAliases.Players: // server sends exact list of players: PLRS|<ID>=<NAME>|<ID>=<NAME|...
                for (int i = 1; i < msg.Length; i++)
                {
                    var idNameArr = msg[i].Split('=');
                    SpawnPlayer(int.Parse(idNameArr[0]), idNameArr[1]);
                }
                break;
            case CommandAliases.PlayerConnected: // PLRCON|<ID>=<NAME>
                {
                    var details = msg[1].Split('=');
                    SpawnPlayer(int.Parse(details[0]), details[1]);
                    break;
                }
            case CommandAliases.PlayersPosition: // server sends position of players PLRSPOS|<TICK>|<ID>=<X>;<Y>;<Z>|<ID>=<X>;<Y>;<Z>|...
                _tick = int.Parse(msg[1]);
                for (int i = 2; i < msg.Length; i++)
                {
                    var idNameArr = msg[i].Split('=');
                    var playerId = int.Parse(idNameArr[0]);
                    if (playerId == _playerId)
                        continue;
                    var positionArr = idNameArr[1].Split(';');
                    var position = new Vector3(float.Parse(positionArr[0]), float.Parse(positionArr[1]), float.Parse(positionArr[2]));
                    if (_players.ContainsKey(playerId))
                        _players[playerId].Instance.Position = position;
                }
                break;
            case CommandAliases.PlayerDisconnected: // PLRDIS|<ID>
                DestroyPlayer(int.Parse(msg[1]));
                break;
            default:
                break;
        }
    }

    private void PlayerFixedUpdate(Vector3 v)
    {
        Send($"{CommandAliases.MyInput}|{_tick}|{v.x}|{v.y}|{v.z}", _unreliableChannel);
    }

    private void SpawnPlayer(int playerId, string playerName)
    {
        if (!_players.ContainsKey(playerId))
        {
            var instance = Instantiate(PlayerPrefab).GetComponent<Player>();

            var player = new NetworkPlayer() { Instance = instance, PlayerId = playerId, PlayerName = playerName };
            _players.Add(playerId, player);

            player.Instance.GetComponentInChildren<TextMesh>().text = playerName;

            if (playerId == _playerId)
            {
                player.Instance.Type = PlayerObjectType.ThisPlayer;
                player.Instance.OnFixedUpdate += PlayerFixedUpdate;
                _canvas.SetActive(false);
                _isStarted = true;
            }
        }
    }

    private void DestroyPlayer(int playerId)
    {
        if (_players.ContainsKey(playerId))
        {
            var instance = _players[playerId].Instance;

            if (instance.Type == PlayerObjectType.ThisPlayer)
                instance.OnFixedUpdate -= PlayerFixedUpdate;

            Destroy(instance.gameObject);
            _players.Remove(playerId);
        }
    }

    private void DestroyAllPlayers()
    {
        foreach (var player in _players)
            Destroy(player.Value.Instance.gameObject);
        _players.Clear();
    }

    private void OnApplicationQuit()
    {
        NetworkTransport.Disconnect(_hostID, _connectionId, out _error);
    }
}
