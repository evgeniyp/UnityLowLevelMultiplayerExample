using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsMe;
    public bool IsOwnedByServer;
    public float Speed = 500f;

    private float _lastTickTime;

    private Vector3 _prevPosition;
    private int _prevTick;

    private Vector3 _nextPosition;
    private int _nextTick;

    void Start()
    {
        _prevPosition = transform.position;
        _nextPosition = transform.position;
    }

    void Update()
    {
        if (IsOwnedByServer)
            return;

        if (!IsMe)
        {
            var lastTimeInterval = (_nextTick - _prevTick) * Time.fixedDeltaTime;
            if (lastTimeInterval == 0) return;
            var time = Time.time;
            var timeSinceLastTick = time - _lastTickTime;

            transform.position = Vector3.LerpUnclamped(_prevPosition, _nextPosition, 1 + (timeSinceLastTick / lastTimeInterval));

            Debug.Log($"p:{_prevTick} pPos:{_prevPosition.x} n:{_nextTick} nPos:{_nextPosition.x} time:{time} timeSinceNext:{timeSinceLastTick} lerpFactor:{1 + (timeSinceLastTick / lastTimeInterval)}");
        }
        else
        {
            var movementVector = new Vector3()
            {
                x = Input.GetAxisRaw("Horizontal") * Time.deltaTime * Speed,
                y = Input.GetAxisRaw("Vertical") * Time.deltaTime * Speed
            };
            transform.position += movementVector;
        }
    }

    public void SetPosition(int tick, Vector3 position)
    {
        if (tick <= _nextTick)
        {
            Debug.LogWarning($"tick {tick} less than last received tick {_nextTick}");
            return;
        }
        if (tick - _nextTick > 1)
        {
            Debug.LogWarning($"tick {tick} is greater than last received tick {_nextTick} by 2 or more");
        }

        Debug.Log($"Received tick:{tick} position:{position.x} time:{Time.time}");

        _prevPosition = _nextPosition;
        _prevTick = _nextTick;

        _nextPosition = position;
        _nextTick = tick;

        _lastTickTime = Time.time;
    }
}
