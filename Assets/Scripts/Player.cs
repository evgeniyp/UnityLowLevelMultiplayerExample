using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool IsMe;
    public Client Client;
    public float Speed = 500f;

    private Vector3 _prevPosition;
    private int _prevTick;

    private Vector3 _nextPosition;
    private int _nextTick;
    private float _nextTickTime;

    void Start()
    {
        _prevPosition = transform.position;
        _nextPosition = transform.position;
    }

    void Update()
    {
        if (!IsMe)
        {
            var lastFrame = (_nextTick - _prevTick) * Time.fixedDeltaTime;
            var thisFrame = Time.time - _nextTickTime;
            transform.position = Vector3.LerpUnclamped(_prevPosition, _nextPosition, 1 + thisFrame / lastFrame);
        }
        else
        {
            var movementVector = new Vector3()
            {
                x = Input.GetAxis("Horizontal") * Time.deltaTime * Speed,
                y = Input.GetAxis("Vertical") * Time.deltaTime * Speed
            };
            transform.position += movementVector;
        }
    }

    void FixedUpdate()
    {
        if (!IsMe) return;

        Client.SendPosition(transform.position);
    }

    public void SetPosition(int tick, Vector3 position)
    {
        if (tick <= _prevTick)
            return;

        _prevPosition = _nextPosition;
        _prevTick = _nextTick;

        _nextPosition = position;
        _nextTick = tick;
        _nextTickTime = Time.time;
    }
}
