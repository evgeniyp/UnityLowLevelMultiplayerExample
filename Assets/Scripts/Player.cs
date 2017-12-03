using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsMe;
    public float Speed = 500f;

    private float _firstTickTime;
    private int _firstTick;

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
        if (!IsMe)
        {
            if (_nextTick == _prevTick)
                return;

            var lastDeltaTickLength = (_nextTick - _prevTick) * Time.fixedDeltaTime;
            var lastTickTime = _firstTickTime + (_nextTick - _firstTick) * Time.fixedDeltaTime;
            var timeSinceLastTick = Time.time - lastTickTime;
            transform.position = Vector3.LerpUnclamped(_prevPosition, _nextPosition, 1 + timeSinceLastTick / lastDeltaTickLength);
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

        if (_firstTick == 0)
        {
            _firstTick = tick;
            _firstTickTime = Time.time;
        }

        _prevPosition = _nextPosition;
        _prevTick = _nextTick;

        _nextPosition = position;
        _nextTick = tick;

    }
}
