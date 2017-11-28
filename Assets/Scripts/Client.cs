using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    private int _connectionId;
    private int _hostID;
    private int _webHostId;
    private int _reliableChannel;
    private int _unreliableChannel;
    private bool _isConnected;
    private float _connectionTime;
    private byte _error;

    private string _playerName;

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
        _reliableChannel = cc.AddChannel(QosType.Reliable);
        _unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, Consts.MAX_CONNECTION);

        _hostID = NetworkTransport.AddHost(topo, 0);
        _connectionId = NetworkTransport.Connect(_hostID, "127.0.0.1", Consts.PORT, 0, out _error);

        if ((NetworkError)_error == NetworkError.Ok)
        {
            _connectionTime = Time.time;
            _isConnected = true;
            Debug.Log("Connected");
        }
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
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);
        switch (recData)
        {
            case NetworkEventType.DataEvent:
                break;
            case NetworkEventType.DisconnectEvent:
                _isConnected = false;
                Debug.Log("Disconnected");
                break;
        }
    }
}
