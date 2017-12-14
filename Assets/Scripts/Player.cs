using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string Name;
    public PlayerObjectType Type = PlayerObjectType.ServerObject;
    public Vector3 UserInput;
    public event Action<Vector3, int> OnFixedUpdate;
    public Vector3 Position => _position;
    public int Tick;

    private Vector3 _position, _oldPosition;
    private float _lastUpdateTimeFromServer;

    private Dictionary<int, Vector3> _inputHistory = new Dictionary<int, Vector3>();

    private void Start()
    {
        _lastUpdateTimeFromServer = Time.time;
    }

    private void FixedUpdate()
    {
        switch (Type)
        {
            case PlayerObjectType.ServerObject:
                _oldPosition = _position;
                _position += UserInput * Consts.ClientSpeed * Time.fixedDeltaTime;
                transform.position = _position;
                break;
        }
    }

    private void ManualFixedUpdate()
    {
        switch (Type)
        {
            case PlayerObjectType.ThisPlayer:
                Tick++;
                UserInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
                _inputHistory[Tick] = UserInput;
                OnFixedUpdate?.Invoke(UserInput, Tick);

                _oldPosition = _position;
                _position += UserInput * Consts.ClientSpeed * Time.fixedDeltaTime;
                transform.position = _position;
                break;
        }
    }

    /// <summary>
    /// Update real position of other player as it comes from server
    /// </summary>
    public void UpdatePlayerPositionFromServer(Vector3 position, int tick)
    {
        if (Tick == 0)
            Tick = tick + 1; // After sending position server has changed tick + 1, so we're trying to be in sync

        if (Tick > tick + 1)
            Debug.Log($"client ahead by {Tick - tick}");

        // TODO: think about tick syncing or calling FixedUpdate() manually

        _lastUpdateTimeFromServer = Time.time;
        if (Type == PlayerObjectType.OtherPlayer)
        {
            Tick = tick;
            _oldPosition = _position;
            _position = position;
            transform.position = _position;
        }
        else if (Type == PlayerObjectType.ThisPlayer)
        {
            // Restore position, remove old commands from the queue, apply remaining commands
            _position = position;
            _inputHistory = _inputHistory.Where(w => w.Key > tick).ToDictionary(d => d.Key, d => d.Value);
            foreach (var input in _inputHistory.OrderBy(o => o.Key).Select(s => s.Value))
            {
                _oldPosition = _position;
                _position += input * Consts.ClientSpeed * Time.fixedDeltaTime;
            }
            transform.position = _position;
        }

        ManualFixedUpdate();
    }

    private void Update()
    {
        switch (Type)
        {
            case PlayerObjectType.ThisPlayer:
                {
                    var timeFactor = (Time.time - _lastUpdateTimeFromServer) / Time.fixedDeltaTime;
                    transform.position = Vector3.LerpUnclamped(_oldPosition, _position, 1 + timeFactor); // extrapolation
                    break;
                }
            case PlayerObjectType.OtherPlayer: // Interpolation between 2 last states. Brings fixedDeltaTime lag.
                {
                    var timeFactor = (Time.time - _lastUpdateTimeFromServer) / Time.fixedDeltaTime;
                    transform.position = Vector3.LerpUnclamped(_oldPosition, _position, 1 + timeFactor);
                    break;
                }
        }

        GetComponentInChildren<TextMesh>().text = $"name:{Name}\ntick:{Tick}\ninput:{UserInput}\npos:{transform.position}";
    }

}
